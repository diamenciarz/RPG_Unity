using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{
    [SerializeField] GameObject objectMissingIconGameObject;
    public bool hurtsPlayer;

    [Header("Rocket Flight Settings")]
    public float startingRocketSpeed = 2f;
    [SerializeField] float maxRocketSpeed;
    [SerializeField] float speedChangeRatePerSecond = 1f;
    public float rocketRotationSpeed; //Degrees per second

    [Header("Explosion Settings")]
    public float timeToExpire;

    //Private variables
    private float currentRocketSpeed;
    private GameObject targetGameObject;
    GameObject newBullet;

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
        if (startingRocketSpeed == -1)
        {
            currentRocketSpeed = maxRocketSpeed;
        }
        else
        {
            currentRocketSpeed = startingRocketSpeed;
        }
    }
    protected override void Update()
    {
        base.Update();
        CheckForTarget();
        ChangeSpeedTowardsTargetSpeed();
        if (targetGameObject != null)
        {
            RotateTowardsTarget();
        }
    }
    protected void CheckForTarget()
    {
        if (myTeam != 0)
        {
            if (targetGameObject == null)
            {
                targetGameObject = StaticDataHolder.GetTheNearestEnemy(transform.position, myTeam);
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
    
    private void RotateTowardsTarget()
    {
        Quaternion newRocketRotation = StaticDataHolder.GetRotationFromToIn2D(transform.position, targetGameObject.transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRocketRotation, rocketRotationSpeed * Time.deltaTime);
    }
    public void SetTarget(GameObject target)
    {
        targetGameObject = target;
    }
    
}
