using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementController : TeamUpdater
{
    public GameObject lastTargetPosition;
    [Header("Instances")]
    public GameObject target;
    [Header("Instances")]
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 120;

    private Pathfinding.AIDestinationSetter aiDestinationSetter;

    private float POSITION_REFRESH_RATE = 0.25f; // Cooldown to refresh path to target

    private void Start()
    {
        lastTargetPosition = new GameObject();
        lastTargetPosition.transform.position = transform.position;
        aiDestinationSetter = GetComponent<Pathfinding.AIDestinationSetter>();
    }
    private void Update()
    {
        RotateTowardsTarget();
        StartCoroutine(ITargetPositionUpdater(POSITION_REFRESH_RATE));
    }
    
    #region Pathing
    private IEnumerator ITargetPositionUpdater(float refreshRate)
    {
        UpdateLastTargetPosition();
        yield return new WaitForSeconds(refreshRate);
    }
    private void UpdateLastTargetPosition()
    {
        if (HelperMethods.CanSeeTargetDirectly(transform.position, target))
        {
            lastTargetPosition.transform.position = target.transform.position;
        }
        else
        {
            GameObject newTarget = StaticDataHolder.GetClosestEnemyInSight(transform.position, team);
            if (newTarget)
            {
                target = newTarget;
                aiDestinationSetter.target = newTarget.transform;
            }
        }
    }
    #endregion
    
    #region Rotation
    private void RotateTowardsTarget()
    {
        Quaternion targetRotation = HelperMethods.DeltaPositionRotation(transform.position, lastTargetPosition.transform.position);
        float angleThisFrame = rotationSpeed * Time.deltaTime;

        Quaternion.RotateTowards(transform.rotation, targetRotation, angleThisFrame);
    }
    #endregion
}
