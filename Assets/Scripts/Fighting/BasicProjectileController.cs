using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : MonoBehaviour
{
    [Header("Projectile Properties")]
    public int team;
    [SerializeField] protected List<Sprite> spriteList;
    [SerializeField] protected float startingSpeed = 2f;
    [SerializeField] protected int damage;

    [Header("Upon Breaking")]
    [SerializeField] protected List<EntityCreator.BulletTypes> gameObjectsToTurnIntoList;

    [SerializeField] protected bool shootsAtEnemies; //Otherwise shoots forward
    [SerializeField] protected float basicDirection;
    [SerializeField] protected bool spreadProjectilesEvenly;
    [SerializeField] protected float spreadDegrees;
    [SerializeField] protected float leftBulletSpread;
    [SerializeField] protected float rightBulletSpread;

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
    public bool breaksOnContactWithRockets = true;

    [Header("Physics settings")]
    public bool isPushing = true;
    public float pushingPower;

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
        velocityVector = StaticDataHolder.GetDirectionVector(startingSpeed, transform.rotation.eulerAngles.z);
        creationTime = Time.time;
        SetSpriteAccordingToTeam();
    }
    protected virtual void Update()
    {
        MoveOneStep();
    }
    private void MoveOneStep()
    {
        transform.position += new Vector3(velocityVector.x, velocityVector.y, 0) * Time.deltaTime;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        HandleAllCollisionChecks(collision);
    }


    //Collision checks
    private void HandleAllCollisionChecks(Collider2D collision)
    {
        //Can deal damage
        if (CheckCollisionWithEntity(collision))
        {
            return;
        }
        //Just destroy checks
        if (CheckCollisionWithBullet(collision))
        {
            return;
        }
        if (CheckCollisionWithBomb(collision))
        {
            return;
        }
        if (CheckCollisionWithRocket(collision))
        {
            return;
        }
        if (CheckCollisionWithObstacle(collision))
        {
            return;
        }
    }
    private bool CheckCollisionWithEntity(Collider2D collision)
    {
        DamageReceiver damageReceiver = collision.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            bool hitEnemy = damageReceiver.GetTeam() != team;
            if (hitEnemy)
            {
                damageReceiver.ReceiveDamage(damage);
            }
            bool shouldBreak = (damageReceiver.GetTeam() == team && breaksOnContactWithAllies)
            || (damageReceiver.GetTeam() != team && breaksOnContactWithEnemies);
            if (shouldBreak)
            {
                HandleHit(damageReceiver);
            }
            return true;
        }
        return false;
    }
    private bool CheckCollisionWithBullet(Collider2D collision)
    {
        BasicProjectileController basicProjectileController = collision.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            int otherBulletTeam = basicProjectileController.team;
            bool shouldBreak = (otherBulletTeam != team && breaksOnContactWithEnemyBullets)
                || (breaksOnContactWithAllyBullets && otherBulletTeam == team);
            if (shouldBreak)
            {
                HandleBreak();
            }
            return true;
        }
        return false;

    }
    private bool CheckCollisionWithBomb(Collider2D collision)
    {
        BombController bombController = collision.GetComponent<BombController>();
        if (bombController != null)
        {
            if (breaksOnContactWithBombs)
            {
                HandleBreak();
            }
            return true;
        }
        return false;

    }
    private bool CheckCollisionWithRocket(Collider2D collision)
    {
        RocketController rocketController = collision.GetComponent<RocketController>();
        if (rocketController != null)
        {
            if (breaksOnContactWithRockets)
            {
                HandleBreak();
            }
            return true;
        }
        return false;

    }
    private bool CheckCollisionWithObstacle(Collider2D collision)
    {
        if (collision.tag == "Obstacle")
        {
            HandleBreak();
            return true;
        }
        return false;

    }


    //Handle destroy
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            TryPlaySound(GetBreakSound());

            bool bulletSplits = gameObjectsToTurnIntoList.Count != 0;
            if (bulletSplits)
            {
                CreateNewProjectiles();
                DestroyProjectile();
            }
            else
            {
                DestroyProjectile();
            }
        }
    }
    protected void HandleHit(DamageReceiver collisionDamageReceiver)
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            TryPlaySound(GetHitSound());

            bool bulletSplits = gameObjectsToTurnIntoList.Count != 0;
            if (bulletSplits)
            {
                CreateNewProjectiles();
                DestroyProjectile();
            }
            else
            {
                DestroyProjectile();
            }
        }

    }
    protected void DestroyProjectile()
    {
        StaticDataHolder.RemoveProjectile(gameObject);
        StaticDataHolder.RemoveDashableObject(gameObject);
        StaticDataHolder.RemoveProjectile(gameObject);
        if (isAPlayerBullet)
        {
            StaticDataHolder.RemovePlayerProjectile(gameObject);
        }
        StartCoroutine(DestroyAtTheEndOfFrame());
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    private void OnDestroy()
    {

    }


    //Sounds
    protected void TryPlaySound(AudioClip sound)
    {
        if (sound != null)
        {
            if (StaticDataHolder.GetSoundCount() <= (StaticDataHolder.GetSoundLimit() - 4))
            {
                AudioSource.PlayClipAtPoint(sound, transform.position, hitSoundVolume);
                StaticDataHolder.AddSoundDuration(sound.length);
            }
        }
    }
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
    //Shoot 
    public void CreateNewProjectiles()
    {
        if (gameObjectsToTurnIntoList.Count != 0)
        {
            for (int i = 0; i < gameObjectsToTurnIntoList.Count; i++)
            {

                if (shootsAtEnemies == true)
                {
                    GameObject targetGO = StaticDataHolder.GetTheNearestEnemy(transform.position, team);
                    if (targetGO != null)
                    {
                        ShootAtTarget(targetGO.transform.position, i);
                    }
                    else
                    {
                        ShootAtNoTarget(i);
                    }
                }
                else
                {
                    ShootAtNoTarget(i);
                }
            }
        }
    }


    //Shoot once
    private void ShootAtTarget(Vector3 targetPosition, int i)
    {
        if (spreadProjectilesEvenly)
        {
            ShootOnceTowardsPositionWithRegularSpread(i, targetPosition);
        }
        else
        {
            ShootOnceTowardsPositionWithRandomSpread(i, targetPosition);
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

        newBulletRotation *= transform.rotation * Quaternion.Euler(0,0, basicDirection);
        Vector3 myPositionPlusOneStep = transform.position + (GetVelocityVector3() * Time.deltaTime);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], myPositionPlusOneStep, newBulletRotation, team, objectThatCreatedThisProjectile);
    }
    private void ShootOnceForwardWithRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset + basicDirection);

        newBulletRotation *= transform.rotation;
        Vector3 myPositionPlusOneStep = transform.position + (GetVelocityVector3() * Time.deltaTime);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], myPositionPlusOneStep, newBulletRotation, team, objectThatCreatedThisProjectile);
    }
    private void ShootOnceTowardsPositionWithRandomSpread(int index, Vector3 shootAtPosition)
    {
        Quaternion newBulletRotation = StaticDataHolder.GetRandomRotationInRange(leftBulletSpread, rightBulletSpread);
        Quaternion rotationToTarget = StaticDataHolder.GetRotationFromToIn2D(gameObject.transform.position, shootAtPosition);

        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, basicDirection);
        Vector3 myPositionPlusOneStep = transform.position + (GetVelocityVector3() * Time.deltaTime);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], myPositionPlusOneStep, newBulletRotation, team, objectThatCreatedThisProjectile);
    }
    private void ShootOnceTowardsPositionWithRegularSpread(int index, Vector3 shootAtPosition)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = StaticDataHolder.GetRotationFromToIn2D(gameObject.transform.position, shootAtPosition);

        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, basicDirection);
        Vector3 myPositionPlusOneStep = transform.position + (GetVelocityVector3() * Time.deltaTime);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], myPositionPlusOneStep, newBulletRotation, team, objectThatCreatedThisProjectile);
    }


    //Set values
    public void SetTeam(int newTeam)
    {
        team = newTeam;
        SetSpriteAccordingToTeam();
    }
    public void SetObjectThatCreatedThisProjectile(GameObject parentGameObject)
    {
        objectThatCreatedThisProjectile = parentGameObject;
    }
    private void SetSpriteAccordingToTeam()
    {
        if (spriteList.Count >= team && team != 0)
        {
            try
            {
                mySpriteRenderer.sprite = spriteList[team - 1];
            }
            catch (System.Exception)
            {
                Debug.LogError("Bullet sprite list out of bounds. Index: " + (team - 1));
                throw;
            }
        }
    }
    public void SetVelocityVector(Vector2 newVelocityVector)
    {
        velocityVector = newVelocityVector;
    }


    //Get values
    public Vector2 GetVelocityVector2()
    {
        return velocityVector;
    }
    public Vector3 GetVelocityVector3()
    {
        return new Vector3(velocityVector.x, velocityVector.y, 0);
    }
    public int GetDamage()
    {
        return damage;
    }
    public GameObject GetObjectThatCreatedThisProjectile()
    {
        return objectThatCreatedThisProjectile;
    }
}
