using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : OnCollisionBreak, IDamage
{
    [Header("Basic Stats")]
    [SerializeField] int damage;
    [SerializeField] protected bool hurtsAllies;
    [SerializeField] bool isPiercing;

    [Header("Damage type")]
    public List<TypeOfDamage> damageTypes = new List<TypeOfDamage>();

    [Header("Physics settings")]
    public bool isPushing = false;
    public float pushingPower;

    public enum TypeOfDamage
    {
        Projectile,
        Explosion,
        Rocket
    }

    private ICollidingEntityData entityData;
    private int currentDamageLeft;

    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        entityData = GetComponent<ICollidingEntityData>();
        currentDamageLeft = damage;
    }

    #region Collisions
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        DamageCheck(collision.gameObject);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        DamageCheck(collision.gameObject);
    }
    private void DamageCheck(GameObject collisionObject)
    {
        DamageReceiver damageReceiver = collisionObject.GetComponent<DamageReceiver>();
        bool canReceiveDamage = damageReceiver != null;
        if (canReceiveDamage)
        {
            bool isInvulnerable = CheckParent(collisionObject);
            if (isInvulnerable)
            {
                bool shouldDealDamage = damageReceiver.GetTeam() != team || hurtsAllies;
                if (shouldDealDamage)
                {
                    DealDamageToObject(damageReceiver);
                }
            }
        }
    }
    private void DealDamageToObject(DamageReceiver damageReceiver)
    {
        damageReceiver.DealDamage(this);
        HandlePiercing(damageReceiver);
    }
    private void HandlePiercing(DamageReceiver damageReceiver)
    {
        if (isPiercing)
        {
            int collisionHP = damageReceiver.GetCurrentHealth();
            currentDamageLeft -= collisionHP;
            if (currentDamageLeft < 0)
            {
                currentDamageLeft = 0;
                DestroyObject();
            }
        }
    }
    protected void DestroyObject()
    {
        if (!HelperMethods.CallAllTriggers(gameObject))
        {
            StartCoroutine(DestroyAtTheEndOfFrame());
        }
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion

    #region Accessor methods
    public virtual Vector3 GetVelocityVector3()
    {
        return entityData.GetVelocityVector3();
    }
    public int GetDamage()
    {
        return currentDamageLeft;
    }
    public List<TypeOfDamage> GetDamageTypes()
    {
        return damageTypes;
    }
    public bool DamageTypeContains(TypeOfDamage damageType)
    {
        if (damageTypes.Contains(damageType))
        {
            return true;
        }
        return false;
    }
    public bool IsAProjectile()
    {
        return damageTypes.Count != 0;
    }
    public bool GetIsPushing()
    {
        return isPushing;
    }
    public virtual Vector3 GetPushVector(Vector3 colisionPosition)
    {
        return entityData.GetVelocityVector3().normalized * pushingPower;
    }
    #endregion
}
