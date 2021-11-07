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
    public bool isDashing;
    public bool canDash = true;

    private Vector3 dashDirection;
    private Vector3 moveVectorThisFrame;
    private Coroutine dashCoroutine;

    private Rigidbody2D myRigidbody2D;
    private BoxCollider2D myCollider2D;
    private Animator myAnimator;
    private Animation myAnimation;
    private const float PLAYER_SPRITE_ROTATION = -90;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = defaultPlayerSpeed;
        myCollider2D = GetComponent<BoxCollider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myAnimation = GetComponent<Animation>();
        UpdatePlayerGameObject();
    }
    public void UpdatePlayerGameObject()
    {
        EventManager.TriggerEvent("SetPlayerGameObject", gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
        CheckDash();

        RotateTowardsMouseCursor();
    }
    void FixedUpdate()
    {
        DoMove();
    }
    private void DoMove()
    {
        AdjustMovementSpeed();
        UpdateVelocity();
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
    private void UpdateVelocity()
    {
        Vector3 inputVector = GetInputVector();
        Vector3 dashVector = dashDirection * dashSpeed;
        Vector2 newVelocity = (inputVector * playerSpeed) + dashVector;

        myRigidbody2D.velocity = newVelocity;
    }
    private Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
    }
    private void CheckDash()
    {
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
        if (IsDashableObjectInRange())
        {
            canDash = false;
            StartCoroutine(DashCooldownCoroutine());

            GameObject objectToDashThrough = StaticDataHolder.GetCurrentDashObject();
            DashThroughObject(objectToDashThrough);
        }
    }
    private bool IsDashableObjectInRange()
    {
        GameObject objectToDashThrough = StaticDataHolder.GetCurrentDashObject();
        return objectToDashThrough != null;
    }
    private void DashThroughObject(GameObject dashGO)
    {
        isDashing = true;
        Vector3 dashDirection = StaticDataHolder.GetDeltaPositionFromToIn2D(gameObject, dashGO);
        dashCoroutine = StartCoroutine(DashCoroutine(dashDirection));
    }
    private IEnumerator DashCoroutine(Vector3 dashDirection)
    {
        myAnimator.SetBool("isDashing", true);
        yield return new WaitForSeconds(dashDuration);

        /*
        float totalTime = 0;
        float stepDuration = (1f / 30f); //In seconds
        int stepAmount = Mathf.FloorToInt(dashDuration / stepDuration);
        while ((totalTime < dashDuration) && !shouldStopDashImmediately)
        {
            this.dashDirection = dashDirection.normalized * dashSpeed * dashRange / stepAmount;
            yield return new WaitForSeconds(stepDuration);
            totalTime += stepDuration;
        }
        */
        myAnimator.SetBool("isDashing", false);
        isDashing = false;
    }
    private IEnumerator DashCooldownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    private void RotateTowardsMouseCursor()
    {
        Vector3 mousePosition = StaticDataHolder.GetTranslatedMousePosition(transform.position);
        Quaternion newRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, mousePosition);
        Debug.DrawRay(transform.position, StaticDataHolder.GetDirectionVector(2, newRotation.eulerAngles.z),Color.red, 0.5f);
        newRotation *= Quaternion.Euler(0, 0, PLAYER_SPRITE_ROTATION);
        transform.rotation = newRotation;
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
    private void RotateTowardsMoveVector(Vector3 moveVector)
    {
        if (moveVector.magnitude != 0)
        {
            if (moveVector.normalized == Vector3.down)
            {
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                transform.rotation = Quaternion.FromToRotation(Vector3.up, moveVector);
            }
            /*
            Quaternion newRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, transform.position + moveVector);
            newRotation *= Quaternion.Euler(0, 0, PLAYER_SPRITE_ROTATION);
            transform.rotation = newRotation;
            */
        }
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
