using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementController : TeamUpdater
{
    public GameObject lastTargetPosition;
    [Header("Instances")]
    public GameObject target;
    [Header("Movement")]
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 120;
    [SerializeField] float shootRange = 2;

    private Pathfinding.AIDestinationSetter aiDestinationSetter;

    private float POSITION_REFRESH_RATE = 0.25f; // Cooldown to refresh path to target

    #region Initiation
    private void Start()
    {
        SetupStartingVariables();
        StartCoroutine(ITargetPositionUpdater(POSITION_REFRESH_RATE));
    }
    private void Update()
    {
        RotateTowardsTarget();
    }
    private void SetupStartingVariables()
    {
        lastTargetPosition = new GameObject();
        lastTargetPosition.transform.position = transform.position;
        aiDestinationSetter = GetComponent<Pathfinding.AIDestinationSetter>();
    }
    #endregion
    
    #region Pathing
    private IEnumerator ITargetPositionUpdater(float refreshRate)
    {
        while (true)
        {
            UpdateTargetPosition();
            yield return new WaitForSeconds(refreshRate);
        }
    }
    private void UpdateTargetPosition()
    {
        if (HelperMethods.CanSeeTargetDirectly(transform.position, target))
        {
            SetTargetPositionPointer();
        }
        else
        {
            FindNewTargetInSight();
        }
    }
    private void FindNewTargetInSight()
    {
        GameObject newTarget = StaticDataHolder.GetClosestEnemyInSight(transform.position, team);
        if (newTarget)
        {
            target = newTarget;
            aiDestinationSetter.target = newTarget.transform;
        }
    }
    private void SetTargetPositionPointer()
    {
        if (HelperMethods.Distance(gameObject, target) <= shootRange)
        {
            MovePointerOntoMyself();
        }
        else
        {
            MovePointerOntoTarget();
        }
    }
    private void MovePointerOntoTarget()
    {
        lastTargetPosition.transform.position = target.transform.position;
    }
    private void MovePointerOntoMyself()
    {
        lastTargetPosition.transform.position = transform.position;
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
