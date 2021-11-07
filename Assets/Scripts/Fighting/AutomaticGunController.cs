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
    [SerializeField] Transform shootingZoneTransform;
    private ProgressionBarController shootingZoneScript;

    [Header("Mouse Steering")]
    [SerializeField] bool isControlledByMouseCursor;
    [SerializeField] bool isGunReloadingBarOn;

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
        UpdateUIState();

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
        UpdateUI();

        LookForTargets();
        if (shootsAtTheNearestEnemy)
        {
            RotateOneStepTowardsTarget();
        }
    }
    private void UpdateTimeBank()
    {
        float timeSinceLastShot = Time.time - lastShotTime;
        float timeToFillMagazine = timeBetweenEachShootingChain + (howManyShots * timeBetweenEachShot) - shootingTimeBank;
        bool shouldFillAmmo = timeSinceLastShot >= timeToFillMagazine;
        if (shouldFillAmmo)
        {
            shootingTimeBank = timeBetweenEachShot * howManyShots;
        }
    }
    private void UpdateUI()
    {
        UpdateAmmoBar();
        UpdateShootingZone();
    }
    private void UpdateAmmoBar()
    {
        bool ammoBarExistsAsItShould = isGunReloadingBarOn && (gunReloadingBarScript != null);
        if (ammoBarExistsAsItShould)
        {
            gunReloadingBarScript.UpdateProgressionBar(shootingTimeBank, timeBetweenEachShot * howManyShots);
        }

    }
    private void UpdateShootingZone()
    {
        bool shootingZoneExistsAsItShould = shootingZoneScript != null && isControlledByMouseCursor;
        if (shootingZoneExistsAsItShould)
        {
            bool mouseButtonIsPressedOutsideOfTheShootingZone = !areEnemiesInRange && Input.GetKey(KeyCode.Mouse0);
            bool thereIsEnoughAmmoForAShot = shootingTimeBank >= timeBetweenEachShot;
            if (mouseButtonIsPressedOutsideOfTheShootingZone && thereIsEnoughAmmoForAShot)
            {
                //Make the light orange bar show up
                shootingZoneScript.ShowBar(true);
            }
            else
            {
                shootingZoneScript.ShowBar(false);
            }
        }
    }


    //SHOOTING ------------
    //----Coroutines
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
                    //Debug.Log("Shooting time bank: " + shootingTimeBank / (timeBetweenEachShot * howManyShots));
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
                //Slowly rotates towards new position
                yield return RotateTowardsUntilDone(i);
                //If rotated faster than next shot delay, then waits
                float timeSinceLastShot = Time.time - lastShotTime;
                if (timeSinceLastShot < timeBetweenEachShot)
                {
                    yield return new WaitForSeconds(timeBetweenEachShot - timeSinceLastShot);
                }

                ShootOneSalvo();
            }
            yield return new WaitForSeconds(timeBetweenEachShootingChain);
        }
    }
    //----Checks
    private void LookForTargets()
    {
        if (shootsAtTheNearestEnemy)
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
            return CanShootMouse(GetTranslatedMousePosition(), maximumRangeFromMouseToShoot);
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
            return CanShootMouse(GetTranslatedMousePosition(), maximumRangeFromMouseToShoot);
        }
        else
        {
            return IsEnemyInRange();
        }
    }
    private Vector3 GetTranslatedMousePosition()
    {
        Vector3 returnVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        returnVector.z = transform.position.z;
        return returnVector;

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
                if (CanShootTarget(item, maximumShootingRange))
                {
                    return true;
                }
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
    private bool IsPositionInRange(Vector3 targetPosition, float range)
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
    private bool IsPositionInCone(Vector3 targetPosition, float range)
    {
        if (IsPositionInRange(targetPosition, range))
        {
            float middleZRotation = GetMiddleZRotation();
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
    private bool CanSeeTargetDirectly(GameObject target)
    {
        if (target)
        {
            int obstacleLayerMask = LayerMask.GetMask("Actors", "Obstacles");
            Vector2 origin = shootingPoint.transform.position;
            Vector2 direction = transform.TransformDirection(Vector2.up);

            Debug.DrawRay(shootingPoint.transform.position, direction * 3, Color.red, 0.5f);
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, direction, Mathf.Infinity, obstacleLayerMask);

            if (raycastHit2D)
            {
                GameObject objectHit = raycastHit2D.collider.gameObject;

                Debug.Log("Hit: " + objectHit.name);
                bool hitTargetDirectly = objectHit == target;
                if (hitTargetDirectly)
                {
                    return true;
                }
            }
        }
        return false;
    }


    //Move gun
    private IEnumerator RotateTowardsUntilDone(int i)
    {
        const int STEPS_PER_SECOND = 30;
        //Counts the target rotation
        float gunRotationOffset = (shootingSpread * i);
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
        float degreesToRotateThisFrame = CountDegreesToRotateThisFrame();
        transform.rotation *= Quaternion.Euler(0, 0, degreesToRotateThisFrame);
    }
    //----Helper functions
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
        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float middleZRotation = GetMiddleZRotation();
        float zAngleFromMiddleToItem = angleFromZeroToItem - middleZRotation;

        if (zAngleFromMiddleToItem < -180)
        {
            zAngleFromMiddleToItem += 360;
        }
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
            relativePositionToTarget.z = 0;
        }
        return relativePositionToTarget;
    }
    private Vector3 GetRelativePositionToMouseVector()
    {
        Vector3 relativePositionToTarget = GetTranslatedMousePosition() - transform.position;
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
        foreach (var item in enemyList)
        {
            //I expect enemyList to never have a single null value
            if (CanShootTarget(item, maximumShootingRange))
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
        DecreaseShootingTime();
    }
    private void DecreaseShootingTime()
    {
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
        entityCreator.SummonProjectile(projectilesToCreateList[index], shootingPoint.transform.position, newBulletRotation, team, parentGameObject);
    }
    private void SingleShotForwardWithRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (projectilesToCreateList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);

        newBulletRotation *= transform.rotation;
        entityCreator.SummonProjectile(projectilesToCreateList[index], shootingPoint.transform.position, newBulletRotation, team, parentGameObject);
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
        Debug.LogError("Gun has no team!");
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
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        isControlledByMouseCursor = isTrue;
        UpdateUIState();
    }
    private void CreateUIOnPlayerTakesControl()
    {
        if (isGunReloadingBarOn && gunReloadingBarScript == null)
        {
            CreateGunReloadingBar();
        }
        if (shootingZoneScript == null)
        {
            CreateGunShootingZone();
        }
    }
    private void CreateGunReloadingBar()
    {
        if (gunReloadingBarPrefab != null)
        {
            GameObject newReloadingBarGO = Instantiate(gunReloadingBarPrefab, transform.position, transform.rotation);
            gunReloadingBarScript = newReloadingBarGO.GetComponent<ProgressionBarController>();
            gunReloadingBarScript.SetObjectToFollow(gameObject);
            lastShotTime = Time.time;
        }
    }
    private void CreateGunShootingZone()
    {
        if (shootingZonePrefab != null)
        {
            GameObject newShootingZoneGo = Instantiate(shootingZonePrefab, shootingZoneTransform);
            newShootingZoneGo.transform.localScale = new Vector3(maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.x, maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.y, 1);
            float shootingZoneRotation = leftMaxRotationLimit;

            shootingZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
            shootingZoneScript.UpdateProgressionBar((leftMaxRotationLimit + rightMaxRotationLimit), 360);
            shootingZoneScript.SetObjectToFollow(shootingZoneTransform.gameObject);
            shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        }
    }


    //Set value methods
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
}