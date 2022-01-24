using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMover : MonoBehaviour, IEntityMover
{
    #region Serialization
    [Header("Movement")]
    public float defaultSpeed = 4f;
    [SerializeField] bool isPlayer = false;
    [Tooltip("How slippery the movement is. 1 for no slipping, 0 for walking on ice")]
    [SerializeField] [Range(0, 1)] float slipFactor;
    [Header("Rotation")]
    [Tooltip("Whether this unit can turn towards target immediately")]
    [SerializeField] bool rotateImmediately = true;
    [Tooltip("The deg/sec rotation speed, if 'rotateImmediately' is set to false.")]
    [SerializeField] float turnSpeed = 240;
    [Header("Rotation settings")]
    [Tooltip("Whether walking backwards slows the unit down")]
    [SerializeField] bool rotationSlowdown = false;
    [Tooltip("The highest percentage that the speed can be decreased by")]
    [SerializeField] float MAX_SLOWDOWN = 0.4f;
    [Tooltip("Determines, whether this unit should look at, where the mouse cursor points")]
    [SerializeField] bool lookingAtMouse;
    [SerializeField] GameObject lookAt;
    #endregion

    #region Private variables
    private GameObject lookTarget;
    private GameObject rotator;
    private static List<GameObject> collidingObjects = new List<GameObject>();
    private Rigidbody2D myRigidbody2D;
    DashAbility dashAbility;
    private Coroutine regainControlCoroutine;

    private bool hasControl;
    private Vector2 inputVector;
    private float currentSpeed;
    private const float DELTA_SPRITE_ROTATION = -90;
    #endregion

    #region Startup
    void Start()
    {
        hasControl = true;
        lookTarget = new GameObject();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        dashAbility = GetComponent<DashAbility>();
        rotator = transform.GetChild(0).gameObject;

        lookTarget.name = gameObject.name + "LookTarget";
        currentSpeed = defaultSpeed;

        CheckIfIsPlayer();
    }
    private void CheckIfIsPlayer()
    {
        if (isPlayer)
        {
            SetAsPlayerGameObject();
        }
    }
    public void SetAsPlayerGameObject()
    {
        EventManager.TriggerEvent("SetPlayerGameObject", gameObject);
    }
    #endregion

    private void Update()
    {
        UpdateTarget();
        LookAtPosition(lookTarget.transform.position);
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

    #region Adjust movement speed
    private void AdjustMovementSpeed()
    {
        currentSpeed = defaultSpeed * GetHighestSlowEffect();
        AdjustByMSModifier();
    }
    private void AdjustByMSModifier()
    {
        if (rotationSlowdown)
        {
            currentSpeed *= CountMSModifier();
        }
    }
    /// <summary>
    /// The player walks slower, when not facing the movement direction. Actually it caps at min 60% of total speed
    /// </summary>
    /// <returns></returns>
    private float CountMSModifier()
    {
        float deltaAngle = GetMoveRotAngle();
        //Don't modify the speed, if facing nearly forward
        deltaAngle = ModifyMSAngle(deltaAngle);
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
    
    private void UpdateVelocity()
    {
        myRigidbody2D.velocity = CountSummedUpVelocity();
    }
    private Vector2 CountSummedUpVelocity()
    {
        return (inputVector * currentSpeed) + GetDashVector();
    }
    private Vector2 GetDashVector()
    {
        if (dashAbility)
        {
            return dashAbility.GetDashVector();
        }
        return Vector2.zero;
    }

    #region Push
    private void HandlePush(Vector2 force)
    {
        myRigidbody2D.AddForce(force, ForceMode2D.Force);
        hasControl = false;
        SetRegainControlTimer();
    }
    private void SetRegainControlTimer()
    {
        if (regainControlCoroutine == null)
        {
            regainControlCoroutine = StartCoroutine(RegainControlCoroutine());
        }
    }
    private IEnumerator RegainControlCoroutine()
    {
        //Waits, until the velocity of this unit drops down to a controllable level
        yield return new WaitUntil(() => GetVelocity() < defaultSpeed);
        hasControl = true;
        regainControlCoroutine = null;
    } 
    #endregion
    #endregion

    #region Collisions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AddCollidingObject(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveCollidingObject(collision.gameObject);
    }
    private void AddCollidingObject(GameObject obj)
    {
        collidingObjects.Add(obj);
    }
    private void RemoveCollidingObject(GameObject obj)
    {
        if (collidingObjects.Contains(obj))
        {
            collidingObjects.Remove(obj);
        }
    }
    public static float GetHighestSlowEffect()
    {
        if (collidingObjects.Count != 0)
        {
            float maxSlowEffect = 1;
            foreach (GameObject item in collidingObjects)
            {
                EntityProperties entityProperties = item.GetComponent<EntityProperties>();
                if (entityProperties)
                {
                    if (entityProperties.slowingEffect < maxSlowEffect)
                    {
                        maxSlowEffect = entityProperties.slowingEffect;
                    }
                }
            }
            return maxSlowEffect;
        }
        return 1;
    }
    #endregion

    #region Rotation
    private void UpdateTarget()
    {
        if (lookingAtMouse)
        {
            lookTarget.transform.position = HelperMethods.TranslatedMousePosition(transform.position);
        }
        else
        {
            if (lookAt != null)
            {
                lookTarget.transform.position = lookAt.transform.position;
            }
        }
    }
    private void LookAtPosition(Vector3 pos)
    {
        Quaternion newRot = CountNewRotation(pos);
        rotator.transform.rotation = newRot;
    }
    private Quaternion CountNewRotation(Vector3 pos)
    {
        if (rotateImmediately)
        {
            return CountRotationTowardsPosition(pos);
        }
        else
        {
            return CountOneStepTowardsPosition(pos);
        }
    }
    private Quaternion CountOneStepTowardsPosition(Vector3 pos)
    {
        Quaternion targetRotation = CountRotationTowardsPosition(pos);
        Quaternion deltaRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        return deltaRotation;
    }
    private Quaternion CountRotationTowardsPosition(Vector3 pos)
    {
        Quaternion newRotation = HelperMethods.DeltaPositionRotation(transform.position, pos);
        newRotation *= Quaternion.Euler(0, 0, DELTA_SPRITE_ROTATION);
        return newRotation;
    }
    #endregion

    #region Accessor / mutator methods
    public Quaternion GetRotation()
    {
        return transform.rotation;
    }
    public float GetSpeed()
    {
        return currentSpeed;
    }
    public float GetVelocity()
    {
        return myRigidbody2D.velocity.magnitude;
    }
    //Mutator
    public void SetInputVector(Vector2 newInputVector)
    {
        inputVector = newInputVector;
    }
    public void SetLookAtObject(GameObject obj)
    {
        lookAt = obj;
    }
    public void SetLookingAtMouse(bool setting)
    {
        lookingAtMouse = setting;
    }
    public void Push(Vector2 force)
    {
        HandlePush(force);
    }
    #endregion
}
