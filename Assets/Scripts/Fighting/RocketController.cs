using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{
    [SerializeField] GameObject objectMissingIconGameObject;
    public bool hurtsPlayer;

    [Header("Rocket Flight Settings")]
    [SerializeField] float maxRocketSpeed;
    [SerializeField] float speedChangeRatePerSecond = 1f;
    public float rocketRotationSpeed; //Degrees per second
    public float spriteDeltaRotation = -90;

    [Header("Explosion Settings")]
    public float timeToExpire;

    //Private variables
    private float currentRocketSpeed;
    private GameObject targetGameObject;

    protected void Start()
    {
        CreateMiaIcon();
        SetupStartingSpeed();
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
    protected override void Update()
    {
        base.Update();
        CheckForTarget();
        ChangeSpeedTowardsTargetSpeed();
        if (targetGameObject != null)
        {
            TurnTowardsTarget();
        }
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
    private void ChangeSpeedTowardsTargetSpeed()
    {
        if (currentRocketSpeed != maxRocketSpeed)
        {
            currentRocketSpeed = Mathf.MoveTowards(currentRocketSpeed, maxRocketSpeed, speedChangeRatePerSecond * Time.deltaTime);
        }
    }

    private void TurnTowardsTarget()
    {
        Quaternion targetRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, targetGameObject.transform.position) * Quaternion.Euler(0, 0, spriteDeltaRotation);
        Quaternion newRotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rocketRotationSpeed * Time.deltaTime);
        Vector2 newVelocity = StaticDataHolder.GetNormalizedDirectionVector(newRotation.eulerAngles.z) * currentRocketSpeed;

        transform.rotation = newRotation;
        SetVelocityVector(newVelocity);
    }
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
}
