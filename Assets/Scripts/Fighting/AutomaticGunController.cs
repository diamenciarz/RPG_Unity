using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGunController : MonoBehaviour
{
    //Instances
    GameObject theNearestEnemyGameObject;

    [Header("Sounds")]
    //Sound
    [SerializeField] List<AudioClip> shootingSoundsList;
    [SerializeField] [Range(0, 1)] float shootSoundVolume;

    [Header("Gun stats")]
    [SerializeField] float gunBasicDirection;
    [SerializeField] bool shootsAtTheNearestEnemy;
    [SerializeField] float maximumShootingRange = 20f;
    [SerializeField] float maximumRangeFromMouseToShoot = 20f;

    [Header("Shooting chain stats")]
    [SerializeField] int howManyShots;
    [SerializeField] float shootingSpread;
    [SerializeField] float timeBetweenEachShootingChain;

    [Header("Each shot")]
    [SerializeField] bool spreadProjectilesEvenly;
    [SerializeField] float spreadDegrees;
    [SerializeField] float leftBulletSpread;
    [SerializeField] float rightBulletSpread;
    [SerializeField] protected List<EntityCreator.BulletTypes> projectilesToCreateList;
    [SerializeField] bool addMySpeedToBulletSpeed;

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
    [SerializeField] GameObject gunReloadingBarPrefab;
    private ProgressionBarController gunReloadingBarScript;
    [Header("Shooting Zone")]
    [SerializeField] GameObject shootingZonePrefab;
    [SerializeField] [Tooltip("For forward orientation")] GameObject theShootingZonesParent;
    [SerializeField] Transform shootingZoneTransform;
    private ProgressionBarController shootingZoneScript;

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouseCursor;
    [SerializeField] bool isGunReloadingBarOn;

    [SerializeField] GameObject laserPrefab;

    [HideInInspector]
    public int team;
    private bool areEnemiesInRange;
    private float lastShotTime;
    private bool isMyBulletARocket;
    private float shootingTimeBank;
    private EntityCreator entityCreator;


    // Startup
    void Start()
    {
        InitializeStartingVariables();

        CallStartingMethods();
    }
    private void InitializeStartingVariables()
    {
        entityCreator = FindObjectOfType<EntityCreator>();
        lastShotTime = Time.time;
        shootingTimeBank = 0f;

    }
    private void CallStartingMethods()
    {
        UpdateTeam();
        CheckIfMyBulletIsARocket();
        SetIsControlledByMouseCursorTo(isControlledByMouseCursor);

        StartCoroutine(AttackCoroutine());
    }
    private void CheckIfMyBulletIsARocket()
    {
        foreach (var item in projectilesToCreateList)
        {
            if (entityCreator.IsThisProjectileARocket(item))
            {
                isMyBulletARocket = true;
            }
        }
    }
    private void Update()
    {
        UpdateTimeBank();
        LookForTargets();

        UpdateAmmoBarIfCreated();
    }
    private void UpdateTimeBank()
    {
        if ((Time.time - lastShotTime) >= (timeBetweenEachShootingChain + timeBetweenEachShot * howManyShots - shootingTimeBank))
        {
            shootingTimeBank = timeBetweenEachShot * howManyShots;

            if (shootingTimeBank > (timeBetweenEachShot * howManyShots))
            {
                shootingTimeBank = timeBetweenEachShot * howManyShots;
            }
        }
    }


    //SHOOTING ------------
    //Coroutines
    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            if (isControlledByMouseCursor)
            {
                yield return new WaitUntil(() => (areEnemiesInRange == true) && Input.GetKey(KeyCode.Mouse0));
            }
            else
            {
                yield return new WaitForSeconds(timeBetweenEachShootingChain);

                yield return new WaitUntil(() => (areEnemiesInRange == true));
            }

            yield return LongShotLoop();
        }
    }
    public IEnumerator LongShotLoop()
    {
        if (shootsAtTheNearestEnemy)
        {
            if (isControlledByMouseCursor)
            {
                if ((shootingTimeBank) >= timeBetweenEachShot)
                {
                    ShootOneSalvo();
                }

                yield return new WaitForSeconds(timeBetweenEachShot);
            }
            else
            {
                for (int i = 0; i < howManyShots; i++)
                {
                    ShootOneSalvo();

                    yield return new WaitForSeconds(timeBetweenEachShot);
                }
            }
        }
        else
        {
            for (int i = 0; i < howManyShots; i++)
            {
                ShootOneSalvo();

                //Slowly rotates towards new position
                yield return RotateTowardsUntilDone(i);
                //If rotated faster than next shot delay, then waits
                if (Time.time - lastShotTime < timeBetweenEachShot)
                {
                    yield return new WaitForSeconds(timeBetweenEachShot - (Time.time - lastShotTime));
                }
                else
                {
                    Debug.Log("Gun rotation speed is too low for the gun to shoot at it's shooting rate. Parent: " + parentGameObject + " ID: " + parentGameObject.GetInstanceID());
                }
            }
        }
    }
    //Checks
    private void LookForTargets()
    {
        if (shootsAtTheNearestEnemy)
        {
            areEnemiesInRange = CheckForEnemiesOnTheFrontInRange();
            RotateOneStepTowardsTarget();
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
            Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return CanShootTarget(translatedMousePosition);
        }
        else
        {
            return IsEnemyInRange();
        }
    }
    private bool CheckForTargetsInRange()
    {

        if (isControlledByMouseCursor)
        {
            Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return CanShootTarget(translatedMousePosition);
        }
        else
        {
            return IsEnemyInRange();
        }
    }
    private bool IsEnemyInRange()
    {
        List<GameObject> targetList = StaticDataHolder.GetMyEnemyList(team);

        //Rocket launchers don't shoot at debris
        if (!isMyBulletARocket)
        {
            targetList.AddRange(StaticDataHolder.GetObstacleList());
        }

        foreach (var item in targetList)
        {
            if (item != null)
            {
                return CanShootTarget(item.transform.position);
            }
        }
        return false;
    }
    //Helper functions
    private bool CanShootTarget(Vector3 targetPosition)
    {
        if (hasRotationLimits)
        {
            return IsTargetInCone(targetPosition);
        }
        else
        {
            return IsTargetInRange(targetPosition);
        }
    }
    private bool IsTargetInRange(Vector3 targetPosition)
    {
        targetPosition.z = transform.position.z;

        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        bool canShoot = maximumRangeFromMouseToShoot > relativePositionFromGunToItem.magnitude || maximumRangeFromMouseToShoot == 0;
        if (canShoot)
        {
            return true;
        }
        return false;
    }
    private bool IsTargetInCone(Vector3 targetPosition)
    {

        if (IsTargetInRange(targetPosition))
        {
            float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
            Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
            float angleFromUpToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
            float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromUpToItem);

            bool isCursorInCone = zAngleFromMiddleToItem > -(rightMaxRotationLimit + 5) && zAngleFromMiddleToItem < (leftMaxRotationLimit + 5);
            if (isCursorInCone)
            {
                return true;
            }
        }
        return false;
    }





    //Move gun
    private IEnumerator RotateTowardsUntilDone(int i)
    {
        //Counts the target rotation
        float gunRotationOffset = (shootingSpread * i);
        //Ustawia rotacjê, na pocz¹tkow¹ rotacjê startow¹
        Quaternion targetRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
        while (transform.rotation != targetRotation)
        {
            targetRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
            const int STEPS_PER_SECOND = 30;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, gunRotationSpeed / STEPS_PER_SECOND);

            yield return new WaitForSeconds(1 / STEPS_PER_SECOND);
        }
    }
    private void RotateOneStepTowardsTarget()
    {
        float degreesToRotateThisFrame = CountDegreesToRotateThisFrame();
        transform.rotation *= Quaternion.Euler(0, 0, degreesToRotateThisFrame);
    }
    //Helper functions
    private float CountDegreesToRotateThisFrame()
    {
        if (!isControlledByMouseCursor)
        {
            theNearestEnemyGameObject = FindTheClosestEnemyInTheFrontInRange();
        }

        if (theNearestEnemyGameObject != null || isControlledByMouseCursor)
        {
            //The whole angle to move
            float zMoveAngle = CountZMoveAngleTowardsTargetPosition();
            //Clamp to one step this frame
            return Mathf.Clamp(zMoveAngle, -gunRotationSpeed * Time.deltaTime, gunRotationSpeed * Time.deltaTime);
        }
        return 0;
    }
    private float CountZMoveAngleTowardsTargetPosition()
    {
        Quaternion startingRotation = transform.rotation * Quaternion.Euler(0, 0, -gunTextureRotationOffset);
        Vector3 relativePositionToTarget = GetRelativePositionToTarget();

        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionToTarget, Vector3.forward);
        float zAngleFromGunToItem = Mathf.DeltaAngle(startingRotation.eulerAngles.z, angleFromZeroToItem);
        float zMoveAngle = zAngleFromGunToItem;

        if (hasRotationLimits)
        {
            zMoveAngle = AdjustZAngleAccordingToBoundaries(zAngleFromGunToItem, relativePositionToTarget);
        }
        return zMoveAngle;
    }
    private float CountAngleFromMiddleToPosition(Vector3 targetPosition)
    {
        float middleZRotation = GetMiddleZRotation();
        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float zAngleFromMiddleToItem = angleFromZeroToItem - middleZRotation;
        return zAngleFromMiddleToItem;
    }
    private Vector3 GetRelativePositionToTarget()
    {
        Vector3 relativePositionToTarget;
        if (isControlledByMouseCursor)
        {
            relativePositionToTarget = GetRelativePositionToMouseVector();
        }
        else
        {
            relativePositionToTarget = theNearestEnemyGameObject.transform.position - transform.position;
        }
        return relativePositionToTarget;
    }
    private Vector3 GetRelativePositionToMouseVector()
    {
        Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        translatedMousePosition.z = transform.position.z;
        Vector3 relativePositionToTarget = translatedMousePosition - transform.position;
        return relativePositionToTarget;
    }
    private float AdjustZAngleAccordingToBoundaries(float zAngleFromGunToItem, Vector3 relativePositionToTarget)
    {
        float zAngleToMove = IfTargetIsOutOfBoundariesSetItToMaxValue(zAngleFromGunToItem, relativePositionToTarget);

        //If zAngleToMove would cross a boundary, go around it instead
        zAngleToMove = GoAroundBoundaries(zAngleToMove);

        return zAngleToMove;
    }
    private float IfTargetIsOutOfBoundariesSetItToMaxValue(float zAngleFromGunToItem, Vector3 relativePositionToTarget)
    {
        float zAngleFromMiddleToItem = CountAngleFromMiddleToPosition(relativePositionToTarget + transform.position);
        float middleZRotation = GetMiddleZRotation();
        float gunRotation = GetGunRotation();

        if (zAngleFromMiddleToItem < -rightMaxRotationLimit)
        {
            zAngleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleZRotation - rightMaxRotationLimit);
        }
        else
                if (zAngleFromMiddleToItem > leftMaxRotationLimit)
        {
            zAngleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleZRotation + leftMaxRotationLimit);
        }
        return zAngleFromGunToItem;
    }
    private float GoAroundBoundaries(float zAngleToMove)
    {
        float middleZRotation = GetMiddleZRotation();
        float gunRotation = GetGunRotation();
        if (zAngleToMove > 0)
        {
            float zRotationFromGunToLeftLimit = Mathf.DeltaAngle(gunRotation, middleZRotation + leftMaxRotationLimit);
            if ((zRotationFromGunToLeftLimit) >= 0)
            {
                if ((zAngleToMove) > zRotationFromGunToLeftLimit)
                {
                    zAngleToMove -= 360;
                }
            }
        }
        if (zAngleToMove < 0)
        {
            float zRotationFromGunToRightLimit = Mathf.DeltaAngle(gunRotation, middleZRotation - rightMaxRotationLimit);
            if (zRotationFromGunToRightLimit <= 0)
            {
                if ((zAngleToMove) < zRotationFromGunToRightLimit)
                {
                    zAngleToMove += 360;
                }
            }
        }

        return zAngleToMove;
    }
    private float GetMiddleZRotation()
    {
        float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
        while (middleZRotation > 180f)
        {
            middleZRotation -= 360f;
        }
        return middleZRotation;
    }
    private float GetGunRotation()
    {
        Quaternion startingRotation = transform.rotation * Quaternion.Euler(0, 0, -gunTextureRotationOffset);
        float gunRotation = startingRotation.eulerAngles.z;

        if (gunRotation > 180)
        {
            gunRotation -= 360;
        }
        return gunRotation;
    }


    //Look for targets
    private GameObject FindTheClosestEnemyInTheFrontInRange()
    {
        List<GameObject> enemyList = StaticDataHolder.GetMyEnemyList(team);
        enemyList.AddRange(StaticDataHolder.obstacleList);

        if (enemyList.Count == 0)
        {
            return null;
        }

        GameObject currentClosestEnemy = enemyList[0];
        float middleZRotation = GetMiddleZRotation();

        foreach (var item in enemyList)
        {
            //I expect enemyList to never have a single null value
            if (CanShootTarget(item.transform.position))
            {
                float zAngleFromMiddleToCurrentClosestEnemy = CountAngleFromMiddleToPosition(currentClosestEnemy.transform.position);
                float zAngleFromMiddleToItem = CountAngleFromMiddleToPosition(item.transform.position);
                //If the found target is closer to the middle (angle wise) than the current closest target, make is the closest target
                if ((Mathf.Abs(zAngleFromMiddleToCurrentClosestEnemy) > Mathf.Abs(zAngleFromMiddleToItem)))
                {
                    currentClosestEnemy = item;
                }
            }
        }
        return currentClosestEnemy;
    }


    //Shoot once
    private void ShootOneSalvo()
    {
        PlayShotSound();
        CreateNewProjectiles();

        //Update time bank
        lastShotTime = Time.time;
        shootingTimeBank -= timeBetweenEachShot;
    }
    public void CreateNewProjectiles()
    {
        if (projectilesToCreateList.Count != 0)
        {
            for (int i = 0; i < projectilesToCreateList.Count; i++)
            {
                SingleShotForward(i);
            }
        }
    }
    private void SingleShotForward(int i)
    {
        if (spreadProjectilesEvenly)
        {
            SingleShotForwardWithRegularSpread(i);
        }
        else
        {
            SingleShotForwardWithRandomSpread(i);
        }
    }
    private void SingleShotForwardWithRandomSpread(int index)
    {
        Quaternion newBulletRotation = StaticDataHolder.GetRandomRotationInRange(leftBulletSpread, rightBulletSpread);

        newBulletRotation *= transform.rotation;
        entityCreator.SummonProjectile(projectilesToCreateList[index], transform.position, newBulletRotation, team, gameObject);
    }
    private void SingleShotForwardWithRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (projectilesToCreateList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);

        newBulletRotation *= transform.rotation;
        entityCreator.SummonProjectile(projectilesToCreateList[index], transform.position, newBulletRotation, team, gameObject);
    }


    //Sounds
    private void PlayShotSound()
    {
        if (StaticDataHolder.GetSoundCount() <= (StaticDataHolder.GetSoundLimit() - 8))
        {
            if (shootingSoundsList.Count != 0)
            {
                AudioSource.PlayClipAtPoint(shootingSoundsList[Random.Range(0, shootingSoundsList.Count)], transform.position, shootSoundVolume);
            }
        }
    }


    //Update states
    private void UpdateTeam()
    {
        DamageReceiver damageReceiver = GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            team = damageReceiver.GetTeam();
            return;
        }
        DamageReceiver damageReceiverParent = GetComponentInParent<DamageReceiver>();
        if (damageReceiverParent != null)
        {
            team = damageReceiverParent.GetTeam();
            return;
        }
    }
    private void UpdateUIState()
    {
        if (isControlledByMouseCursor)
        {
            CreateUIOnPlayerTakesControl();
        }
        else
        {
            if (gunReloadingBarScript != null)
            {
                Destroy(gunReloadingBarScript.gameObject);
            }

            if (shootingZoneScript != null)
            {
                Destroy(shootingZoneScript.gameObject);
            }
        }
    }


    //UI
    private void UpdateAmmoBarIfCreated()
    {
        if (isGunReloadingBarOn && (gunReloadingBarScript != null))
        {
            gunReloadingBarScript.UpdateProgressionBar(shootingTimeBank, timeBetweenEachShot * howManyShots);
        }
        if (shootingZoneScript != null && isControlledByMouseCursor)
        {
            if (!areEnemiesInRange && Input.GetKey(KeyCode.Mouse0) && (shootingTimeBank >= timeBetweenEachShot))
            {
                shootingZoneScript.ShowBar(true);
            }
            else
            {
                shootingZoneScript.ShowBar(false);
            }
        }
    }
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        isControlledByMouseCursor = isTrue;
        UpdateUIState();
    }
    private void CreateUIOnPlayerTakesControl()
    {
        if (isGunReloadingBarOn && (gunReloadingBarScript == null) && (gunReloadingBarPrefab != null))
        {
            SetUpGunReloadingBar();
        }
        if ((shootingZoneScript == null) && (shootingZonePrefab != null))
        {
            SetUpGunShootingZone();
        }
    }
    private void SetUpGunReloadingBar()
    {
        if (gunReloadingBarPrefab != null)
        {
            GameObject newReloadingBarGO = Instantiate(gunReloadingBarPrefab, transform.position, transform.rotation);
            gunReloadingBarScript = newReloadingBarGO.GetComponent<ProgressionBarController>();
            gunReloadingBarScript.SetObjectToFollow(gameObject);
            lastShotTime = Time.time;
        }
    }
    private void SetUpGunShootingZone()
    {
        GameObject newShootingZoneGo = Instantiate(shootingZonePrefab, shootingZoneTransform);
        newShootingZoneGo.transform.localScale = new Vector3(maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.x, maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.y, 1);
        float shootingZoneRotation = leftMaxRotationLimit;

        shootingZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        shootingZoneScript.UpdateProgressionBar((leftMaxRotationLimit + rightMaxRotationLimit), 360);
        shootingZoneScript.SetObjectToFollow(shootingZoneTransform.gameObject);
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
    }


    //Set value methods
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
}