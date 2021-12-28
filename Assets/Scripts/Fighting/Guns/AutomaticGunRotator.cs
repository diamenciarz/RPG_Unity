using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGunRotator : TeamUpdater
{
    #region Serializable
    [Header("Gun stats")]
    [Tooltip("Delta angle from the middle of parent's rotation")]
    [SerializeField] float basicGunDirection;
    [Tooltip("The shooting range of the gun in autonomic mode. Choose 0 for infinite range")]
    [SerializeField] float maximumShootingRange = 10f;
    [Tooltip("The click range of the gun in manual mode. Choose 0 for infinite range")]
    [SerializeField] float maximumRangeFromMouseToShoot = 10f;

    [Header("Turret stats")]
    [Tooltip("In degrees per second")]
    [SerializeField] float gunRotationSpeed;
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [Tooltip("The delta rotation of the gun sprite to an enemy's position")]
    [SerializeField] float gunTextureRotationOffset = -90f;

    [Header("Instances")]
    [SerializeField] [Tooltip("For forward orientation and team setup")] GameObject parentGameObject;
    [SerializeField] ShootingController[] shootingControllers;
    [SerializeField] VisualDetector[] visualDetectors;
    [Header("Shooting Zone")]
    [SerializeField] GameObject shootingZonePrefab;
    [SerializeField] Transform shootingZoneTransform;

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouseCursor;
    [SerializeField] bool isShootingZoneOn;
    #endregion

    private bool areTargetsInRange;
    private float invisibleTargetRotation;
    [SerializeField] bool debugZoneOn = true;
    private Coroutine randomRotationCoroutine;
    private ProgressionBarController debugZoneScript;
    private bool lastRotationLimitValue;
    //Instances
    private ProgressionBarController shootingZoneScript;
    GameObject theNearestEnemyGameObject;

    protected void Update()
    {
        UpdateUI();

        LookForTargets();
        Rotate();
        CheckShooting();
    }
    private void Rotate()
    {
        if (areTargetsInRange)
        {
            StopRandomRotationCoroutine();
        }
        else
        {
            CreateRandomRotationCoroutine();
        }
        RotateOneStepTowardsTarget();
    }

    #region RandomRotation
    private void CreateRandomRotationCoroutine()
    {
        if (randomRotationCoroutine == null)
        {
            float deltaAngleFromTheMiddle = GetGunDeltaMiddleAngle();
            invisibleTargetRotation = deltaAngleFromTheMiddle;
            randomRotationCoroutine = StartCoroutine(RotateRandomly());
        }
    }
    private void StopRandomRotationCoroutine()
    {
        if (randomRotationCoroutine != null)
        {
            StopCoroutine(randomRotationCoroutine);
            randomRotationCoroutine = null;
        }
    }
    private IEnumerator RotateRandomly()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3, 8));
            if (!areTargetsInRange)
            {
                GenerateNewInvisibleTargetAngle();
            }
        }
    }
    private void GenerateNewInvisibleTargetAngle()
    {
        invisibleTargetRotation = Random.Range(-leftMaxRotationLimit, rightMaxRotationLimit);
    }
    private float CountMoveTowardsInvisibleTarget()
    {
        float deltaAngleFromTheMiddle = GetGunDeltaMiddleAngle();
        float angleFromGunToItem = Mathf.DeltaAngle(deltaAngleFromTheMiddle, invisibleTargetRotation);
        return angleFromGunToItem;
    }
    #endregion

    private void UpdateUI()
    {
        UpdateUIState();
        //UpdateShootingZoneVisibility();
    }


    #region Shooting behaviour
    private void CheckShooting()
    {
        if (areTargetsInRange)
        {
            if (isControlledByMouseCursor)
            {
                if (Input.GetKey(KeyCode.Mouse0))
                {
                    SetShoot(true);
                    return;
                }
            }
            else
            {
                SetShoot(true);
                return;
            }
        }
        SetShoot(false);
    }
    private void SetShoot(bool shoot)
    {
        foreach (var item in shootingControllers)
        {
            item.SetShoot(shoot);
        }
    }
    #endregion

    #region CanShoot
    private void LookForTargets()
    {
        foreach (VisualDetector detector in visualDetectors)
        {
            if (detector.get)
            {

            }
        }
        areTargetsInRange = AreTargetsInRange();
    }
    private bool AreTargetsInRange()
    {
        if (isControlledByMouseCursor)
        {
            Vector3 mousePosition = HelperMethods.TranslatedMousePosition(transform.position);
            return CanShootMouse(mousePosition, maximumRangeFromMouseToShoot);
        }
        else
        {
            return IsAnyEnemyInRange();
        }
    }
    private bool IsAnyEnemyInRange()
    {
        List<GameObject> targetList = StaticDataHolder.GetEnemyList(team);

        targetList.AddRange(StaticDataHolder.GetObstacleList());

        foreach (var item in targetList)
        {
            if (CanShootTarget(item, maximumShootingRange))
            {
                return true;
            }
        }
        return false;
    }

    #region Helper functions
    private bool CanShootTarget(GameObject target, float range)
    {
        if (HelperMethods.CanSeeDirectly(transform.position, target))
        {
            if (hasRotationLimits)
            {
                return IsPositionInCone(target.transform.position, range);
            }
            else
            {
                return IsPositionInRange(target.transform.position, range);
            }
        }
        return false;
    }
    private bool CanShootMouse(Vector3 targetPosition, float range)
    {
        //Mouse can not hide behind a bush
        if (hasRotationLimits)
        {
            return IsPositionInCone(targetPosition, range);
        }
        else
        {
            return IsPositionInRange(targetPosition, range);
        }
    }
    private bool IsPositionInCone(Vector3 targetPosition, float range)
    {
        if (IsPositionInRange(targetPosition, range))
        {
            float angleFromZeroToItem = HelperMethods.AngleFromUpToPosition(transform.position, targetPosition);
            float angleFromMiddleToItem = Mathf.DeltaAngle(GetMiddleAngle(), angleFromZeroToItem);

            bool isCursorInCone = angleFromMiddleToItem > -(rightMaxRotationLimit) && angleFromMiddleToItem < (leftMaxRotationLimit);
            if (isCursorInCone)
            {
                return true;
            }
        }
        return false;
    }
    private bool IsPositionInRange(Vector3 targetPosition, float range)
    {
        float distanceToTarget = HelperMethods.Distance(transform.position, targetPosition);
        bool canShoot = range > distanceToTarget || range == 0;
        if (canShoot)
        {
            return true;
        }
        return false;
    }
    #endregion

    #endregion

    #region Movement
    private void RotateOneStepTowardsTarget()
    {
        float degreesToRotateThisFrame = CountAngleToRotateThisFrameBy();
        RotateBy(degreesToRotateThisFrame);
    }

    private void RotateBy(float angle)
    {
        transform.rotation *= Quaternion.Euler(0, 0, angle);
    }
    #endregion

    #region M Helper Methods
    private float CountAngleToRotateThisFrameBy()
    {
        float zMoveAngle = GetTargetAngle();
        //Clamp by gun rotation speed and frame rate
        float degreesToRotateThisFrame = Mathf.Clamp(zMoveAngle, -gunRotationSpeed * Time.deltaTime, gunRotationSpeed * Time.deltaTime);
        return degreesToRotateThisFrame;
    }
    private float GetTargetAngle()
    {
        if (areTargetsInRange)
        {
            return CountEnemyTargetAngle();
        }
        else
        {
            return CountMoveTowardsInvisibleTarget();
        }
    }
    private float CountEnemyTargetAngle()
    {
        float deltaAngle = CountDeltaAngleToEnemy();
        if (hasRotationLimits)
        {
            deltaAngle = GoAroundBoundaries(deltaAngle);
        }
        if (debugZoneOn)
        {
            UpdateDebugZone(GetGunAngle(), GetGunAngle() + deltaAngle);
        }
        return deltaAngle;
    }

    #region Get Delta Angle
    private float CountDeltaAngleToEnemy()
    {
        if (isControlledByMouseCursor)
        {
            Vector3 mousePosition = HelperMethods.TranslatedMousePosition(transform.position);
            return CountAngleFromGunToEnemyPosition(mousePosition);
        }
        else
        {
            theNearestEnemyGameObject = FindTheClosestEnemyInTheFrontInRange();
            return CountAngleFromGunToEnemyPosition(theNearestEnemyGameObject.transform.position);
        }
    }
    private float CountAngleFromGunToPosition(Vector3 targetPosition)
    {
        float angleFromZeroToItem = HelperMethods.DeltaPositionRotation(transform.position, targetPosition).eulerAngles.z + gunTextureRotationOffset;
        float angleFromGunToItem = Mathf.DeltaAngle(GetGunAngle(), angleFromZeroToItem);

        return angleFromGunToItem;
    }
    private float CountAngleFromGunToEnemyPosition(Vector3 targetPosition)
    {
        Vector3 relativePositionFromGunToItem = HelperMethods.DeltaPosition(transform.position, targetPosition);
        float angleFromMiddleToItem = CountAngleFromMiddleToPosition(relativePositionFromGunToItem);
        if (hasRotationLimits)
        {
            angleFromMiddleToItem = ClampAngleToBoundaries(angleFromMiddleToItem);
        }

        float angleFromGunToItem = Mathf.DeltaAngle(GetGunDeltaMiddleAngle(), angleFromMiddleToItem);

        return angleFromGunToItem;
    }
    private float ClampAngleToBoundaries(float angleFromMiddleToItem)
    {
        if (angleFromMiddleToItem < -rightMaxRotationLimit)
        {
            angleFromMiddleToItem = -rightMaxRotationLimit;
        }
        if (angleFromMiddleToItem > leftMaxRotationLimit)
        {
            angleFromMiddleToItem = leftMaxRotationLimit;
        }
        return angleFromMiddleToItem;
    }
    private float CountAngleFromMiddleToPosition(Vector3 relativePositionFromGunToItem)
    {
        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float middleZRotation = GetMiddleAngle();
        float angleFromMiddleToItem = angleFromZeroToItem - middleZRotation;

        if (angleFromMiddleToItem < -180)
        {
            angleFromMiddleToItem += 360;
        }
        return angleFromMiddleToItem;
    }
    #endregion

    #region Go Around Boundaries
    private float GoAroundBoundaries(float angleToMove)
    {
        if (angleToMove > 0)
        {
            return GoAroundLeftBoundary(angleToMove);
        }
        if (angleToMove < 0)
        {
            return GoAroundRightBoundary(angleToMove);
        }
        return angleToMove;
    }
    private float GoAroundLeftBoundary(float angleToMove)
    {
        float angleFromGunToLeftLimit = CountAngleFromGunToLeftLimit();
        if (angleFromGunToLeftLimit >= 0)
        {
            if (angleToMove > angleFromGunToLeftLimit)
            {
                return (angleToMove - 360);
            }
        }
        return angleToMove;
    }
    private float GoAroundRightBoundary(float angleToMove)
    {
        float angleFromGunToRightLimit = CountAngleFromGunToRightLimit();
        if (angleFromGunToRightLimit <= 0)
        {
            if (angleToMove < angleFromGunToRightLimit)
            {
                return (angleToMove + 360);
            }
        }
        return angleToMove;
    }
    private float CountAngleFromGunToLeftLimit()
    {
        float angleFromGunToLeftLimit = leftMaxRotationLimit - GetGunDeltaMiddleAngle();
        if (angleFromGunToLeftLimit > 180)
        {
            angleFromGunToLeftLimit -= 360;
        }
        return angleFromGunToLeftLimit;
        //return Mathf.DeltaAngle(GetGunAngle() - gunTextureRotationOffset, GetMiddleAngle() + leftMaxRotationLimit);
    }
    private float CountAngleFromGunToRightLimit()
    {
        float angleFromGunToRightLimit = -(rightMaxRotationLimit + GetGunDeltaMiddleAngle());
        if (angleFromGunToRightLimit < -180)
        {
            angleFromGunToRightLimit += 360;
        }
        return angleFromGunToRightLimit;
        //return Mathf.DeltaAngle(GetGunAngle() - gunTextureRotationOffset, GetMiddleAngle() - rightMaxRotationLimit);
    }
    #endregion

    #endregion

    #region GetValues
    /// <summary>
    /// Delta angle between the gun and the middle of the shooting zone
    /// </summary>
    /// <returns></returns>
    private float GetGunDeltaMiddleAngle()
    {
        return GetGunAngle() - GetMiddleAngle();
    }
    /// <summary>
    /// Angle from zero (up) to middle of the gun shooting area
    /// </summary>
    /// <returns></returns>
    private float GetMiddleAngle()
    {
        float middleAngle = parentGameObject.transform.rotation.eulerAngles.z + basicGunDirection;
        if (middleAngle > 180)
        {
            middleAngle -= 360;
        }
        if (middleAngle < -180)
        {
            middleAngle += 360;
        }
        return middleAngle;
    }
    /// <summary>
    /// Angle from zero (up) to the gun
    /// </summary>
    /// <returns></returns>
    private float GetGunAngle()
    {
        Quaternion gunRotation = transform.rotation;
        float gunAngle = gunRotation.eulerAngles.z;

        if (gunAngle > 180)
        {
            gunAngle -= 360;
        }

        return gunAngle;
    }
    #endregion

    #region UpdateUI
    //Update states
    private void UpdateDebugZone(float startAngle, float endAngle)
    {
        float parentAngle = parentGameObject.transform.rotation.eulerAngles.z;
        float angleSize = startAngle - endAngle;
        //Debug.Log(startAngle + ", " + endAngle);
        if (angleSize < 0)
        {
            debugZoneScript.UpdateProgressionBar(-angleSize, 360);
            float shootingZoneRotation = endAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        }
        else
        {
            debugZoneScript.UpdateProgressionBar(angleSize, 360);
            float shootingZoneRotation = startAngle - parentAngle;
            debugZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        }

    }
    private void UpdateShootingZoneVisibility()
    {
        if (shootingZoneScript != null)
        {
            if (areTargetsInRange)
            {
                //Make the light orange bar show up
                shootingZoneScript.IsVisible(true);
            }
            else
            {
                shootingZoneScript.IsVisible(false);
            }
        }
    }
    private void UpdateUIState()
    {
        if (isControlledByMouseCursor || isShootingZoneOn)
        {
            CreateGunShootingZone();
        }
        else
        {
            DeleteGunShootingZone();
        }
        if (lastRotationLimitValue != hasRotationLimits)
        {
            lastRotationLimitValue = hasRotationLimits;
            DeleteGunShootingZone();
            CreateGunShootingZone();
        }
        if (debugZoneScript == null && debugZoneOn)
        {
            CreateDebugZone();
        }
    }
    #endregion

    #region Create/Destroy UI
    //UI
    private void CreateDebugZone()
    {
        if (shootingZonePrefab != null)
        {
            GameObject newShootingZoneGo = Instantiate(shootingZonePrefab, shootingZoneTransform);
            newShootingZoneGo.transform.localScale = new Vector3(1.8f, 1.8f, 1);

            SetupDebugZone(newShootingZoneGo);
        }
    }
    private void SetupDebugZone(GameObject newShootingZoneGo)
    {
        debugZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        debugZoneScript.SetObjectToFollow(shootingZoneTransform.gameObject);
    }
    private void DeleteGunShootingZone()
    {
        if (shootingZoneScript != null)
        {
            Destroy(shootingZoneScript.gameObject);
        }
    }
    private void CreateGunShootingZone()
    {
        if (shootingZonePrefab != null && shootingZoneScript == null)
        {
            GameObject newShootingZoneGo = Instantiate(shootingZonePrefab, shootingZoneTransform);

            float xScale = GetCurrentRange() / newShootingZoneGo.transform.lossyScale.x;
            float yScale = GetCurrentRange() / newShootingZoneGo.transform.lossyScale.y;
            newShootingZoneGo.transform.localScale = new Vector3(xScale, yScale, 1);

            SetupShootingZoneShape(newShootingZoneGo);
        }
    }
    private void SetupShootingZoneShape(GameObject newShootingZoneGo)
    {
        shootingZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        if (hasRotationLimits)
        {
            shootingZoneScript.UpdateProgressionBar((leftMaxRotationLimit + rightMaxRotationLimit), 360);
        }
        else
        {
            shootingZoneScript.UpdateProgressionBar(1, 1);
        }
        shootingZoneScript.SetObjectToFollow(shootingZoneTransform.gameObject);
        float shootingZoneRotation = basicGunDirection + leftMaxRotationLimit;
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
    }
    #endregion

    //Look for targets
    private GameObject FindTheClosestEnemyInTheFrontInRange()
    {
        List<GameObject> targetList = StaticDataHolder.GetEnemyList(team);

        targetList.AddRange(StaticDataHolder.GetObstacleList());
        if (targetList.Count == 0)
        {
            return null;
        }

        GameObject currentClosestEnemy = null;
        foreach (var item in targetList)
        {
            //I expect enemyList to never have a single null value
            if (CanShootTarget(item, maximumShootingRange))
            {
                if (currentClosestEnemy == null)
                {
                    currentClosestEnemy = item;
                }
                float zAngleFromMiddleToCurrentClosestEnemy = CountAngleFromGunToPosition(currentClosestEnemy.transform.position);
                float zAngleFromMiddleToItem = CountAngleFromGunToPosition(item.transform.position);
                //If the found target is closer to the middle (angle wise) than the current closest target, make is the closest target
                bool isCloserAngleWise = Mathf.Abs(zAngleFromMiddleToCurrentClosestEnemy) > Mathf.Abs(zAngleFromMiddleToItem);
                if (isCloserAngleWise)
                {
                    currentClosestEnemy = item;
                }
            }
        }
        return currentClosestEnemy;
    }

    #region Mutator methods
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        isControlledByMouseCursor = isTrue;
    }
    #endregion

    #region Accessor methods
    public float GetCurrentRange()
    {
        if (isControlledByMouseCursor)
        {
            return maximumRangeFromMouseToShoot;
        }
        else
        {
            return maximumShootingRange;
        }
    }
    #endregion
}