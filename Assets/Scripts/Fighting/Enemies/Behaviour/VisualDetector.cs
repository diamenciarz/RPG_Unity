using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualDetector : TeamUpdater
{
    [Header("Instances")]
    [SerializeField] [Tooltip("For forward orientation and team setup")] GameObject parentGameObject;
    [SerializeField] ShootingController[] shootingControllers;


    [Tooltip("Delta angle from the middle of parent's rotation")]
    [SerializeField] float basicGunDirection;

    [Header("Visual Zone")]
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [Tooltip("The visual range of the camera. Choose 0 for infinite range")]
    [SerializeField] float range = 10f;
    [Tooltip("The click range of the camera if overridden by mouse cursor. Choose 0 for infinite range")]
    [SerializeField] float mouseRange = 10f;
    [SerializeField] float refreshRate = 0.1f;
    [SerializeField] bool targetObstacles = false;

    [Header("Mouse Steering")]
    [SerializeField] bool controlledByMouse;
    [SerializeField] bool isShootingZoneOn;
    [SerializeField] bool ignoreMouseCollisions;

    [Header("Shooting Zone")]
    [SerializeField] GameObject visualZonePrefab;
    [SerializeField] Transform visualZoneTransform;

    private GameObject currentTarget;
    private GameObject[] targetsInSightList;
    private ProgressionBarController shootingZoneScript;

    private bool lastRotationLimitValue;
    private bool isTargetInSight;
    private Coroutine checkCoroutine;

    void Start()
    {
        checkCoroutine = StartCoroutine(IVisualChecks());
    }
    private void Update()
    {
        UpdateUI();
    }

    #region Check coroutine
    private IEnumerator IVisualChecks()
    {
        while (true)
        {
            yield return new WaitForSeconds(refreshRate);
            DoChecks();
            SetShooting();
        }
    }
    private void DoChecks()
    {
        targetsInSightList = FindAllEnemiesInSight();
        currentTarget = StaticDataHolder.GetClosestObjectInSightAngleWise(targetsInSightList,transform.position, GetGunAngle());
        isTargetInSight = CanSeeATarget();
    }
    #region Shooting behaviour
    private void SetShooting()
    {
        if (isTargetInSight)
        {
            UpdateShootingControllers(true);
        }
        else
        {
            UpdateShootingControllers(false);
        }
    }
    private void UpdateShootingControllers(bool shoot)
    {
        foreach (var item in shootingControllers)
        {
            item.SetShoot(shoot);
        }
    }
    #endregion
    #endregion

    #region Checks
    private GameObject[] FindAllEnemiesInSight()
    {
        List<GameObject> targetList = StaticDataHolder.GetEnemyList(team);
        targetList.AddRange(StaticDataHolder.GetObstacleList());
        if (targetList.Count == 0)
        {
            return null;
        }

        List<GameObject> targetsInSight = new List<GameObject>();
        foreach (GameObject target in targetList)
        {
            //I expect enemyList to never have a single null value
            if (CanSeeTarget(target))
            {
                targetsInSight.Add(target);
            }
        }
        return targetsInSight.ToArray();
    }
    private bool CanSeeATarget()
    {
        if (controlledByMouse)
        {
            return IsMouseInSight();
        }
        else
        {
            return IsAnyTargetInSight();
        }
    }
    private bool IsMouseInSight()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 mousePosition = HelperMethods.TranslatedMousePosition(transform.position);
            return CanSeePosition(mousePosition, ignoreMouseCollisions);
        }
        else
        {
            return false;
        }
    }
    private bool IsAnyTargetInSight()
    {
        List<GameObject> enemyList = StaticDataHolder.GetEnemyList(team);
        if (targetObstacles)
        {
            enemyList.AddRange(StaticDataHolder.GetObstacleList());
        }
        foreach (GameObject enemy in enemyList)
        {
            if (CanSeeTarget(enemy))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Helper methods
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
    private float CountAngleToPosition(Vector3 targetPosition)
    {
        float angleFromZeroToItem = HelperMethods.DeltaPositionRotation(transform.position, targetPosition).eulerAngles.z;
        float angleFromGunToItem = Mathf.DeltaAngle(GetGunAngle(), angleFromZeroToItem);

        return angleFromGunToItem;
    }
    #endregion

    #region Count values
    private float GetMiddleAngle()
    {
        float middleAngle = parentGameObject.transform.rotation.eulerAngles.z + basicGunDirection;
        return middleAngle;
    }
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

    #region UI
    private void UpdateUI()
    {
        UpdateUIState();
        UpdateShootingZoneVisibility();
    }
    private void UpdateUIState()
    {
        if (controlledByMouse || isShootingZoneOn)
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
    }
    private void UpdateShootingZoneVisibility()
    {
        if (shootingZoneScript != null)
        {
            if (isTargetInSight)
            {
                //Make the light orange bar disappear
                shootingZoneScript.IsVisible(false);
            }
            else
            {
                //Make the light orange bar show up
                shootingZoneScript.IsVisible(true);
            }
        }
    }

    #region Create/Delete UI
    private void DeleteGunShootingZone()
    {
        if (shootingZoneScript != null)
        {
            Destroy(shootingZoneScript.gameObject);
        }
    }
    private void CreateGunShootingZone()
    {
        if (visualZonePrefab != null && shootingZoneScript == null)
        {
            GameObject newShootingZoneGo = Instantiate(visualZonePrefab, visualZoneTransform);

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
        shootingZoneScript.SetObjectToFollow(visualZoneTransform.gameObject);
        float shootingZoneRotation = basicGunDirection + leftMaxRotationLimit;
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
    }
    #endregion

    #endregion

    #region Accessor methods
    /// <summary>
    /// Checks, whether this detector can directly see this position
    /// </summary>
    /// <param name="targetPosition"></param>
    /// <param name="ignoreCollisions"></param>
    /// <returns></returns>
    public bool CanSeePosition(Vector3 targetPosition, bool ignoreCollisions = false)
    {
        if (ignoreCollisions || HelperMethods.CanSeeDirectly(transform.position, targetPosition))
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
        return false;
    }
    public bool CanSeeTarget(GameObject target)
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
    /// <summary>
    /// Returns the current closest target that this detector can see. (The closest target angle wise)
    /// </summary>
    /// <returns></returns>
    public GameObject GetClosestTarget()
    {
        return currentTarget;
    }
    /// <summary>
     /// Returns all targets that this detector can see
     /// </summary>
     /// <returns></returns>
    public GameObject[] GetTargetsInSight()
    {
        return targetsInSightList;
    }
    /// <summary>
    /// Returns true if the detector can see at least one target
    /// </summary>
    /// <returns></returns>
    public bool CanSeeTargets()
    {
        return isTargetInSight;
    }
    public float GetCurrentRange()
    {
        if (controlledByMouse)
        {
            return mouseRange;
        }
        else
        {
            return range;
        }
    }
    #endregion
}
