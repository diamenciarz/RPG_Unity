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
    [SerializeField] int howManyBulletsAtOnce;
    [SerializeField] bool spreadProjectilesEvenly;
    [SerializeField] float spreadDegrees;
    [SerializeField] float leftBulletSpread;
    [SerializeField] float rightBulletSpread;
    [SerializeField] bool addMySpeedToBulletSpeed;
    [SerializeField] protected List<EntityCreator.BulletTypes> projectilesToCreateList;

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


    [HideInInspector]
    public int team;
    private bool enemiesInRange;
    private float lastShotTime;
    private bool isMyBulletARocket;
    private float shootingTimeBank;
    private EntityCreator entityCreator;


    // Start is called before the first frame update
    void Start()
    {
        entityCreator = FindObjectOfType<EntityCreator>();

        CheckIfMyBulletIsARocket();
        lastShotTime = Time.time;
        shootingTimeBank = 0f;

        StartCoroutine(AttackCoroutine());
    }
    private void Update()
    {
        UpdateTimeBank();
        LookForTargets();

        UpdateAmmoBarIfCreated();
    }


    //Update variables
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
    
    
    //Shooting
        //Coroutines
    private IEnumerator AttackCoroutine()
    {
        while (true)
        {
            if (isControlledByMouseCursor)
            {
                yield return new WaitUntil(() => (enemiesInRange == true) && Input.GetKey(KeyCode.Mouse0));
            }
            else
            {
                yield return new WaitForSeconds(timeBetweenEachShootingChain);

                yield return new WaitUntil(() => (enemiesInRange == true));
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
                    ShootOnce();
                }

                yield return new WaitForSeconds(timeBetweenEachShot);
            }
            else
            {
                for (int i = 0; i < howManyShots; i++)
                {
                    ShootOnce();

                    yield return new WaitForSeconds(timeBetweenEachShot);
                }
            }
        }
        else
        {
            for (int i = 0; i < howManyShots; i++)
            {
                ShootOnce();

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
            enemiesInRange = CheckForEnemiesOnTheFrontInRange();
            RotateOneStepTowardsTarget();
        }
        else
        {
            enemiesInRange = CheckForTargetsInRange();
        }
    }
    private bool CheckForEnemiesOnTheFrontInRange()
    {
        if (isControlledByMouseCursor)
        {
            Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return IsTargetInRange(translatedMousePosition);
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
            return IsTargetInRange(translatedMousePosition);
        }
        else
        {
            return IsEnemyInRange();
        }
    }
    private bool IsEnemyInRange()
    {
        List<GameObject> enemyList = StaticDataHolder.GetMyEnemyList(team);

        //Rocket launchers don't shoot at debris
        if (!isMyBulletARocket)
        {
            enemyList.AddRange(StaticDataHolder.obstacleList);
        }

        foreach (var item in enemyList)
        {
            if (item != null)
            {
                IsTargetInRange(item.transform.position);
            }
        }
        return false;
    }
    private bool IsTargetInRange(Vector3 targetPosition)
    {
        targetPosition.z = transform.position.z;

        Vector3 relativePositionFromGunToItem = targetPosition - transform.position;
        bool canShoot = maximumRangeFromMouseToShoot > relativePositionFromGunToItem.magnitude || maximumRangeFromMouseToShoot == 0;
        if (canShoot)
        {
            if (hasRotationLimits)
            {
                IsTargetInCone(relativePositionFromGunToItem);
            }
            else
            {
                return true;
            }
        }
        return false;
    }
    private bool IsTargetInCone(Vector3 relativePositionFromGunToItem)
    {
        float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;

        float angleFromUpToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
        float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromUpToItem);

        bool isCursorInCone = zAngleFromMiddleToItem > -(rightMaxRotationLimit + 5) && zAngleFromMiddleToItem < (leftMaxRotationLimit + 5);
        if (isCursorInCone)
        {
            return true;
        }
        return false;
    }
    
    //Move gun
    private void RotateOneStepTowardsTarget()
    {
        Quaternion startingRotation = transform.rotation * Quaternion.Euler(0, 0, -gunTextureRotationOffset);
        Vector3 relativePositionToTarget;
        float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
        float gunRotation = startingRotation.eulerAngles.z;

        while (middleZRotation > 180)
        {
            middleZRotation -= 360;
        }
        if (gunRotation > 180)
        {
            gunRotation -= 360;
        }

        if (!isControlledByMouseCursor)
        {
            theNearestEnemyGameObject = FindTheClosestEnemyInTheFrontInRange();
        }

        if (theNearestEnemyGameObject != null || isControlledByMouseCursor)
        {
            //Policz kierunek, w którym trzeba spojrzeæ na najbli¿szego przeciwnika w zasiêgu widzenia i przedstaw go, jako wektor
            if (isControlledByMouseCursor)
            {
                Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                translatedMousePosition.z = transform.position.z;
                relativePositionToTarget = translatedMousePosition - transform.position;
            }
            else
            {
                relativePositionToTarget = theNearestEnemyGameObject.transform.position - transform.position;
            }
            //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
            float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionToTarget, Vector3.forward);

            float zAngleFromGunToItem = Mathf.DeltaAngle(startingRotation.eulerAngles.z, angleFromZeroToItem);
            float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromZeroToItem);

            if (hasRotationLimits)
            {
                //Je¿eli cel jest poza zasiêgiem, to zajmuje najbli¿sz¹ skrajn¹ pozycjê
                if (zAngleFromMiddleToItem < -rightMaxRotationLimit)
                {
                    zAngleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleZRotation - rightMaxRotationLimit);
                }
                else
                if (zAngleFromMiddleToItem > leftMaxRotationLimit)
                {
                    zAngleFromGunToItem = Mathf.DeltaAngle(gunRotation, middleZRotation + leftMaxRotationLimit);
                }

                //Je¿eli chcia³by siê obróciæ przez zakazany teren, to musi iœæ na oko³o
                if (zAngleFromGunToItem > 0)
                {
                    float zRotationFromGunToLeftLimit = Mathf.DeltaAngle(gunRotation, middleZRotation + leftMaxRotationLimit);
                    if ((zRotationFromGunToLeftLimit) >= 0)
                    {
                        if ((zAngleFromGunToItem) > zRotationFromGunToLeftLimit)
                        {
                            zAngleFromGunToItem -= 360;
                        }
                    }
                }
                if (zAngleFromGunToItem < 0)
                {
                    float zRotationFromGunToRightLimit = Mathf.DeltaAngle(gunRotation, middleZRotation - rightMaxRotationLimit);
                    if (zRotationFromGunToRightLimit <= 0)
                    {
                        if ((zAngleFromGunToItem) < zRotationFromGunToRightLimit)
                        {
                            zAngleFromGunToItem += 360;
                        }
                    }
                }
            }

            float degreesToRotateThisFrame = Mathf.Clamp(zAngleFromGunToItem, -gunRotationSpeed * Time.deltaTime, gunRotationSpeed * Time.deltaTime);
            transform.rotation *= Quaternion.Euler(0, 0, degreesToRotateThisFrame);
        }
    }
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


    //Look for targets
    private GameObject FindTheClosestEnemyInTheFrontInRange()
    {
        List<GameObject> enemyList = StaticDataHolder.GetMyEnemyList(team);
        enemyList.AddRange(StaticDataHolder.obstacleList);
        GameObject currentClosestEnemy = null;
        Vector3 relativePositionFromGunToItem;

        float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
        while (middleZRotation > 180f)
        {
            middleZRotation -= 360f;
        }

        foreach (var item in enemyList)
        {
            if (item != null)
            {
                relativePositionFromGunToItem = item.transform.position - transform.position;
                if (maximumShootingRange > relativePositionFromGunToItem.magnitude || maximumShootingRange == 0)
                {
                    if (hasRotationLimits)
                    {
                        //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
                        float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
                        float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromZeroToItem);

                        //Je¿eli nie ma przeciwnika w zasiêgu obrotu, to zwraca fa³sz
                        if (zAngleFromMiddleToItem > -rightMaxRotationLimit && zAngleFromMiddleToItem < leftMaxRotationLimit)
                        {
                            currentClosestEnemy = item;
                            break;
                        }
                    }
                    else
                    {
                        currentClosestEnemy = item;
                        break;
                    }
                }
            }
        }

        if (currentClosestEnemy == null)
        {
            return currentClosestEnemy;
        }

        foreach (var item in enemyList)
        {
            if (item != null)
            {
                relativePositionFromGunToItem = item.transform.position - transform.position;

                //Sprawdza, czy przeciwnik jest w zasiêgu odleg³oœci
                if (maximumShootingRange > relativePositionFromGunToItem.magnitude || maximumShootingRange == 0)
                {

                    //Oblicza k¹t od dzia³ka do aktualnego najbli¿szego przeciwnika
                    Vector3 relativePositionToCurrentClosestEnemy = currentClosestEnemy.transform.position - transform.position;
                    float angleFromGunToCurrentClosestEnemy = Vector3.SignedAngle(transform.rotation.eulerAngles, relativePositionToCurrentClosestEnemy, Vector3.forward);
                    float angleFromGunToItem = Vector3.SignedAngle(transform.rotation.eulerAngles, relativePositionFromGunToItem, Vector3.forward);

                    //Sprawdza, czy item jest pod bli¿szym k¹tem do dzia³ka, ni¿ aktualny najbli¿szy przeciwnik
                    if ((Mathf.Abs(angleFromGunToCurrentClosestEnemy) > Mathf.Abs(angleFromGunToItem)))
                    {

                        if (hasRotationLimits)
                        {
                            //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
                            float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
                            float zAngleFromMiddleToItem = angleFromZeroToItem - middleZRotation;

                            //Je¿eli nie ma przeciwnika w zasiêgu obrotu, to zwraca fa³sz
                            if (zAngleFromMiddleToItem > -rightMaxRotationLimit && zAngleFromMiddleToItem < leftMaxRotationLimit)
                            {
                                currentClosestEnemy = item;
                            }
                        }
                        else
                        {
                            currentClosestEnemy = item;
                        }
                    }
                }
            }
        }
        return currentClosestEnemy;
    }
    private void ShootOnce()
    {
        PlayShotSound();
        CreateNewProjectiles();

        //Update time bank
        lastShotTime = Time.time;
        shootingTimeBank -= timeBetweenEachShot;
    }


    //Shoot once
    public void CreateNewProjectiles()
    {
        if (projectilesToCreateList.Count != 0)
        {
            for (int i = 0; i < howManyBulletsAtOnce; i++)
            {
                ShootAtNoTarget(i);
            }
        }
    }
    private void ShootAtNoTarget(int i)
    {
        if (spreadProjectilesEvenly)
        {
            ShootOnceForwardWithRegularSpread(i);
        }
        else
        {
            ShootOnceForwardWithRandomSpread(i);
        }
    }
    private void ShootOnceForwardWithRandomSpread(int index)
    {
        Quaternion newBulletRotation = StaticDataHolder.GetRandomRotationInRange(leftBulletSpread, rightBulletSpread);

        newBulletRotation *= transform.rotation;
        entityCreator.SummonProjectile(projectilesToCreateList[index], transform.position, newBulletRotation, team, gameObject);
    }
    private void ShootOnceForwardWithRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (howManyBulletsAtOnce - 1f) / 2));
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
            else
            {
                Debug.LogError("No sounds to play: " + gameObject.name);
            }
        }
    }


    //UI
    private void CreateUIOnPlayerTakesControl()
    {
        if (isGunReloadingBarOn && (gunReloadingBarScript == null))
        {
            SetUpGunReloadingBar();
        }
        if ((shootingZoneScript == null) && (shootingZonePrefab != null))
        {
            SetUpGunShootingZone();
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
    private void UpdateAmmoBarIfCreated()
    {
        if (isGunReloadingBarOn && (gunReloadingBarScript != null))
        {
            gunReloadingBarScript.UpdateProgressionBar(shootingTimeBank, timeBetweenEachShot * howManyShots);
        }
        if (shootingZoneScript != null && isControlledByMouseCursor)
        {
            if (!enemiesInRange && Input.GetKey(KeyCode.Mouse0) && (shootingTimeBank >= timeBetweenEachShot))
            {
                shootingZoneScript.ShowBar(true);
            }
            else
            {
                shootingZoneScript.ShowBar(false);
            }
        }
    }



    //Set values
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        if (isTrue)
        {
            isControlledByMouseCursor = isTrue;
            CreateUIOnPlayerTakesControl();
        }
        else
        {
            isControlledByMouseCursor = isTrue;

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
    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }
}