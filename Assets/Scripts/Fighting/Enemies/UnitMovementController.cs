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
    [SerializeField] float moveSpeed = 3;

    private Pathfinding.AIDestinationSetter aiDestinationSetter;
    private Pathfinding.AIBase aiBase;
    private float currentMoveSpeed;

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
        //Setup instances
        aiDestinationSetter = GetComponent<Pathfinding.AIDestinationSetter>();
        aiBase = GetComponent<Pathfinding.AIBase>();
        //Setup target position
        lastTargetPosition = new GameObject();
        lastTargetPosition.transform.position = transform.position;
        //Movement speed
        currentMoveSpeed = moveSpeed;
        SetMovementSpeed(currentMoveSpeed);
    }
    #endregion

    #region Movement
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
    private void SetMovementSpeed(float newSpeed)
    {
        aiBase.maxSpeed = newSpeed;
    }
    #endregion

}
