using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float defaultPlayerSpeed = 10f;
    [SerializeField] float bushSpeedModifier = 0.4f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] float dashLength = 2f;
    public float dashRange = 3f;
    private float dashDuration = 0.5f;
    [HideInInspector]
    public float dashSpeed;

    private float playerSpeed;
    private bool isDashing;
    private bool canDash = true;

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
        EventManager.TriggerEvent("UpdateDashSnapRange", dashRange);
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
        playerSpeed = defaultPlayerSpeed * StaticDataHolder.GetHighestSlowEffect();
    }
    private void UpdateVelocity()
    {
        Vector3 inputVector = GetInputVector();
        
        Vector2 newVelocity = (inputVector * playerSpeed) + CountDashVector();

        myRigidbody2D.velocity = newVelocity;
    }
    private Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
    }

    #region Dash
    private Vector3 CountDashVector()
    {
        const float rangeMultiplier = 2.6f; //Scales dash range to one map unit
        Vector3 dashVector = dashDirection * rangeMultiplier * dashSpeed * dashLength;
        return dashVector;
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
            EventManager.TriggerEvent("UpdateDashIcon", false);
            StartCoroutine(DashCooldownCoroutine());

            GameObject objectToDashThrough = StaticDataHolder.GetTheClosestDashableObject(transform.position, dashRange);
            DashThroughObject(objectToDashThrough);
        }
    }
    private bool IsDashableObjectInRange()
    {
        GameObject objectToDashThrough = StaticDataHolder.GetTheClosestDashableObject(transform.position, dashRange);
        return objectToDashThrough != null;
    }
    private void DashThroughObject(GameObject dashGO)
    {
        isDashing = true;
        dashDirection = StaticDataHolder.GetDeltaPositionFromToIn2D(gameObject, dashGO).normalized;
        dashCoroutine = StartCoroutine(DashCoroutine());
    }
    private IEnumerator DashCoroutine()
    {
        myAnimator.SetBool("isDashing", true);
        yield return new WaitForSeconds(dashDuration);

        myAnimator.SetBool("isDashing", false);
        isDashing = false;
    }
    private IEnumerator DashCooldownCoroutine()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
        EventManager.TriggerEvent("UpdateDashIcon", true);
    }
    #endregion

    private void RotateTowardsMouseCursor()
    {
        Vector3 mousePosition = StaticDataHolder.GetTranslatedMousePositionIn2D(transform.position);
        Quaternion newRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, mousePosition);

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
