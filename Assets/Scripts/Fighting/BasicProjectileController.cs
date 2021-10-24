using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : MonoBehaviour
{
    [Header("Bullet Properties")]
    public int myTeam;
    [SerializeField] protected float speed;
    [SerializeField] protected int damage;
    [SerializeField] protected List<Sprite> spriteList;

    [Header("Upon Breaking")]
    [SerializeField] protected bool turnsIntoSomething;
    [SerializeField] protected List<EntityCreator.BulletTypes> turnsIntoGameObjects;

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
        velocityVector = StaticDataHolder.GetDirectionVector(speed, transform.rotation.eulerAngles.z);
        creationTime = Time.time;
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
            bool hitEnemy = damageReceiver.GetTeam() != myTeam;
            if (hitEnemy)
            {
                damageReceiver.ReceiveDamage(damage);
            }
            bool shouldBreak = (damageReceiver.GetTeam() == myTeam && breaksOnContactWithAllies)
            || (damageReceiver.GetTeam() != myTeam && breaksOnContactWithEnemies);
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
            int otherBulletTeam = basicProjectileController.myTeam;
            bool shouldBreak = (otherBulletTeam != myTeam && breaksOnContactWithEnemyBullets)
                || (breaksOnContactWithAllyBullets && otherBulletTeam == myTeam);
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
    public void DestroyProjectile()
    {
        StaticDataHolder.projectileList.Remove(gameObject);
        Destroy(gameObject);
    }
    protected void HandleBreak()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            TryPlaySound(GetBreakSound());
            if (turnsIntoSomething)
            {
                CreateNewProjectiles();
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
            if (turnsIntoSomething)
            {
                CreateNewProjectiles();
            }
            else
            {
                DestroyProjectile();
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
    protected void CreateNewProjectiles()
    {
        if (turnsIntoGameObjects != EntityCreator.BulletTypes.Nothing)
        {
            entityCreator.SummonProjectile(turnsIntoGameObjects, transform.position, transform.rotation, myTeam, gameObject);
        }

        DestroyProjectile();
    }
    public void CreateNewProjectiles(GameObject whatToInstantiate)
    {
        if (whatToInstantiate != null)
        {
            //Mo¿e strzela kilka razy?
            for (int i = 0; i < howManyBulletsAtOnce; i++)
            {

                if (shootsAtPlayer == true)
                {
                    targetGameObject = scoreCounter.CheckForTheNearestEnemy(transform.position, rocketTeam, gameObject);
                    if (targetGameObject != null)
                    {
                        //Policz kierunek, w którym trzeba spojrzeæ na gracza i przedstaw go, jako wektor
                        Vector3 relativePositionToPlayer = targetGameObject.transform.position - transform.position;
                        //Debug.Log("Relative position to Player: " + relativePositionToPlayer);
                        // Zdefiniuj rotacjê, o zwrocie w kierunku gracza

                        Quaternion newBulletRotation;

                        if (relativePositionToPlayer.y >= 0)
                        {
                            newBulletRotation = Quaternion.Euler(0, 0, -Mathf.Atan(relativePositionToPlayer.x / relativePositionToPlayer.y) * 180 / Mathf.PI);

                        }
                        else
                        {
                            newBulletRotation = Quaternion.Euler(0, 0, (-Mathf.Atan(relativePositionToPlayer.x / relativePositionToPlayer.y) * 180 / Mathf.PI) + 180);
                        }
                        //Debug.Log("Rotation to player: " + newBulletRotation.eulerAngles);

                        //Zmodyfikuj t¹ rotacjê o losow¹ wartoœæ
                        newBulletRotation *= Quaternion.AngleAxis(Random.Range(-rightBulletSpread, leftBulletSpread), Vector3.forward);

                        //Stwórz pocisk o odpowiednich w³aœciwoœciach
                        newBullet = Instantiate(whatToInstantiate, transform.position, newBulletRotation);
                    }
                }
                else
                {
                    Quaternion newBulletRotation = Quaternion.Euler(0, 0, Random.Range(-rightBulletSpread, leftBulletSpread));
                    newBulletRotation *= transform.rotation;

                    //Debug.Log("Created a bullet with a rotation of:" + newBulletRotation.eulerAngles);
                    newBullet = Instantiate(whatToInstantiate, transform.position, newBulletRotation);
                    //newBulletRotation must be in degrees
                }
            }
            if (isAPlayerBullet)
            {
                scoreCounter.RemovePlayerBulletFromList(gameObject);
            }

            ShipDamageReceiver enemyDamageReceiver;
            if (newBullet.TryGetComponent<ShipDamageReceiver>(out enemyDamageReceiver))
            {
                enemyDamageReceiver.planeTeam = rocketTeam;
            }
            BulletController enemyBulletController;
            if (newBullet.TryGetComponent<BulletController>(out enemyBulletController))
            {
                enemyBulletController.SetBulletTeam(rocketTeam);
                enemyBulletController.SetObjectThatCreatedThisBullet(objectThatCreatedTheRocket);
                scoreCounter.AddBulletToList(newBullet);
            }
            PiercingBulletController piercingBulletController;
            if (newBullet.TryGetComponent<PiercingBulletController>(out piercingBulletController))
            {
                piercingBulletController.SetBulletTeam(rocketTeam);
                piercingBulletController.SetObjectThatCreatedThisBullet(objectThatCreatedTheRocket);
                scoreCounter.AddBulletToList(newBullet);
            }
            RocketController rocketControllerScript;
            if (newBullet.TryGetComponent<RocketController>(out rocketControllerScript))
            {
                if (targetGameObject != null)
                {
                    StartCoroutine(rocketControllerScript.SetTarget(targetGameObject));
                }
                rocketControllerScript.SetTeam(rocketTeam, objectThatCreatedTheRocket);
            }

        }
        Destroy(gameObject);
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
        myTeam = newTeam;
        SetSpriteAccordingToTeam();
    }
    public void SetObjectThatCreatedThisProjectile(GameObject parentGameObject)
    {
        objectThatCreatedThisProjectile = parentGameObject;
    }
    private void SetSpriteAccordingToTeam()
    {
        if (spriteList.Count >= myTeam && myTeam != 0)
        {
            try
            {
                mySpriteRenderer.sprite = spriteList[myTeam - 1];
            }
            catch (System.Exception)
            {
                Debug.LogError("Bullet sprite list out of bounds. Index: " + (myTeam - 1));
                throw;
            }
        }
    }
    public void SetVelocityVector(Vector2 newVelocityVector)
    {
        velocityVector = newVelocityVector;
    }
    //Get values
    public Vector2 GetVelocityVector()
    {
        return velocityVector;
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
