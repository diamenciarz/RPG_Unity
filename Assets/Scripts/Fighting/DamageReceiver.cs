using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] int team;
    [SerializeField] int health;
    [SerializeField] bool isAnObstacle; //Every team can damage an obstacle
    [SerializeField] GameObject healthBarPrefab;
    [SerializeField] bool turnHealthBarOn;
    [SerializeField] bool canBePushed;

    [Header("Add to lists")]
    [SerializeField] List<AddToLists> putInLists = new List<AddToLists>();

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    public enum AddToLists
    {
        Projectile,
        PlayerProjectile,
        Entity,
        Obstacle,
        DashableObject
    }

    private GameObject healthBarInstance;
    private bool isDestroyed = false;
    private ICollidingEntityData myEntityData;

    private void Start()
    {
        UpdateStartingVariables();
        if (turnHealthBarOn)
        {
            CreateHealthBar();
        }
    }
    private void OnEnable()
    {
        AddObjectToLists();
    }
    private void OnDisable()
    {
        RemoveObjectFromLists();
    }


    //Modify lists
    protected void AddObjectToLists()
    {
        if (putInLists.Contains(AddToLists.Projectile))
        {
            StaticDataHolder.AddProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.PlayerProjectile))
        {
            StaticDataHolder.AddPlayerProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.Obstacle))
        {
            StaticDataHolder.AddObstacle(gameObject);
        }
        if (putInLists.Contains(AddToLists.Entity))
        {
            StaticDataHolder.AddEntity(gameObject);
        }
        if (putInLists.Contains(AddToLists.DashableObject))
        {
            StaticDataHolder.AddDashableObject(gameObject);
        }
    }
    protected void RemoveObjectFromLists()
    {
        if (putInLists.Contains(AddToLists.Projectile))
        {
            StaticDataHolder.RemoveProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.PlayerProjectile))
        {
            StaticDataHolder.RemovePlayerProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.Obstacle))
        {
            StaticDataHolder.RemoveObstacle(gameObject);
        }
        if (putInLists.Contains(AddToLists.Entity))
        {
            StaticDataHolder.RemoveEntity(gameObject);
        }
        if (putInLists.Contains(AddToLists.DashableObject))
        {
            StaticDataHolder.RemoveDashableObject(gameObject);
        }
    }

    private void UpdateStartingVariables()
    {
        myEntityData = GetComponent<ICollidingEntityData>();
    }


    //Receive damage
    public void DealDamage(int damage)
    {
        health -= damage;
        CheckHealth();
    }
    /// <summary>
    /// Deal damage and try to push object
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="gameObject"></param>
    public void DealDamage(int damage, GameObject gameObject)
    {
        health -= damage;
        CheckHealth();

        ModifyVelocity(gameObject);
    }
    private void ModifyVelocity(GameObject damagingObject)
    {
        if (myEntityData != null)
        {
            IDamage iDamage = damagingObject.GetComponent<IDamage>();
            if (canBePushed && iDamage.GetIsPushing())
            {
                myEntityData.ModifyVelocityVector3(iDamage.GetPushVector());
            }
        }
    }
    private void CheckHealth()
    {
        if (health <= 0)
        {
            HandleBreak();
        }
        else
        {
            HandleHit();
        }
    }
    #region MyRegion

    #endregion

    //Break methods
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            StaticDataHolder.TryPlaySound(GetBreakSound(), transform.position, breakingSoundVolume);

            DestroyObject();
        }
    }
    protected void HandleHit()
    {
        StaticDataHolder.TryPlaySound(GetHitSound(), transform.position, hitSoundVolume);
    }


    //Destroy methods
    public void DestroyObject()
    {
        RemoveObjectFromLists();

        TriggerOnDeath[] triggerOnDeath = GetComponentsInChildren<TriggerOnDeath>();
        if (triggerOnDeath.Length != 0)
        {
            foreach (TriggerOnDeath item in triggerOnDeath)
            {
                item.ObjectDestroyed();
            }
        }
        else
        {
            Debug.Log("No TriggerOnDeath found");
            StartCoroutine(DestroyAtTheEndOfFrame());
        }
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }


    //Sounds
    protected AudioClip GetHitSound()
    {
        int soundIndex = Random.Range(0, hitSounds.Count);
        if (hitSounds.Count > soundIndex)
        {
            return hitSounds[soundIndex];
        }
        return null;
    }
    protected AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }


    //Other stuff
    public void CreateHealthBar()
    {
        healthBarInstance = Instantiate(healthBarPrefab, transform.position, transform.rotation);
        ProgressionBarController progressionBarController = healthBarInstance.GetComponent<ProgressionBarController>();
        if (progressionBarController)
        {
            progressionBarController.SetObjectToFollow(gameObject);
        }
    }


    //Set methods
    public void SetTeam(int newTeam)
    {
        team = newTeam;
        EventManager.TriggerEvent("ChangedObjectTeam", gameObject);
    }


    //Accessor methods
    public int GetCurrentHealth()
    {
        return health;
    }
    public int GetTeam()
    {
        return team;
    }
}
