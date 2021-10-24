using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutomaticGunController : MonoBehaviour
{
    //Instances
    ScoreCounter scoreCounter;
    GameObject theNearestEnemyGameObject;
    WaveCreator waveCreator;

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
    [SerializeField] bool isSpreadRandom;
    [SerializeField] float nonrandomBulletSpread;
    [SerializeField] float leftBulletSpread;
    [SerializeField] float rightBulletSpread;
    [SerializeField] bool addMySpeedToBulletSpeed;

    [Header("Turret stats")]
    [SerializeField] float gunRotationSpeed;
    [SerializeField] float timeBetweenEachShot;
    //Ewentualnoœæ
    [SerializeField] bool hasRotationLimits;
    [SerializeField] float leftMaxRotationLimit;
    [SerializeField] float rightMaxRotationLimit;
    [SerializeField] float gunTextureRotationOffset = 180f;

    [Header("Instances")]
    [SerializeField] GameObject enemyBullet;
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
    public int planeTeam;
    private bool enemiesInRange;
    private float lastShotTime;
    private bool isMyBulletARocket;
    private float shootingTimeBank;


    // Start is called before the first frame update
    void Start()
    {
        scoreCounter = FindObjectOfType<ScoreCounter>();
        waveCreator = FindObjectOfType<WaveCreator>();

        CheckIfMyBulletIsARocket();
        lastShotTime = Time.time;
        shootingTimeBank = 0f;

        StartCoroutine(AttackCoroutine());
    }
    private void Update()
    {
        CheckForTimeBankUpdate();
        CheckForShooting();

        UpdateAmmoBarIfCreated();
    }
    private void CheckForTimeBankUpdate()
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
    private void CheckForShooting()
    {
        if (shootsAtTheNearestEnemy)
        {
            enemiesInRange = CheckForEnemiesOnTheFrontInRange();
            RotateOneStepTowardsTarget();
        }
        else
        {
            enemiesInRange = CheckForEnemiesInRange();
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
    public void SetTeam()
    {
        ShipDamageReceiver enemyDamageReceiver;
        if (parentGameObject.TryGetComponent<ShipDamageReceiver>(out enemyDamageReceiver))
        {
            planeTeam = enemyDamageReceiver.planeTeam;
        }
        BulletController enemyBulletController;
        if (parentGameObject.TryGetComponent<BulletController>(out enemyBulletController))
        {
            planeTeam = enemyBulletController.myTeam;
        }
        RocketController rocketController;
        if (parentGameObject.TryGetComponent<RocketController>(out rocketController))
        {
            planeTeam = rocketController.rocketTeam;
        }
        PlayerMovement playerMovement;
        if (parentGameObject.TryGetComponent<PlayerMovement>(out playerMovement))
        {
            planeTeam = playerMovement.planeTeam;
        }
    }
    private void CheckIfMyBulletIsARocket()
    {
        RocketController rocketController;
        if (enemyBullet.TryGetComponent<RocketController>(out rocketController))
        {
            isMyBulletARocket = true;
        }
    }
    private void SetUpGunShootingZone()
    {
        //yield return new WaitForEndOfFrame();

        GameObject newShootingZoneGo = Instantiate(shootingZonePrefab, shootingZoneTransform);
        //Debug.Log("Created shooting zone");
        newShootingZoneGo.transform.localScale = new Vector3(maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.x, maximumRangeFromMouseToShoot / newShootingZoneGo.transform.lossyScale.y, 1);
        //Rotacja we w³aœciwym kierunku
        float shootingZoneRotation = leftMaxRotationLimit;
        //newShootingZoneGo.transform.localRotation = Quaternion.Euler(0,0, shootingZoneRotation);

        //Ustawia wycinek ko³a
        shootingZoneScript = newShootingZoneGo.GetComponent<ProgressionBarController>();
        shootingZoneScript.UpdateProgressionBar((leftMaxRotationLimit + rightMaxRotationLimit), 360);
        //Ustawia objekt do pod¹¿ania
        shootingZoneScript.SetObjectToFollow(shootingZoneTransform.gameObject);
        shootingZoneScript.SetDeltaRotationToObject(Quaternion.Euler(0, 0, shootingZoneRotation));
        //Debug.Log("Rotator z rotation: " + newShootingZoneGo.transform.rotation.eulerAngles.z);
    }
    private void SetUpGunReloadingBar()
    {
        if (gunReloadingBarPrefab != null)
        {
            GameObject newReloadingBarGO = Instantiate(gunReloadingBarPrefab, transform.position, transform.rotation);
            gunReloadingBarScript = newReloadingBarGO.GetComponent<ProgressionBarController>();
            gunReloadingBarScript.SetObjectToFollow(gameObject);
            lastShotTime = parentGameObject.GetComponent<DataScriptForAvoidance>().creationTime;
        }
    }
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
                    LongShot(howManyBulletsAtOnce);

                    lastShotTime = Time.time;
                    shootingTimeBank -= timeBetweenEachShot;
                }

                yield return new WaitForSeconds(timeBetweenEachShot);
            }
            else
            {
                for (int i = 0; i < howManyShots; i++)
                {
                    LongShot(howManyBulletsAtOnce);

                    lastShotTime = Time.time;
                    shootingTimeBank -= timeBetweenEachShot;

                    yield return new WaitForSeconds(timeBetweenEachShot);
                }
            }
        }
        else
        {
            for (int i = 0; i < howManyShots; i++)
            {
                LongShot(howManyBulletsAtOnce);

                lastShotTime = Time.time;
                shootingTimeBank -= timeBetweenEachShot;

                //Powoli siê obraca w tym kierunku (ruch limitowany przez maksymaln¹ prêdkoœæ k¹tow¹)
                yield return RotateTowardsUntilDone(i);
                //Je¿eli obróci³ siê szybciej, ni¿ ma strzelaæ, to czeka
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
    private bool CheckForEnemiesOnTheFrontInRange()
    {
        float middleZRotation = parentGameObject.transform.rotation.eulerAngles.z + gunBasicDirection;
        Vector3 relativePositionFromGunToItem;
        List<GameObject> enemyList = new List<GameObject>();

        if (!isControlledByMouseCursor)
        {
            enemyList.AddRange(FindObjectOfType<WaveCreator>().GetMyEnemyList(planeTeam));

            //Rocket launchers don't shoot at debris
            if (!isMyBulletARocket)
            {
                enemyList.AddRange(scoreCounter.spaceDebrisList);
            }
        }

        if (isControlledByMouseCursor)
        {
            Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            translatedMousePosition.z = transform.position.z;
            relativePositionFromGunToItem = translatedMousePosition - transform.position;
            if (maximumRangeFromMouseToShoot > relativePositionFromGunToItem.magnitude || maximumRangeFromMouseToShoot == 0)
            {
                if (hasRotationLimits)
                {
                    //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika.
                    float angleFromZeroToItem = Vector3.SignedAngle(Vector3.up, relativePositionFromGunToItem, Vector3.forward);
                    float zAngleFromMiddleToItem = Mathf.DeltaAngle(middleZRotation, angleFromZeroToItem);
                    //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika, je¿eli nie ma przeciwnika w zasiêgu, to zwraca fa³sz
                    if (zAngleFromMiddleToItem > -(rightMaxRotationLimit + 5) && zAngleFromMiddleToItem < (leftMaxRotationLimit + 5))
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
        else
        {
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
                            //Wylicza k¹t od aktualnego kierunku do najbli¿szego przeciwnika, je¿eli nie ma przeciwnika w zasiêgu, to zwraca fa³sz
                            if (zAngleFromMiddleToItem > -rightMaxRotationLimit && zAngleFromMiddleToItem < leftMaxRotationLimit)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }
    private bool CheckForEnemiesInRange()
    {

        if (isControlledByMouseCursor)
        {
            Vector3 relativePositionFromGunToItem;
            Vector3 translatedMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            translatedMousePosition.z = transform.position.z;
            relativePositionFromGunToItem = translatedMousePosition - transform.position;
            if (maximumRangeFromMouseToShoot > relativePositionFromGunToItem.magnitude || maximumRangeFromMouseToShoot == 0)
            {
                return true;
            }
        }
        else
        {
            Vector3 relativePositionFromGunToItem;
            List<GameObject> enemyList = FindObjectOfType<WaveCreator>().GetMyEnemyList(planeTeam);

            //Rocket launchers don't shoot at debris
            if (!isMyBulletARocket)
            {
                enemyList.AddRange(scoreCounter.spaceDebrisList);
            }

            foreach (var item in enemyList)
            {
                if (item != null)
                {
                    relativePositionFromGunToItem = item.transform.position - transform.position;
                    if (maximumShootingRange > relativePositionFromGunToItem.magnitude || maximumShootingRange == 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
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
    private GameObject FindTheClosestEnemyInTheFrontInRange()
    {
        List<GameObject> enemyList = waveCreator.GetMyEnemyList(planeTeam);
        enemyList.AddRange(scoreCounter.spaceDebrisList);
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
    private IEnumerator RotateTowardsUntilDone(int i)
    {
        //Wylicza przesuniêcie na podstawie tego, który to strza³ z kolei oraz, o ile ma siê przesuwaæ pomiêdzy strza³ami
        float gunRotationOffset = (shootingSpread * i);
        //Ustawia rotacjê, na pocz¹tkow¹ rotacjê startow¹
        Quaternion basicGunRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
        while (transform.rotation != basicGunRotation)
        {
            Quaternion startingRotation = transform.rotation;


            transform.rotation = Quaternion.RotateTowards(startingRotation, basicGunRotation, gunRotationSpeed / 60);

            yield return new WaitForSeconds(1 / 60);
            basicGunRotation = Quaternion.Euler(0, 0, gunRotationOffset + gunBasicDirection + parentGameObject.transform.rotation.eulerAngles.z);
        }
    }
    private void LongShot(int howManyBulletsAtOnce)
    {
        //DŸwiêk strza³u
        if (shootingSoundsList.Count != 0)
        {
            AudioSource.PlayClipAtPoint(shootingSoundsList[Random.Range(0, shootingSoundsList.Count)], transform.position, shootSoundVolume);
        }
        for (int i = 0; i < howManyBulletsAtOnce; i++)
        {
            Quaternion bulletRotation = transform.rotation;
            float bulletOffset;

            if (isSpreadRandom)
            {
                bulletOffset = Random.Range(-leftBulletSpread, rightBulletSpread);
            }
            else
            {
                bulletOffset = (nonrandomBulletSpread * (i - (howManyBulletsAtOnce - 1f) / 2));
            }
            //Teksturki nie zawsze s¹ przodem
            bulletRotation *= Quaternion.Euler(0, 0, 180 - bulletOffset);
            OneShot(bulletRotation);
        }

    }
    private void OneShot(Quaternion newBulletRotation)
    {
        GameObject newBullet = Instantiate(enemyBullet, shootingPoint.transform.position, newBulletRotation);
        //Ustawia dru¿ynê pocisków na dru¿ynê samolotu z dzia³kiem
        RocketController rocketController;
        if (newBullet.TryGetComponent<RocketController>(out rocketController))
        {
            GameObject target = FindTheClosestEnemyInTheFrontInRange();
            rocketController.SetTarget(target);

            rocketController.SetTeam(planeTeam, parentGameObject);

            if (addMySpeedToBulletSpeed)
            {
                float angleBetweenMySpeedAndRocketSpeed = Mathf.DeltaAngle(newBulletRotation.eulerAngles.z, parentGameObject.transform.rotation.eulerAngles.z);
                float newRocketSpeed = -parentGameObject.GetComponent<DataScriptForAvoidance>().velocityVector.magnitude * Mathf.Cos(angleBetweenMySpeedAndRocketSpeed * Mathf.Deg2Rad);
                Debug.Log("Vector from the ship: " + newRocketSpeed);
                rocketController.SetCurrentRocketSpeed(newRocketSpeed + rocketController.GetTargetRocketSpeed());
            }
        }

        BulletController enemyBulletController;
        if (newBullet.TryGetComponent<BulletController>(out enemyBulletController))
        {

            enemyBulletController.SetBulletTeam(planeTeam);
            scoreCounter.AddBulletToList(newBullet);
            enemyBulletController.SetObjectThatCreatedThisProjectile(parentGameObject);

            if (addMySpeedToBulletSpeed)
            {
                //Ustawia prêdkoœæ pocisku wylatuj¹cego z lufy samolotu
                Vector2 bombSpeedVector = new Vector2(-enemyBulletController.directionalBulletSpeed * Mathf.Sin(newBullet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
                enemyBulletController.directionalBulletSpeed * Mathf.Cos(newBullet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
                //Modyfikuje prêdkoœæ pocisku o prêdkoœæ samolotu
                bombSpeedVector += new Vector2(parentGameObject.GetComponent<DataScriptForAvoidance>().velocityVector.x, parentGameObject.GetComponent<DataScriptForAvoidance>().velocityVector.y);
                enemyBulletController.SetVelocityVector(bombSpeedVector);
            }
        }
        PiercingBulletController piercingBulletController;
        if (newBullet.TryGetComponent<PiercingBulletController>(out piercingBulletController))
        {

            piercingBulletController.SetBulletTeam(planeTeam);
            scoreCounter.AddBulletToList(newBullet);
            piercingBulletController.SetObjectThatCreatedThisBullet(parentGameObject);

            if (addMySpeedToBulletSpeed)
            {
                //Ustawia prêdkoœæ pocisku wylatuj¹cego z lufy samolotu
                Vector2 bombSpeedVector = new Vector2(-piercingBulletController.directionalBulletSpeed * Mathf.Sin(newBullet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad),
                piercingBulletController.directionalBulletSpeed * Mathf.Cos(newBullet.transform.rotation.eulerAngles.z * Mathf.Deg2Rad));
                //Modyfikuje prêdkoœæ pocisku o prêdkoœæ samolotu
                bombSpeedVector += new Vector2(parentGameObject.GetComponent<DataScriptForAvoidance>().velocityVector.x, parentGameObject.GetComponent<DataScriptForAvoidance>().velocityVector.y);
                piercingBulletController.SetNewBulletSpeedVector(bombSpeedVector);
            }
        }
    }

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
}