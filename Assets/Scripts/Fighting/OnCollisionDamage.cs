using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : BreakOnCollision, IDamage
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

    private void Start()
    {
        SetupStartingValues();
        Debug.Log("OnCollisionDamage team:" + team);
    }
    private void SetupStartingValues()
    {
        entityData = GetComponent<ICollidingEntityData>();
        currentDamageLeft = damage;
    }


    //Collision methods
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        HandleCollision(collision.gameObject);
    }
    private void HandleCollision(GameObject collisionObject)
    {
        DamageReceiver damageReceiver = collisionObject.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            if (damageReceiver.GetTeam() != team || hurtsAllies)
            {
                DealDamageToObject(damageReceiver);
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
    private void DestroyObject()
    {
        DamageReceiver damageReceiver = GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            damageReceiver.DestroyObject();
        }
        else
        {
            Debug.Log("No Damage Receiver found");
            StartCoroutine(DestroyAtTheEndOfFrame());
        }
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }


    //Accessor methods
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
    public Vector3 GetVelocityVector3()
    {
        return entityData.GetVelocityVector3();
    }
    public bool GetIsPushing()
    {
        return isPushing;
    }
    public Vector3 GetPushVector()
    {
        return StaticDataHolder.GetDirectionVector(pushingPower, transform.rotation.eulerAngles.z);
    }

}
