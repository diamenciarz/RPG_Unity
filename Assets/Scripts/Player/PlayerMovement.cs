using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float defaultPlayerSpeed = 10f;
    [SerializeField] float bushSpeedModifier = 0.4f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] float dashDuration = 0.5f;
    [SerializeField] float dashRange = 1.5f;
    [HideInInspector]
    public float dashSpeed;

    private float playerSpeed;
    private int forceMultiplier = 100;
    private bool shouldStopDashImmediately;
    private bool isDashing;
    private bool canDash = true;

    private Vector3 dashVector;
    private Vector3 moveVectorThisFrame;
    private Coroutine dashCoroutine;

    private Rigidbody2D myRigidbody2D;
    private BoxCollider2D myCollider2D;
    public List<GameObject> collidingObjectsList = new List<GameObject>();
    private Animator myAnimator;
    private Animation myAnimation;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = defaultPlayerSpeed;
        myCollider2D = GetComponent<BoxCollider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myAnimation = GetComponent<Animation>();
        UpdatePlayerGameObject();

        StaticDataHolder.AddEntity(gameObject);
    }
    public void UpdatePlayerGameObject()
    {
        EventManager.TriggerEvent("SetPlayerGameObject", gameObject);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //MoveUsingPhysics();
        //BounceOffWalls();
    }
    private void Update()
    {
        AdjustMovementSpeed();

        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        moveVectorThisFrame = ((inputVector * playerSpeed) + dashVector) * Time.deltaTime;

        RotateTowardsMoveVector(moveVectorThisFrame);
        if (!isDashing)
        {
            MoveIfPossibleBy(moveVectorThisFrame);
        }
        if (canDash && !isDashing)
        {
            CheckDashInput();
        }
    }
    private void CheckDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryToDash();
        }
    }
    private void TryToDash()
    {
        GameObject objectToDashThrough = StaticDataHolder.GetCurrentDashObject();
        bool isDashableObjectInRange = objectToDashThrough != null;
        if (isDashableObjectInRange)
        {
            canDash = false;
            StartCoroutine(WaitForDashToCoolDown());
            DashThroughObject(objectToDashThrough);
        }
    }
    private void DashThroughObject(GameObject dashGO)
    {
        isDashing = true;
        Vector3 dashDirection = StaticDataHolder.GetDeltaPositionFromToIn2D(gameObject, dashGO);
        dashCoroutine = StartCoroutine(DashCoroutine(dashDirection));
    }
    private IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        Debug.DrawRay(transform.position, dashDirection, Color.red);
        float totalTime = 0;
        float stepDuration = (1f / 30f); //In seconds
        int stepAmount = Mathf.FloorToInt(dashDuration / stepDuration);
        myAnimator.SetBool("isDashing", true);

        while ((totalTime < dashDuration) && !shouldStopDashImmediately)
        {
            Vector3 moveThisStep = dashDirection.normalized * dashSpeed * dashRange / stepAmount;
            MoveIfPossibleBy(moveThisStep);
            yield return new WaitForSeconds(stepDuration);
            totalTime += stepDuration;
        }

        myAnimator.SetBool("isDashing", false);
        isDashing = false;
    }
    private IEnumerator WaitForDashToCoolDown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private void RotateTowardsMoveVector(Vector3 moveVector)
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, moveVector);
    }
    
    private void MoveIfPossibleBy(Vector3 moveVector)
    {
        RaycastHit2D moveHit2D;
        float moveDistance = moveVector.magnitude;

        bool movedHorizontally = false;
        //Check for collisions on the X-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(moveVector.x, 0), moveDistance, LayerMask.GetMask("Actors", "Obstacles"));
        if (CanMove(moveHit2D))
        {
            movedHorizontally = true;
            transform.position += new Vector3(moveVector.x, 0, 0);
        }

        //Check for collisions on theY-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(0, moveVector.y), moveDistance, LayerMask.GetMask("Actors", "Obstacles"));
        if (CanMove(moveHit2D))
        {
            transform.position += new Vector3(0, moveVector.y, 0);
        }
        else
        {
            if (!movedHorizontally)
            {
                shouldStopDashImmediately = true;
            }
        }
    }
    private bool CanMove(RaycastHit2D moveHit2D)
    {
        bool canMove = moveHit2D.collider == null || moveHit2D.collider.tag == "Dashable";

        if (canMove)
        {
            shouldStopDashImmediately = false;
        }
        return canMove;
    }
    private void AdjustMovementSpeed()
    {
        if (StaticDataHolder.IsCollidingWithABush())
        {
            playerSpeed = defaultPlayerSpeed * bushSpeedModifier;
        }
        else
        {
            playerSpeed = defaultPlayerSpeed;
        }
    }


    //Collision handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
         StaticDataHolder.AddCollidingObject(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        StaticDataHolder.RemoveCollidingObject(collision.gameObject);
    }

    //Get Variables
    public bool GetCanDash()
    {
        return canDash;
    }

    //Unused
    private void MoveUsingPhysics()
    {
        myRigidbody2D.AddForce(new Vector2(moveVectorThisFrame.x * Time.deltaTime * playerSpeed * forceMultiplier, moveVectorThisFrame.y * Time.deltaTime * playerSpeed * forceMultiplier));
    }
    private void BounceOffWalls()
    {
        RaycastHit2D moveHit2D;
        float moveDistanceThisFrame = moveVectorThisFrame.magnitude;

        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(moveVectorThisFrame.x, 0), moveDistanceThisFrame, LayerMask.GetMask("Actors", "Obstacles"));
        if (moveHit2D.collider != null)
        {
            myRigidbody2D.velocity = new Vector2(-myRigidbody2D.velocity.x, myRigidbody2D.velocity.y);
        }
        if (moveHit2D.collider != null)
        {
            myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, -myRigidbody2D.velocity.y);
        }
    }
}
