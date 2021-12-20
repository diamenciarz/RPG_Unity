using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float defaultPlayerSpeed = 10f;
    [SerializeField] float dashCooldown = 1f;
    [SerializeField] float dashLength = 2f;
    public float dashRange = 3f;

    [SerializeField] bool rotationSlowdown = false;
    private float dashDuration = 0.5f;
    [HideInInspector]
    public float dashSpeed;

    private float playerSpeed;
    private bool isDashing;
    private bool canDash = true;

    private Vector3 dashDirection;
    private Coroutine dashCoroutine;

    private Rigidbody2D myRigidbody2D;
    private BoxCollider2D myCollider2D;
    private Animator myAnimator;
    private const float PLAYER_SPRITE_ROTATION = -90;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = defaultPlayerSpeed;
        myCollider2D = GetComponent<BoxCollider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
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

    #region Movement
    private void DoMove()
    {
        AdjustMovementSpeed();
        UpdateVelocity();
    }
    private void AdjustMovementSpeed()
    {
        playerSpeed = defaultPlayerSpeed * StaticDataHolder.GetHighestSlowEffect();
        if (rotationSlowdown)
        {
            playerSpeed *= countMSModifier();
        }
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
    #endregion

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
        dashDirection = HelperMethods.DeltaPosition(gameObject, dashGO).normalized;
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

    #region Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        StaticDataHolder.AddCollidingObject(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        StaticDataHolder.RemoveCollidingObject(collision.gameObject);
    }
    #endregion

    #region Rotation
    private void RotateTowardsMouseCursor()
    {
        Vector3 mousePosition = HelperMethods.TranslatedMousePosition(transform.position);
        Quaternion newRotation = HelperMethods.DeltaPositionRotation(transform.position, mousePosition);

        newRotation *= Quaternion.Euler(0, 0, PLAYER_SPRITE_ROTATION);
        transform.rotation = newRotation;
    }

    /// <summary>
    /// The player walks slower, when not facing the movement direction. Actually it caps at min 60% of total speed
    /// </summary>
    /// <returns></returns>
    private float countMSModifier()
    {
        float deltaAngle = GetMoveRotAngle();
        //Don't modify the speed, if facing nearly forward
        deltaAngle = ModifyMSAngle(deltaAngle);
        float MAX_SLOWDOWN = 0.4f;
        // "deltaAngle / 2" to get the sin <0;1>
        float speedModifier = 1 - (MAX_SLOWDOWN * Mathf.Sin(deltaAngle / 2 * Mathf.Deg2Rad));
        return speedModifier;
    }
    /// <summary>
    /// Counts the angle between the move vector and the forward foration of the sprite
    /// </summary>
    /// <returns></returns>
    private float GetMoveRotAngle()
    {
        Vector3 inputVector = GetInputVector();
        Vector3 directionVector = HelperMethods.DirectionVectorNormalized(GetRotation().eulerAngles.z);

        //This angle is in degrees
        float deltaAngle = Vector3.SignedAngle(inputVector, directionVector, Vector3.forward);
        return Mathf.Abs(deltaAngle);
    }
    /// <summary>
    /// A cone at the front has no effect
    /// </summary>
    /// <param name="deltaAngle"></param>
    /// <returns></returns>
    private float ModifyMSAngle(float deltaAngle)
    {
        float CONE = 45f; // Must be in range <0,180)
        if (deltaAngle < CONE)
        {
            deltaAngle = 0;
        }
        else
        {
            deltaAngle -= CONE;
        }
        float scaledAngle = deltaAngle * 180 / (180 - CONE);
        return scaledAngle;
    }
    #endregion

    #region Accessor methods
    public bool GetCanDash()
    {
        return canDash;
    }
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }
    #endregion
}
