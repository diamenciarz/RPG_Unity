using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    public float defaultPlayerSpeed = 4f;
    [SerializeField] bool rotationSlowdown = false;


    private Rigidbody2D myRigidbody2D;
    DashAbility dashAbility;

    private float playerSpeed;
    private const float PLAYER_SPRITE_ROTATION = -90;

    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = defaultPlayerSpeed;
        myRigidbody2D = GetComponent<Rigidbody2D>();
        dashAbility = GetComponent<DashAbility>();
        UpdatePlayerGameObject();
    }
    public void UpdatePlayerGameObject()
    {
        EventManager.TriggerEvent("SetPlayerGameObject", gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
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
        
        Vector2 newVelocity = (inputVector * playerSpeed) + GetDashVector();

        myRigidbody2D.velocity = newVelocity;
    }
    private Vector3 GetDashVector()
    {
        if (dashAbility)
        {
            return dashAbility.GetDashVector();
        }
        return Vector3.zero;
    }
    private Vector3 GetInputVector()
    {
        return new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
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
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }
    #endregion
}
