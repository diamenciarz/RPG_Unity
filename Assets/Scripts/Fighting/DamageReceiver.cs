using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [Header("Basic Stats")]
    [SerializeField] int health;
    [SerializeField] bool isAnObstacle; //Every team can damage an obstacle
    [SerializeField] GameObject healthBarPrefab;
    [SerializeField] bool turnHealthBarOn;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    private GameObject healthBarInstance;
    private bool isDestroyed = false;
    private int team;
    private IEntityData myEntityData;

    private void Start()
    {
        myEntityData = GetComponent<IEntityData>();
        UpdateTeam();

        if (turnHealthBarOn)
        {
            CreateHealthBar();
        }
    }
    private void UpdateTeam()
    {
        team = -1;
        if (myEntityData != null)
        {
            team = myEntityData.GetTeam();
        }
        else
        {
            Debug.LogError("Entity has no team component");
        }
    }


    //Receive damage
    public void DealDamage(int damage)
    {
        health -= damage;
        CheckHealth();
    }
    public void DealDamage(int damage, GameObject gameObject)
    {
        health -= damage;
        CheckHealth();

        ModifyVelocity(gameObject);
    }
    private void ModifyVelocity(GameObject gameObject)
    {
        IEntityData iEntityData = gameObject.GetComponent<IEntityData>();
        IDamage iDamage = gameObject.GetComponent<IDamage>();
        iEntityData.ModifyVelocityVector3(iDamage.GetPushVector());
    }
    private void CheckHealth()
    {
        if (health <= 0)
        {
            if (!isDestroyed)
            {
                isDestroyed = true;
                HandleBreak();
            }
        }
        else
        {
            HandleHit();
        }
    }


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
    private void DestroyObject()
    {
        TriggerOnDeath triggerOnDeath = GetComponent<TriggerOnDeath>();
        if (triggerOnDeath != null)
        {
            triggerOnDeath.DestroyObject();
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
