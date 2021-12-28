using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementController : TeamUpdater
{
    #region Serializable
    [Header("Movement")]
    [Tooltip("The transform from which to shoot a raycast")]
    [SerializeField] Transform[] eyeTransforms;
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 240;
    [SerializeField] float baseMoveSpeed = 3;
    [Header("Hunting")]
    [Tooltip("How much time after losing the trace, will this unit go back to its original routine")]
    [SerializeField] float loseInterestDelay = 10;
    [SerializeField] float placeTraceDelay = 2;
    [Tooltip("If the target travels further than this distance, start losing its trace")]
    [SerializeField] float smellRange = 2;
    [SerializeField] int traceCount = 3;
    #endregion

    #region Private variables
    //Objects
    private Pathfinding.AIDestinationSetter aiDestinationSetter;
    private Pathfinding.AIPath aiPath;
    private AutomaticGunRotator gunRotator;
    public List<Vector3> lastTargetPositions = new List<Vector3>();
    private GameObject lastTargetPosition;
    private GameObject target;
    //Movement
    private float currentMoveSpeed;
    VisualDetector[] inSightDetectors;
    [Tooltip("If this is set to true, the unit will start following its target")]
    private bool isAnyTargetInSight;
    VisualDetector[] inRangeDetectors;
    [Tooltip("If this is set to true, the unit will hold its position")]
    private bool isAnyTargetInRange;

    #region Hunting
    //Booleans
    public bool canSeeTarget;
    public bool canSeeLastTargetPosition;
    private bool startedHunting;
    //Other values
    private float POSITION_REFRESH_RATE = 0.25f; // Cooldown to refresh path to target
    private float targetLastSeenTime; // Will be useful for shooting
    private float lastTraceLeftTime;
    public int totalTracesLeft;
    #endregion
    #endregion

    #region Initialization
    private void Start()
    {
        SetupStartingVariables();
        StartCoroutine(ITargetPositionUpdater(POSITION_REFRESH_RATE));
    }
    private void SetupStartingVariables()
    {
        //Setup instances
        aiDestinationSetter = GetComponent<Pathfinding.AIDestinationSetter>();
        aiPath = GetComponent<Pathfinding.AIPath>();
        //Setup rotation
        aiPath.updateRotation = false;
        aiPath.enableRotation = true;
        //Setup target position
        lastTargetPosition = new GameObject();
        lastTargetPosition.transform.position = transform.position;
        aiDestinationSetter.target = lastTargetPosition.transform;
        targetLastSeenTime = Time.time;
        lastTraceLeftTime = Time.time;
        //Movement speed
        currentMoveSpeed = baseMoveSpeed;
        SetMovementSpeed(currentMoveSpeed);
        //Shooting
        gunRotator = GetComponent<AutomaticGunRotator>();
    }
    #endregion

    #region Update
    private void Update()
    {
        if (target)
        {
            DoChecks();
            UpdateTraces();
            UpdateMovement();
        }
        else
        {
            //Wander around
        }
        Rotate();
    }
    #region Checks
    private void DoChecks()
    {
        if (eyeTransforms.Length > 0)
        {
            CheckIfCanSeeTarget();
            CheckIfCanSeeLastTargetPosition();
            CheckIfIsInRange();
            CheckIfIsInSight();
        }
        else
        {
            Debug.LogError("Unit has no eyes");
        }
    }
    //Can see
    private void CheckIfCanSeeTarget()
    {

        canSeeTarget = CanSee(target);
        if (canSeeTarget)
        {
            targetLastSeenTime = Time.time;
        }
    }
    private void CheckIfCanSeeLastTargetPosition()
    {
        canSeeLastTargetPosition = CanSee(lastTargetPosition.transform.position);
    }
    private bool CanSee(Vector3 pos)
    {
        foreach (Transform eye in eyeTransforms)
        {
            if (!HelperMethods.CanSeeDirectly(eye.position, pos))
            {
                return false;
            }
        }
        return true;
    }
    private bool CanSee(GameObject obj)
    {
        foreach (Transform eye in eyeTransforms)
        {
            if (!HelperMethods.CanSeeDirectly(eye.position, obj))
            {
                return false;
            }
        }
        return true;
    }
    //In range
    private void CheckIfIsInRange()
    {
        isAnyTargetInRange = IsAnyTargetInRange();
    }
    private bool IsAnyTargetInRange()
    {
        if (inRangeDetectors.Length == 0)
        {
            Debug.LogError("This unit has no range detectors");
            return false;
        }
        foreach (VisualDetector detector in inRangeDetectors)
        {
            if (detector.CanSeeTargets())
            {
                return true;
            }
        }
        return false;
    }
    //In sight
    private void CheckIfIsInSight()
    {
        isAnyTargetInRange = IsAnyTargetInSight();
    }
    private bool IsAnyTargetInSight()
    {
        if (inSightDetectors.Length == 0)
        {
            Debug.LogError("This unit has no sight detectors");
            return false;
        }
        foreach (VisualDetector detector in inSightDetectors)
        {
            if (detector.CanSeeTargets())
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #endregion

    #region Movement
    #region Pathing
    #region New target finding
    private IEnumerator ITargetPositionUpdater(float refreshRate)
    {
        while (true)
        {
            UpdateTarget();
            yield return new WaitForSeconds(refreshRate);
        }
    }
    private void UpdateTarget()
    {
        if (!canSeeTarget)
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
        }
    }
    #endregion

    #region Trace following
    private void UpdateMovement()
    {
        if (canSeeTarget && isAnyTargetInRange)
        {
            MovePointerOntoMyself();
        }
        else
        {
            FollowTrace();
        }
    }
    private void MovePointerOntoMyself()
    {
        lastTargetPosition.transform.position = transform.position;
    }

    private void FollowTrace()
    {
        if (startedHunting == false)
        {
            FollowFirstTrace();
            return;
        }
        CheckFresherTraces();
        CheckNextTrace();
    }
    private void FollowFirstTrace()
    {
        if (lastTargetPositions.Count > 0)
        {
            startedHunting = true;
            lastTargetPosition.transform.position = lastTargetPositions[0];
        }
    }
    private void CheckNextTrace()
    {
        Vector3 deltaPositionToTrace = transform.position - lastTargetPositions[0];
        float stopDistance = aiPath.endReachedDistance * 2;
        bool isClose = deltaPositionToTrace.magnitude < stopDistance; //potentially switch to can see last target position
        if (isClose)
        {
            if (lastTargetPositions.Count > 1)
            {
                lastTargetPositions.RemoveAt(0);
                lastTargetPosition.transform.position = lastTargetPositions[0];
            }
        }
    }
    private void CheckFresherTraces()
    {
        if (lastTargetPositions.Count > 1)
        {
            int traceIndex = GetTheFreshestTraceInSight();
            bool fresherTraceFound = traceIndex >= 0;
            if (fresherTraceFound)
            {
                Vector3 newPosition = lastTargetPositions[traceIndex];
                DeleteOldTraces(traceIndex);
                lastTargetPosition.transform.position = newPosition;
            }
        }
    }
    private int GetTheFreshestTraceInSight()
    {
        int highestIndex = -1;
        for (int i = 0; i < lastTargetPositions.Count; i++)
        {
            Vector3 pos = lastTargetPositions[i];
            if (HelperMethods.CanSeeDirectly(transform.position, pos))
            {
                highestIndex = i;
            }
        }
        return highestIndex;
    }
    private void DeleteOldTraces(int upToIndex)
    {
        if (upToIndex >= 0)
        {
            for (int i = 0; i < upToIndex; i++)
            {
                lastTargetPositions.RemoveAt(i);
            }
        }
    }
    #endregion

    #region Trace placement
    private void UpdateTraces()
    {
        if (canSeeTarget)
        {
            ResetLastPlayerPositions();
        }
        else
        {
            if (CanPlaceNextTrace())
            {
                AddTargetPosition();
            }
        }
    }
    private void ResetLastPlayerPositions()
    {
        startedHunting = false;
        totalTracesLeft = traceCount;
        lastTargetPositions.Clear();
        lastTargetPositions.Add(target.transform.position);
    }
    private void AddTargetPosition()
    {
        lastTraceLeftTime = Time.time;
        lastTargetPositions.Add(target.transform.position);
        if (IsOutsideOfSmellRange())
        {
            totalTracesLeft--;
        }
    }
    private bool IsOutsideOfSmellRange()
    {
        Vector3 deltaPosition = lastTargetPositions[lastTargetPositions.Count - 1] - lastTargetPositions[lastTargetPositions.Count - 2];
        float distance = deltaPosition.magnitude;
        if (distance > smellRange)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private bool CanPlaceNextTrace()
    {
        if (Time.time - lastTraceLeftTime > placeTraceDelay)
        {
            if (totalTracesLeft > 0)
            {
                return true;
            }
        }
        return false;
    }
    #endregion
    #endregion

    #region Rotation
    private void Rotate()
    {
        Vector3 lookAt = CountLookAtPosition();
        Quaternion targetRotation = HelperMethods.DeltaPositionRotation(transform.position, lookAt);
        float angleThisFrame = rotationSpeed * Time.deltaTime;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, angleThisFrame);
    }
    private Vector3 CountLookAtPosition()
    {
        if (canSeeTarget && target != null)
        {
            return target.transform.position;
        }
        else
        {
            return lastTargetPosition.transform.position;
        }
    }
    #endregion
    #endregion

    #region Accessor / mutator methods
    private void SetMovementSpeed(float newSpeed)
    {
        aiPath.maxSpeed = newSpeed;
    }
    #endregion
}
