using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : MonoBehaviour
{
    [Header("Bullet Properties")]
    public int bulletTeam;
    [SerializeField] protected float speed;
    [SerializeField] protected int bulletDamage;
    [SerializeField] protected List<Sprite> spriteList;

    [Header("Upon Breaking")]
    [SerializeField] protected bool turnsIntoSomething;
    [SerializeField] protected EntityCreator.BulletTypes turnsIntoGameObject;

    [Header("Sounds")]
    [SerializeField] protected List<AudioClip> breakingSounds;
    [SerializeField] [Range(0, 1)] protected float breakingSoundVolume = 1f;
    [SerializeField] protected List<AudioClip> hitSounds;
    [SerializeField] [Range(0, 1)] protected float hitSoundVolume = 1f;

    [Header("Collision Settings")]
    public bool breaksOnContactWithAllyBullets;
    public bool breaksOnContactWithEnemyBullets;
    public bool breaksOnContactWithAllies;
    public bool breaksOnContactWithEnemies;
    public bool breaksOnContactWithBombs = true;

    //Private variables
    [HideInInspector]
    public bool isAPlayerBullet = false;
    protected bool isDestroyed = false;
    protected Vector2 velocityVector;
    protected float creationTime;
    //Objects
    protected EntityCreator entityCreator;
    protected SpriteRenderer mySpriteRenderer;
    public GameObject objectThatCreatedThisProjectile;



    protected virtual void Awake()
    {
        mySpriteRenderer = FindObjectOfType<SpriteRenderer>();
        entityCreator = FindObjectOfType<EntityCreator>();

        SetupStartingValues();
    }
    protected virtual void SetupStartingValues()
    {
        velocityVector = StaticDataHolder.GetMoveVectorInDirection(speed, transform.rotation.eulerAngles.z);
        creationTime = Time.time;
    }
    protected abstract void Update();

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        DamageReceiver damageReceiver = collision.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            damageReceiver.ReceiveDamage(bulletDamage);
            HandleHit(damageReceiver);
        }
        BulletController bulletController = collision.GetComponent<BulletController>();
        if (bulletController != null)
        {
            int otherBulletTeam = bulletController.bulletTeam;
            bool collidedWithABomb = false; // Implement check later
            bool shouldBreak = (collidedWithABomb && breaksOnContactWithBombs)
                || (otherBulletTeam != bulletTeam && breaksOnContactWithEnemyBullets)
                || (breaksOnContactWithAllyBullets && otherBulletTeam == bulletTeam);
            if (shouldBreak)
            {
                HandleBreak();
            }
        }
        if (collision.tag == "Obstacle")
        {
            HandleBreak();
        }
    }

    //Handle destroy
    public void DestroyProjectile()
    {
        StaticDataHolder.projectileList.Remove(gameObject);
        Destroy(gameObject);
    }
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            TryPlaySound(GetBreakSound());
            if (turnsIntoSomething)
            {
                CreateNewProjectile();
            }
            else
            {
                DestroyProjectile();
            }
        }
    }
    protected void HandleHit(DamageReceiver collisionDamageReceiver)
    {
        bool shouldBreak = (collisionDamageReceiver.GetTeam() == bulletTeam && breaksOnContactWithAllies)
            || (collisionDamageReceiver.GetTeam() != bulletTeam && breaksOnContactWithEnemies);
        if (shouldBreak)
        {
            if (!isDestroyed)
            {
                TryPlaySound(GetHitSound());
                if (turnsIntoSomething)
                {
                    CreateNewProjectile();
                }
                else
                {
                    DestroyProjectile();
                }
            }
        }
    }
    protected void TryPlaySound(AudioClip sound)
    {
        try
        {
            if (StaticDataHolder.GetSoundCount() <= (StaticDataHolder.GetSoundLimit() - 4))
            {
                AudioSource.PlayClipAtPoint(sound, transform.position, hitSoundVolume);
                StaticDataHolder.AddSoundDuration(sound.length);
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("Sound list empty");
            throw;
        }
    }
    protected void CreateNewProjectile()
    {
        entityCreator.SummonProjectile(turnsIntoGameObject, transform.position, transform.rotation, bulletTeam, gameObject);

        DestroyProjectile();
    }
    protected AudioClip GetHitSound()
    {
        int soundIndex = Random.Range(0, hitSounds.Count);
        return hitSounds[soundIndex];
    }
    protected AudioClip GetBreakSound()
    {
        int soundIndex = Random.Range(0, breakingSounds.Count);
        return breakingSounds[soundIndex];
    }


    //Set values
    public void SetBulletTeam(int newTeam)
    {
        bulletTeam = newTeam;
        SetSpriteAccordingToTeam();
    }
    public void SetObjectThatCreatedThisProjectile(GameObject parentGameObject)
    {
        objectThatCreatedThisProjectile = parentGameObject;
    }
    private void SetSpriteAccordingToTeam()
    {
        if (spriteList.Count >= bulletTeam && bulletTeam != 0)
        {
            try
            {
                mySpriteRenderer.sprite = spriteList[bulletTeam - 1];
            }
            catch (System.Exception)
            {
                Debug.LogError("Bullet sprite list out of bounds. Index: " + (bulletTeam - 1));
                throw;
            }
        }
    }


    //Get values
    public int GetDamage()
    {
        return bulletDamage;
    }
    public GameObject GetObjectThatCreatedThisProjectile()
    {
        return objectThatCreatedThisProjectile;
    }
}
