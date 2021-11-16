using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{
    [SerializeField] GameObject objectMissingIconGameObject;

    [Header("Rocket Flight Settings")]
    [SerializeField] float maxRocketSpeed;
    [SerializeField] float speedChangeRatePerSecond = 1f;
    public float rocketRotationSpeed; //Degrees per second
    public float spriteDeltaRotation = -90;
    protected float zRotation;

    [Header("Explosion Settings")]
    public float timeToExpire;

    //Private variables
    protected float currentRocketSpeed;
    private GameObject targetGameObject;
    #region Startup
    protected override void Awake()
    {
        base.Awake();
        SetupStartingSpeed();
    }
    protected override void Start()
    {
        base.Start();
        CreateMiaIcon();
    }
    private void CreateMiaIcon()
    {
        if (objectMissingIconGameObject != null)
        {
            GameObject miaGameObject = Instantiate(objectMissingIconGameObject, transform.position, Quaternion.identity);
            miaGameObject.GetComponent<ObjectMissingIcon>().TryFollowThisObject(gameObject);
        }
    }
    private void SetupStartingSpeed()
    {
        if (startingSpeed == -1)
        {
            currentRocketSpeed = maxRocketSpeed;
        }
        else
        {
            currentRocketSpeed = startingSpeed;
        }
    }
    #endregion

    #region Every Frame
    protected override void Update()
    {
        base.Update();
        CheckForTarget();
        TurnTowardsTarget();
        IncreaseSpeed();
        UpdateSpeed();
    }
    protected void CheckForTarget()
    {
        if (team != 0)
        {
            if (targetGameObject == null)
            {
                targetGameObject = StaticDataHolder.GetTheNearestEnemy(transform.position, team);
            }
        }
    }
    private void IncreaseSpeed()
    {
        if (currentRocketSpeed != maxRocketSpeed)
        {
            currentRocketSpeed = Mathf.MoveTowards(currentRocketSpeed, maxRocketSpeed, speedChangeRatePerSecond * Time.deltaTime);
        }
    }
    private void UpdateSpeed()
    {
        Vector2 newVelocity = StaticDataHolder.GetDirectionVectorNormalized(zRotation) * currentRocketSpeed;
        SetVelocityVector(newVelocity);
    }
    private void TurnTowardsTarget()
    {
        if (targetGameObject != null)
        {
            const int DELTA_ROTATION = -90; //I don't understand, why it's necessary. Something is programmed wrong, but this fixes it
            Quaternion targetRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, targetGameObject.transform.position) * Quaternion.Euler(0, 0, DELTA_ROTATION);
            Quaternion newRotation = Quaternion.RotateTowards(transform.rotation * GetRocketSpriteCounterRotation(), targetRotation, rocketRotationSpeed * Time.deltaTime);
            Debug.DrawRay(transform.position, StaticDataHolder.GetFromToVectorIn2D(transform.position, targetGameObject.transform.position), Color.red, 0.1f);
            zRotation = newRotation.eulerAngles.z;

            transform.rotation = newRotation * GetRocketSpriteRotation();
        }
    }
    #endregion

    #region Accessor/Mutator Methods
    public void SetTarget(GameObject target)
    {
        targetGameObject = target;
    }
    public float GetMaxRocketSpeed()
    {
        return maxRocketSpeed;
    }
    public float GetCurrentRocketSpeed()
    {
        return currentRocketSpeed;
    }
    public void SetCurrentRocketSpeed(float newSpeed)
    {
        currentRocketSpeed = newSpeed;
    }
    private Quaternion GetRocketSpriteRotation()
    {
        return Quaternion.Euler(0, 0, spriteDeltaRotation);
    }
    private Quaternion GetRocketSpriteCounterRotation()
    {
        return Quaternion.Euler(0, 0, -spriteDeltaRotation);
    }
    #endregion
}
