using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGunRotator : TeamUpdater, ISerializationCallbackReceiver
{
    //Instances
    GameObject theNearestEnemyGameObject;

    [Header("Sounds")]
    //Sound
    [SerializeField] List<AudioClip> shootingSoundsList;
    [SerializeField] [Range(0, 1)] float shootSoundVolume;

    [Header("Gun stats")]
    [SerializeField] float gunBasicDirection;
    [SerializeField] bool rotatesTowardsTheNearestEnemy = true;
    [SerializeField] float maximumShootingRange = 20f;
    [SerializeField] float maximumRangeFromMouseToShoot = 20f;
    [SerializeField] float deltaTurretRotation = 15f;

    [Header("Turret stats")]
    [SerializeField] float gunRotationSpeed; //Degrees per second
    [SerializeField] float timeBetweenEachShot;
    //Ewentualnoœæ
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [SerializeField] float gunTextureRotationOffset = 180f;

    [Header("Instances")]
    [SerializeField] Transform shootingPoint;
    [SerializeField] [Tooltip("For forward orientation and team setup")] GameObject parentGameObject;
    [SerializeField] ShootingController[] shootingControllers;
    [Header("Shooting Zone")]
    [SerializeField] GameObject shootingZonePrefab;
    [SerializeField] Transform shootingZoneTransform;
    private ProgressionBarController shootingZoneScript;

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouseCursor;
    [SerializeField] bool isShootingZoneOn;

    private bool areEnemiesInRange;
    private float invisibleTargetRotation;
    private Coroutine randomRotationCoroutine;


    // Startup
    protected void Start()
    {
        InitializeStartingVariables();

        CallStartingMethods();
    }

    private void InitializeStartingVariables()
    {

    }
    private void CallStartingMethods()
    {
        CreateUI();
    }
    protected void Update()
    {
        UpdateUI();

        LookForTargets();
        Rotate();
        CheckShooting();
    }
    private void Rotate()
    {
        if (rotatesTowardsTheNearestEnemy)
        {
            if (areEnemiesInRange)
            {
                StopRandomRotationCoroutine();
            }
            else
            {
                CreateRandomRotationCoroutine();
            }
        }
        else
        {
            CreateRandomRotationCoroutine();
        }
        RotateOneStepTowardsTarget();
    }
    private void CreateRandomRotationCoroutine()
    {
        if (randomRotationCoroutine == null)
        {
            randomRotationCoroutine = StartCoroutine(RotateRandomly());
        }
    }
    private void StopRandomRotationCoroutine()
    {
        if (randomRotationCoroutine != null)
        {
            invisibleTargetRotation = GetGunAngle();
            StopCoroutine(randomRotationCoroutine);
            randomRotationCoroutine = null;
        }
    }
    private void UpdateUI()
    {
        UpdateShootingZone();
    }
    private void UpdateShootingZone()
    {
        if (shootingZoneScript != null)
        {
            //bool mouseButtonIsPressedOutsideOfTheShootingZone = !areEnemiesInRange && Input.GetKey(KeyCode.Mouse0);
            //bool thereIsEnoughAmmoForAShot = true; // shootingTimeBank >= timeBetweenEachShot;
            if (areEnemiesInRange)
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


    //SHOOTING ------------
    private void CheckShooting()
    {
        if (areEnemiesInRange)
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
            item.shoot = shoot;
        }
    }

    #region CanShoot
    //----Checks
    private void LookForTargets()
    {
        if (rotatesTowardsTheNearestEnemy)
        {
            areEnemiesInRange = CheckForEnemiesOnTheFrontInRange();
        }
        else
        {
            areEnemiesInRange = CheckForTargetsInRange();
        }
    }
    private bool CheckForEnemiesOnTheFrontInRange()
    {
        if (isControlledByMouseCursor)
        {
            Vector3 mousePosition = StaticDataHolder.GetTranslatedMousePosition(transform.position);
            return CanShootMouse(mousePosition, maximumRangeFromMouseToShoot);
        }
        else
        {
            return IsAnyEnemyInRange();
        }
    }
    private bool CheckForTargetsInRange()
    {
        if (isControlledByMouseCursor)
        {
            Vector3 mousePosition = StaticDataHolder.GetTranslatedMousePosition(transform.position);
            return CanShootMouse(mousePosition, maximumRangeFromMouseToShoot);
        }
        else
        {
            return IsAnyEnemyInRange();
        }
    }
    private bool IsAnyEnemyInRange()
    {
        List<GameObject> targetList = StaticDataHolder.GetMyEnemyList(team);

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

    //----Helper functions
    private bool CanShootTarget(GameObject target, float range)
    {
        if (CanSeeTargetDirectly(target))
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
            float middleZRotation = GetMiddleAngle();
            Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
            float angleFromUpToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
            float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromUpToItem);

            bool isCursorInCone = zAngleFromMiddleToItem > -(rightMaxRotationLimit + 2) && zAngleFromMiddleToItem < (leftMaxRotationLimit + 2);
            if (isCursorInCone)
            {
                return true;
            }
        }
        return false;
    }
    private bool IsPositionInRange(Vector3 targetPosition, float range)
    {
        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        bool canShoot = range > relativePositionFromGunToItem.magnitude || range == 0;
        if (canShoot)
        {
            return true;
        }
        return false;
    }
    private bool CanSeeTargetDirectly(GameObject target)
    {
        int obstacleLayerMask = LayerMask.GetMask("Actors", "Obstacles");
        Vector2 origin = transform.position;
        Vector2 direction = target.transform.position - transform.position;
        Debug.DrawRay(origin, direction, Color.red, 0.5f);

        RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, direction, Mathf.Infinity, obstacleLayerMask);

        if (raycastHit2D)
        {
            GameObject objectHit = raycastHit2D.collider.gameObject;

            bool hitTargetDirectly = objectHit == target;
            if (hitTargetDirectly)
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Movement
    //Move gun
    private IEnumerator RotateTowardsUntilDone(int i)
    {
        const int STEPS_PER_SECOND = 30;
        //Counts the target rotation
        float gunRotationOffset = (deltaTurretRotation * i);
        //Ustawia rotacjê, na pocz¹tkow¹ rotacjê startow¹
        Quaternion targetRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
        while (transform.rotation != targetRotation)
        {
            targetRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, gunRotationSpeed / STEPS_PER_SECOND);

            yield return new WaitForSeconds(1 / STEPS_PER_SECOND);
        }
    }
    private void RotateOneStepTowardsTarget()
    {
        float degreesToRotateThisFrame = CountAngleToRotateThisFrameBy();
        RotateBy(degreesToRotateThisFrame);
    }
    private IEnumerator RotateRandomly()
    {
        invisibleTargetRotation = GetGunAngle();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1, 4));
            GenerateNewInvisibleTargetAngle();
        }
    }
    private void GenerateNewInvisibleTargetAngle()
    {
        invisibleTargetRotation = Random.Range(-leftMaxRotationLimit, rightMaxRotationLimit);
        /*
        while (CountMoveTowardsInvisibleTarget() < 2)
        {
            invisibleTargetRotation = Random.Range(-leftMaxRotationLimit, rightMaxRotationLimit);
        }
        */
    }
    private void RotateBy(float angle)
    {
        transform.rotation *= Quaternion.Euler(0, 0, angle);
    }
    #endregion

    #region Movement Helper Methods
    //----Helper functions
    private float CountAngleToRotateThisFrameBy()
    {
        float zMoveAngle = GetTargetAngle();
        //Clamp by gun rotation speed and frame rate
        float degreesToRotateThisFrame = Mathf.Clamp(zMoveAngle, -gunRotationSpeed * Time.deltaTime, gunRotationSpeed * Time.deltaTime);
        return degreesToRotateThisFrame;
    }
    private float GetTargetAngle()
    {
        if (areEnemiesInRange)
        {
            return CountAngleFromDeltaPosition();
        }
        else
        {
            return CountMoveTowardsInvisibleTarget();
        }
    }
    private float CountAngleFromDeltaPosition()
    {
        Vector3 deltaPositionToEnemy;
        float deltaAngle;
        if (isControlledByMouseCursor)
        {
            deltaPositionToEnemy = GetRelativePositionToMouseVector();
            deltaAngle = CountAngleFromGunToPosition(deltaPositionToEnemy + transform.position);
        }
        else
        {
            theNearestEnemyGameObject = FindTheClosestEnemyInTheFrontInRange();
            deltaPositionToEnemy = theNearestEnemyGameObject.transform.position - transform.position;
            deltaAngle = CountAngleFromGunToPosition(theNearestEnemyGameObject.transform.position);

        }

        if (hasRotationLimits)
        {
            deltaAngle = AdjustZAngleAccordingToBoundaries(deltaAngle, deltaPositionToEnemy);
        }
        return deltaAngle;
    }
    private float CountMoveTowardsInvisibleTarget()
    {
        float deltaAngleFromTheMiddle = GetGunAngle() - GetMiddleAngle();
        float angleFromGunToItem = Mathf.DeltaAngle(deltaAngleFromTheMiddle, invisibleTargetRotation);
        return angleFromGunToItem;
    }
    private float CountAngleFromGunToPosition(Vector3 targetPosition)
    {
        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float angleFromGunToItem = angleFromZeroToItem - GetGunAngle();

        if (angleFromGunToItem < -180)
        {
            angleFromGunToItem += 360;
        }
        return angleFromGunToItem;
    }
    private float AdjustZAngleAccordingToBoundaries(float zAngleFromGunToItem, Vector3 deltaPositionToTarget)
    {
        float angleToMove = IfTargetIsOutOfBoundariesSetItToMaxValue(zAngleFromGunToItem, deltaPositionToTarget);

        //If zAngleToMove would cross a boundary, go around it instead
        angleToMove = GoAroundBoundaries(angleToMove);

        return angleToMove;
    }
    private float IfTargetIsOutOfBoundariesSetItToMaxValue(float angleFromGunToItem, Vector3 relativePositionToTarget)
    {
        float angleFromMiddleToItem = CountAngleFromMiddleToPosition(relativePositionToTarget + transform.position);

        float middleAngle = GetMiddleAngle();
        float gunRotation = GetGunAngle();

        if (angleFromMiddleToItem < -rightMaxRotationLimit)
        {
            angleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleAngle - rightMaxRotationLimit);
        }
        else
        if (angleFromMiddleToItem > leftMaxRotationLimit)
        {
            angleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleAngle + leftMaxRotationLimit);
        }
        return angleFromGunToItem;
    }
    private float GoAroundBoundaries(float angleToMove)
    {
        float middleAngle = GetMiddleAngle();
        float gunRotation = GetGunAngle();

        if (angleToMove > 0)
        {
            float angleFromGunToLeftLimit = Mathf.DeltaAngle(gunRotation, middleAngle + leftMaxRotationLimit);
            if ((angleFromGunToLeftLimit) >= 0)
            {
                if ((angleToMove) > angleFromGunToLeftLimit)
                {
                    angleToMove -= 360;
                }
            }
        }
        if (angleToMove < 0)
        {
            float zRotationFromGunToRightLimit = Mathf.DeltaAngle(gunRotation, middleAngle - rightMaxRotationLimit);
            if (zRotationFromGunToRightLimit <= 0)
            {
                if ((angleToMove) < zRotationFromGunToRightLimit)
                {
                    angleToMove += 360;
                }
            }
        }

        return angleToMove;
    }
    private float CountAngleFromMiddleToPosition(Vector3 targetPosition)
    {
        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
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

    private Vector3 GetRelativePositionToMouseVector()
    {
        Vector3 relativePositionToTarget = StaticDataHolder.GetTranslatedMousePosition(transform.position) - transform.position;
        return relativePositionToTarget;
    }
    #region GetValues
    private float GetMiddleAngle()
    {
        float middleAngle = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
        while (middleAngle > 180f)
        {
            middleAngle -= 360f;
        }
        return middleAngle;
    }
    private float GetGunAngle()
    {
        Quaternion gunRotation = transform.rotation * Quaternion.Euler(0, 0, -gunTextureRotationOffset);
        float gunAngle = gunRotation.eulerAngles.z;

        if (gunAngle > 180)
        {
            gunAngle -= 360;
        }
        return gunAngle;
    }
    #endregion

    #endregion

    #region UI
    //Update states
    private void UpdateUIState()
    {
        if (isControlledByMouseCursor)
        {
            CreateUI();
        }
        else
        {
            DeleteUI();
        }
    }
    private void DeleteUI()
    {
        if (shootingZoneScript != null)
        {
            Destroy(shootingZoneScript.gameObject);
        }
    }

    //UI
    private void CreateUI()
    {
        if (isShootingZoneOn && shootingZoneScript == null)
        {
            CreateGunShootingZone();
        }
    }
    private void CreateGunShootingZone()
    {
        if (shootingZonePrefab != null)
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
        float shootingZoneRotation = leftMaxRotationLimit;
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
    }
    #endregion

    //Look for targets
    private GameObject FindTheClosestEnemyInTheFrontInRange()
    {
        List<GameObject> targetList = StaticDataHolder.GetMyEnemyList(team);

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
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        isControlledByMouseCursor = isTrue;
        UpdateUIState();
    }
    private float GetCurrentRange()
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

}