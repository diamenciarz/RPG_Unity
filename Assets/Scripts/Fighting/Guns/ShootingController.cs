using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingController : TeamUpdater, ISerializationCallbackReceiver
{
    [Header("Instances")]
    [SerializeField] SalvoScriptableObject salvo;
    [Tooltip("Game Object, which will act as the creation point for the bullets")]
    [SerializeField] Transform shootingPoint;
    [SerializeField] GameObject parentGameObject;
    [SerializeField] GameObject gunReloadingBarPrefab;
    [Header("Settings")]
    [Tooltip("True - the gun waits the full time to reload all ammo at once. False - the ammo reolads gradually")]
    public bool reloadAllAtOnce;
    [Tooltip("The direction of bullets coming out of the gun pipe")]
    [SerializeField] float basicGunRotation;
    [Header("Mouse Steering")]
    bool isControlledByMouseCursor;
    [SerializeField] bool isGunReloadingBarOn;

    //The gun tries to shoot, if this is set to true
    [HideInInspector]
    public bool shoot;
    //Private variables
    private ProgressionBarController gunReloadingBarScript;
    private EntityCreator entityCreator;
    private SingleShotScriptableObject currentShotSO;
    public float shootingTimeBank;
    public float currentTimeBetweenEachShot;
    private float lastShotTime;
    public int shotIndex;
    private bool canShoot;
    private int shotAmount;

    protected void Start()
    {
        InitializeStartingVariables();
        //StartCoroutine(ShootSalvo());
        CallStartingMethods();
    }
    private void InitializeStartingVariables()
    {
        parentGameObject = gameObject.transform.parent.gameObject;
        entityCreator = FindObjectOfType<EntityCreator>();
        lastShotTime = Time.time;
        shootingTimeBank = GetSalvoTimeSum();
        shotAmount = salvo.shots.Length;
        canShoot = true;
        shotIndex = 0;
        UpdateTimeBetweenEachShot();
    }
    public void CallStartingMethods()
    {
        UpdateUIState();
    }
    protected void Update()
    {
        CheckTimeBank();
        Shoot();
        UpdateAmmoBar();
    }

    public void Shoot()
    {
        if (shoot)
        {
            if ((shotIndex <= shotAmount - 1) && canShoot)
            {
                DoOneShot(shotIndex);
                canShoot = false;
                StartCoroutine(WaitForNextShotCooldown(shotIndex));
                shotIndex++;
                UpdateTimeBetweenEachShot();
            }
        }
    }
    private void CheckTimeBank()
    {
        if (reloadAllAtOnce)
        {
            TryReloadAllAmmo();
        }
        else
        {
            TryReloadOneBullet();
        }
    }
    private void TryReloadAllAmmo()
    {
        float reloadCooldown = salvo.additionalReloadTime + GetSalvoTimeSum(shotIndex - 1);
        float timeSinceLastShot = Time.time - lastShotTime;
        if (timeSinceLastShot >= reloadCooldown)
        {
            shootingTimeBank = GetSalvoTimeSum();
            shotIndex = 0;
            UpdateTimeBetweenEachShot();
        }
    }
    private void TryReloadOneBullet()
    {
        if (shotIndex > 0)
        {
            float previousShotDelay = salvo.reloadDelays[shotIndex - 1];
            float reloadCooldown = salvo.additionalReloadTime + previousShotDelay;
            float timeSinceLastShot = Time.time - lastShotTime;

            if ((timeSinceLastShot >= reloadCooldown) && (shotIndex > 0))
            {
                shootingTimeBank += previousShotDelay;
                shotIndex--;
                lastShotTime += previousShotDelay;
                UpdateTimeBetweenEachShot();
            }
        }
    }
    IEnumerator WaitForNextShotCooldown(int index)
    {
        float delay = salvo.delayAfterEachShot[index];
        yield return new WaitForSeconds(delay);
        canShoot = true;
    }
    IEnumerator ShootSalvo()
    {
        for (int i = 0; i < shotAmount; i++)
        {
            shotIndex = i;
            UpdateTimeBetweenEachShot();
            DoOneShot(i);
            yield return new WaitForSeconds(salvo.delayAfterEachShot[i]);
        }
    }

    #region Shot Methods
    private void DoOneShot(int shotIndex)
    {
        currentShotSO = salvo.shots[shotIndex];
        PlayShotSound();
        CreateNewProjectiles();
        //Update time bank
        DecreaseShootingTime();
    }
    public void CreateNewProjectiles()
    {
        if (currentShotSO.projectilesToCreateList.Count != 0)
        {
            for (int i = 0; i < currentShotSO.projectilesToCreateList.Count; i++)
            {
                SingleShotForward(i);
            }
        }
    }
    private void SingleShotForward(int i)
    {
        if (currentShotSO.spreadProjectilesEvenly)
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
        Quaternion newBulletRotation = HelperMethods.RandomRotationInRange(currentShotSO.leftBulletSpread, currentShotSO.rightBulletSpread);

        newBulletRotation *= transform.rotation * Quaternion.Euler(0, 0, basicGunRotation);
        entityCreator.SummonProjectile(currentShotSO.projectilesToCreateList[index], shootingPoint.transform.position, newBulletRotation, team, gameObject);
    }
    private void SingleShotForwardWithRegularSpread(int index)
    {
        float bulletOffset = (currentShotSO.spreadDegrees * (index - (currentShotSO.projectilesToCreateList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);

        newBulletRotation *= transform.rotation * Quaternion.Euler(0, 0, basicGunRotation);
        //Parent game object should be the owner of the gun
        entityCreator.SummonProjectile(currentShotSO.projectilesToCreateList[index], shootingPoint.transform.position, newBulletRotation, team, gameObject);
    }
    #endregion

    #region Sound
    //Sounds
    private void PlayShotSound()
    {
        if (currentShotSO.shotSounds.Length != 0)
        {
            AudioClip sound = currentShotSO.shotSounds[Random.Range(0, currentShotSO.shotSounds.Length)];
            StaticDataHolder.PlaySound(sound, transform.position, currentShotSO.shotSoundVolume);
        }
    }
    #endregion

    #region Helper Functions
    public float GetSalvoTimeSum()
    {
        float timeSum = 0;
        foreach (var item in salvo.reloadDelays)
        {
            timeSum += item;
        }
        return timeSum;

    }
    /// <summary>
    /// Summes the time for the amount of shots. Starts counting from the last index. Amount starts from 0.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public float GetSalvoTimeSum(int amount)
    {
        amount = ClampInputIndex(amount);
        float timeSum = 0;

        for (int i = 0; i < amount; i++)
        {
            timeSum += salvo.reloadDelays[i];
        }
        return timeSum;
    }
    private void DecreaseShootingTime()
    {
        lastShotTime = Time.time;
        shootingTimeBank -= currentTimeBetweenEachShot;
    }
    private int ClampInputIndex(int index)
    {
        int shotAmount = salvo.shots.Length;
        if (index < 0)
        {
            index = 0;
        }
        else
        if (index >= shotAmount)
        {
            index = shotAmount - 1;
        }
        return index;
    }
    private void UpdateTimeBetweenEachShot()
    {
        if (shotIndex < salvo.reloadDelays.Count)
        {
            currentTimeBetweenEachShot = salvo.reloadDelays[shotIndex];
        }
        else
        {
            currentTimeBetweenEachShot = 1000;
        }
    }
    #endregion

    #region UI
    //Update states
    private void UpdateUIState()
    {
        if (isControlledByMouseCursor || isGunReloadingBarOn)
        {
            CreateUI();
        }
        else
        {
            DeleteUI();
        }
    }
    private void CreateUI()
    {
        if (gunReloadingBarScript == null)
        {
            CreateGunReloadingBar();
        }
    }
    private void DeleteUI()
    {
        if (gunReloadingBarScript != null)
        {
            Destroy(gunReloadingBarScript.gameObject);
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
    private void UpdateAmmoBar()
    {
        if (gunReloadingBarScript != null)
        {
            gunReloadingBarScript.UpdateProgressionBar(shootingTimeBank, GetSalvoTimeSum());
        }

    }
    public void SetIsControlledByMouseCursorTo(bool isTrue)
    {
        isControlledByMouseCursor = isTrue;
        UpdateUIState();
    }
    #endregion

}
