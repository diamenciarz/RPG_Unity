using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{
    [Header("Timed destroy")]
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinity")]
    [SerializeField] float destroyDelay = 5f;
    [Tooltip("Destroy the bullet after it has travelled this much distance. -1 for infinity")]
    [SerializeField] float destroyDistance = 1;

    [Header("Wall bouncing")]
    public List<BreaksOn> bounceEnum = new List<BreaksOn>();
    [Tooltip("Min angle from the collision's normal to reflect")]
    [SerializeField] float minAngleToReflect = 45;
    [Tooltip("-1 for infinite bounces")]
    [SerializeField] int maxReflections = 3;
    [Tooltip("How much time to add to the bullet's lifetime after a reflection")]
    [SerializeField] float timeToAdd = 2f;

    private float destroyTime;
    private int bounces;
    private Vector3 collisionNormal;
    private CapsuleCollider2D myCollider2D;
    private bool timedDestroy = false;

    protected override void Start()
    {
        base.Start();

        SetupStartingVariables();
        if (timedDestroy)
        {
            StartCoroutine(CheckDestroyDelay());
        }
    }
    private void SetupStartingVariables()
    {
        myCollider2D = GetComponent<CapsuleCollider2D>();
        SetupDestroyTime();
    }

    #region Destroy
    private void SetupDestroyTime()
    {
        float destroyDistanceDelay = CountDistanceDelay();
        bool distanceDelayExists = destroyDistanceDelay != -1;
        bool destroyTimeExists = destroyDelay != -1;
        destroyTime = Time.time;

        if (destroyTimeExists && distanceDelayExists)
        {
            if (destroyDistanceDelay < destroyDelay)
            {
                destroyTime += destroyDistanceDelay;
            }
            else
            {
                destroyTime += destroyDelay;
            }
            timedDestroy = true;
            return;
        }
        if (destroyTimeExists)
        {
            destroyTime += destroyDelay;
            timedDestroy = true;
            return;
        }
        if (distanceDelayExists)
        {
            destroyTime += destroyDistanceDelay;
            timedDestroy = true;
            return;
        }
        destroyTime += 100;
    }
    private float CountDistanceDelay()
    {
        if (destroyDistance != -1)
        {
            return destroyDistance / myRigidbody2D.velocity.magnitude;
        }
        else
        {
            return -1;
        }

    }
    private IEnumerator CheckDestroyDelay()
    {
        yield return new WaitUntil(() => destroyTime < Time.time);
        DestroyObject();
    }
    #endregion

    #region Collisions
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        BounceCheck(collision);
        StartCoroutine(WaitAndUpdateRotation());
    }
    private void BounceCheck(Collision2D collision)
    {
        if (!ShouldReflect(collision))
        {
            DestroyObject();
        }
    }
    private IEnumerator WaitAndUpdateRotation()
    {
        yield return new WaitForEndOfFrame();
        UpdateRotationToFaceForward();
    }

    #region Reflect
    private bool ShouldReflect(Collision2D collision)
    {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        float hitAngle = Vector3.Angle(GetVelocityVector3(), collisionNormal);

        bool isAngleCorrect = Mathf.Abs(hitAngle) >= minAngleToReflect;
        bool hasBouncesLeft = maxReflections == -1 || bounces < maxReflections;

        return hasBouncesLeft && isAngleCorrect;
    }
    #endregion

    #region Bounce
    private void Bounce(Vector3 incomingMomentum)
    {
        Vector3 myMomentum = GetVelocityVector3().normalized * pushingPower;
        Vector3 outcomeVelocity = incomingMomentum + myMomentum;

        //Modify bullet velocity
        SetVelocityVector(outcomeVelocity);
    }
    #endregion

    #region Bounce checks
    /// <summary>
    /// Checks, if the colliding object is an obstacle and can be bounced off of
    /// </summary>
    /// <param name="collisionObject"></param>
    /// <returns></returns>
    private bool ReflectsOffObstacle(GameObject collisionObject)
    {
        return IsAnObstacle(collisionObject) && BouncesOnContactWith(BreaksOn.Obstacles);
    }
    private bool IsAnObstacle(GameObject collisionObject)
    {
        bool isAnObstacle = false;
        ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();

        if (listUpdater)
        {
            isAnObstacle = listUpdater.ListContains(ListUpdater.AddToLists.Obstacle);
        }

        return isAnObstacle || collisionObject.tag == "Obstacle";
    }
    /// <summary>
    /// Checks, if the angle of the hit is correct for a bounce and if there are enough bounces left
    /// </summary>
    /// <returns></returns>
    private bool ReflectsOffAllyOrEnemy(GameObject collisionObject)
    {
        bool areTeamsEqual = team == HelperMethods.GetObjectTeam(collisionObject);
        if (IsInvulnerable(collisionObject))
        {
            bool bouncesOnAlly = areTeamsEqual && HelperMethods.IsObjectAnEntity(collisionObject) && BouncesOnContactWith(BreaksOn.Allies);
            if (bouncesOnAlly)
            {
                return true;
            }
        }
        bool bouncesOnEnemy = !areTeamsEqual && BouncesOnContactWith(BreaksOn.Enemies) && HelperMethods.IsObjectAnEntity(collisionObject);
        return bouncesOnEnemy;
    }
    private bool BouncesOffProjectile(IDamageReceived damageReceiver)
    {
        if (damageReceiver != null && ProjectileCheck(damageReceiver))
        {
            return true;
        }
        return false;
    }
    private bool ProjectileCheck(IDamageReceived iDamage)
    {
        if (!iDamage.GetIsPushing())
        {
            return false;
        }
        int collisionTeam = iDamage.GetTeam();
        bool areTeamsEqual = collisionTeam == team;

        bool breaksOnAllyBullet = BouncesOnContactWith(BreaksOn.AllyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Projectile) && areTeamsEqual;
        if (breaksOnAllyBullet)
        {
            return true;
        }
        bool breaksOnEnemyBullet = BouncesOnContactWith(BreaksOn.EnemyBullets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Projectile) && !areTeamsEqual;
        if (breaksOnEnemyBullet)
        {
            return true;
        }
        bool breaksOnExplosion = BouncesOnContactWith(BreaksOn.Explosions) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Explosion);
        if (breaksOnExplosion)
        {
            return true;
        }
        bool breaksOnRocket = BouncesOnContactWith(BreaksOn.Rockets) && iDamage.DamageTypeContains(OnCollisionDamage.TypeOfDamage.Rocket);
        if (breaksOnRocket)
        {
            return true;
        }
        return false;
    }
    public bool BouncesOnContactWith(BreaksOn contact)
    {
        if (bounceEnum.Contains(contact))
        {
            return true;
        }
        return false;
    }
    #endregion

    #endregion
}


