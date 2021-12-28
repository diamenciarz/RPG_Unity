using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMovementController : TeamUpdater, IOnDamageDealt
{
    #region Serializable
    [Header("Movement")]
    [Tooltip("The transform from which to shoot a raycast")]
    [SerializeField] Transform[] eyeTransforms;
    [Tooltip("A list of detectors that act as this unit's vision area")]
    [SerializeField] VisualDetector[] sightDetectors;
    [Tooltip("Degrees per second")]
    public float rotationSpeed = 240;
    [SerializeField] float baseMoveSpeed = 3;
    [Header("Hunting")]
    [Tooltip("A list of detectors that decide, if this unit should hold its position")]
    [SerializeField] VisualDetector[] rangeDetectors;
    [Tooltip("How much time after losing the trace, will this unit go back to its original routine")]
    [SerializeField] float loseInterestDelay = 10; // Will be useful for wandering
    [SerializeField] float placeTraceDelay = 2;
    [Tooltip("If the target travels further than this distance, start losing its trace")]
    [SerializeField] float smellRange = 2;
    [SerializeField] int traceCount = 3;
    #endregion

    #region Private variables
    //Objects
    private Pathfinding.AIDestinationSetter aiDestinationSetter;
    private Pathfinding.AIPath aiPath;
    public List<Vector3> lastTargetPositions = new List<Vector3>();
    private GameObject lastTargetPosition;
    public GameObject target;
    //Movement
    private float currentMoveSpeed;
    [Tooltip("If this is set to true, the unit will start following its target")]
    public bool isTargetInSight;
    [Tooltip("If this is set to true, the unit will hold its position")]
    public bool isTargetInRange;

    #region Hunting
    //Booleans
    public bool canShootTarget;
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
            CheckIfCanShootTarget();
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
    private void CheckIfCanShootTarget()
    {
        canShootTarget = CanSee(target);
        if (canShootTarget)
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
        isTargetInRange = IsTargetInRange();
    }
    private bool IsTargetInRange()
    {
        if (rangeDetectors.Length == 0)
        {
            Debug.LogError("This unit has no range detectors");
            return false;
        }
        foreach (VisualDetector detector in rangeDetectors)
        {
            if (detector.CanSeeTarget(target))
            {
                return true;
            }
        }
        return false;
    }
    //In sight
    private void CheckIfIsInSight()
    {
        isTargetInSight = IsTargetInSight();
    }
    private bool IsTargetInSight()
    {
        if (sightDetectors.Length == 0)
        {
            Debug.LogError("This unit has no sight detectors");
            return false;
        }
        foreach (VisualDetector detector in sightDetectors)
        {
            if (detector.CanSeeTarget(target))
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
        if (!canShootTarget)
        {
            FindNewTargetInSight();
        }
    }
    private void FindNewTargetInSight()
    {
        List<GameObject> targetList = GetDetectedTargets();
        GameObject newTarget = StaticDataHolder.GetClosestObject(targetList, transform.position);
        SetTarget(newTarget);
    }
    private List<GameObject> GetDetectedTargets()
    {
        List<GameObject> detectedTargets = new List<GameObject>();
        foreach (VisualDetector detector in sightDetectors)
        {
            GameObject target = detector.GetClosestTarget();
            if (target)
            {
                detectedTargets.Add(target);
            }
        }
        return detectedTargets;
    }
    #endregion

    #region Trace following
    private void UpdateMovement()
    {
        if (canShootTarget && isTargetInRange)
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
        float distanceToTrace = HelperMethods.Distance(transform.position, lastTargetPositions[0]);
        float stopDistance = aiPath.endReachedDistance * 2f;
        Debug.Log("Distance to trace: " + distanceToTrace + " stop distance " + stopDistance);
        bool isClose = distanceToTrace < stopDistance;
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
            bool fresherTraceFound = traceIndex > 0;
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
            if (CanSeeTrace(pos))
            {
                highestIndex = i;
            }
        }
        return highestIndex;
    }
    private bool CanSeeTrace(Vector3 tracePosition)
    {
        if (sightDetectors.Length == 0)
        {
            Debug.LogError("This unit has no sight detectors");
            return false;
        }
        foreach (VisualDetector detector in sightDetectors)
        {
            if (detector.CanSeePosition(tracePosition))
            {
                return true;
            }
        }
        return false;
    }
    private void DeleteOldTraces(int upToIndex)
    {
        if (upToIndex > 0)
        {
            for (int i = 0; i < upToIndex; i++)
            {
                if (lastTargetPositions.Count > i)
                {
                    lastTargetPositions.RemoveAt(i);
                }
                else
                {
                    Debug.Log("Tried to delete from a position of: " + i + " which is out of range!");
                }
            }
        }
    }

    #region On hit
    /// <summary>
    /// If the current target shoots the unit from behing - it will immediately reveal its position
    /// </summary>
    /// <param name="hitBy"></param>
    public void HitBy(GameObject hitBy)
    {
        if (target)
        {
            if (hitBy == target)
            {
                RevealTargetPosition();
            }
        }
        else
        {
            SetTarget(hitBy);
            RevealTargetPosition();
        }
    }
    private void RevealTargetPosition()
    {
        AddTargetPosition();
        JumpToTheNewestTrace();
    }
    private void JumpToTheNewestTrace()
    {
        if (lastTargetPositions.Count > 1)
        {
            DeleteOldTraces(lastTargetPositions.Count - 2);
            lastTargetPosition.transform.position = lastTargetPositions[0];
        }
    }
    #endregion

    #endregion

    #region Trace placement
    private void UpdateTraces()
    {
        if (isTargetInSight)
        {
            ResetLastPlayerPositions();
        }
        else
        {
            if (CanPlaceNextTrace())
            {
                AddTargetPosition();
                if (IsOutsideOfSmellRange())
                {
                    totalTracesLeft--;
                }
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
    }
    private bool IsOutsideOfSmellRange()
    {
        float distance = HelperMethods.Distance(lastTargetPositions[lastTargetPositions.Count - 1], lastTargetPositions[lastTargetPositions.Count - 2]);
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
        if (isTargetInSight && canShootTarget && target != null)
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
    public void SetTarget(GameObject newTarget)
    {
        if (newTarget)
        {
            target = newTarget;
        }
    }
    #endregion
}
