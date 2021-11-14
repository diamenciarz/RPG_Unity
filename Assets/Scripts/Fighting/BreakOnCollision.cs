using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakOnCollision : MonoBehaviour
{
    [Header("Collision Settings")]
    public List<BreaksImmediatelyOnContactWith> breakEnum = new List<BreaksImmediatelyOnContactWith>();

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;

    public enum BreaksImmediatelyOnContactWith
    {
        Allies,
        Enemies,
        AllyBullets,
        EnemyBullets,
        Explosions,
        Rockets,
        Obstacles
    }

    private bool isDestroyed = false;
    private int team;
    private float creationTime;
    private IEntityData entityData;
    private bool isARocket;

    private void Start()
    {
        UpdateStartingVariables();
    }
    private void UpdateStartingVariables()
    {
        UpdateTeam();
        entityData = GetComponent<IEntityData>();
        creationTime = Time.time;
        CheckRocket();
    }
    private void UpdateTeam()
    {
        team = -1;
        if (entityData != null)
        {
            team = entityData.GetTeam();
        }
        else
        {
            Debug.LogError("Entity has no team component");
        }
    }
    private void CheckRocket()
    {
        OnCollisionDamage onCollisionDamage = GetComponent<OnCollisionDamage>();
        if (onCollisionDamage != null)
        {
            isARocket = onCollisionDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Rocket);
        }
    }


    //Collision methods
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }
    protected virtual void HandleCollision(GameObject collisionObject)
    {
        IDamage damageReceiver = collisionObject.GetComponent<IDamage>();
        if (damageReceiver != null)
        {
            bool shouldBreak = (collisionObject.tag == "Obstacle" && BreaksOnContactWith(BreaksImmediatelyOnContactWith.Obstacles)) || ShouldBreak(damageReceiver);
            if (shouldBreak)
            {
                HandleBreak();
            }
        }
    }
    private bool ShouldBreak(IDamage iDamage)
    {
        int collisionTeam = iDamage.GetTeam();
        bool areTeamsEqual = collisionTeam == team;

        bool breaksOnAllyBullet = BreaksOnContactWith(BreaksImmediatelyOnContactWith.AllyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Bullet) && areTeamsEqual;
        if (breaksOnAllyBullet)
        {
            return true;
        }
        bool breaksOnEnemyBullet = BreaksOnContactWith(BreaksImmediatelyOnContactWith.EnemyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Bullet) && !areTeamsEqual;
        if (breaksOnEnemyBullet)
        {
            return true;
        }
        bool breaksOnAlly = BreaksOnContactWith(BreaksImmediatelyOnContactWith.Allies) && !iDamage.IsAProjectile() && areTeamsEqual;
        if (breaksOnAlly)
        {
            if (Time.time - creationTime > 0.1f)
            {
                return true;
            }
            else
            {
                if (isARocket)
                {
                    return false;
                }
                return true;

            }
        }
        bool breaksOnEnemy = BreaksOnContactWith(BreaksImmediatelyOnContactWith.Enemies) && !iDamage.IsAProjectile() && !areTeamsEqual;
        if (breaksOnEnemy)
        {
            return true;
        }
        bool breaksOnExplosion = BreaksOnContactWith(BreaksImmediatelyOnContactWith.Explosions) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Explosion);
        if (breaksOnExplosion)
        {
            return true;
        }
        bool breaksOnRocket = BreaksOnContactWith(BreaksImmediatelyOnContactWith.Rockets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Rocket);
        if (breaksOnRocket)
        {
            return true;
        }
        return false;
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
    protected AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        if (breakingSounds.Count > soundIndex)
        {
            return breakingSounds[soundIndex];
        }
        return null;
    }


    //Accessor methods
    public bool BreaksOnContactWith(BreaksImmediatelyOnContactWith contact)
    {
        if (breakEnum.Contains(contact))
        {
            return true;
        }
        return false;
    }
}
