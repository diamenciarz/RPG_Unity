using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnCollisionDamage : MonoBehaviour, IDamage
{
    [Header("Basic Stats")]
    [SerializeField] int damage;

    [Header("Damage type")]
    public List<TypeOfDamage> damageTypes = new List<TypeOfDamage>();

    [Header("Physics settings")]
    public bool isPushing = true;
    public float pushingPower;

    public enum TypeOfDamage
    {
        Bullet,
        Explosion,
        Rocket
    }

    private IEntityData entityData;
    private int team;

    private void Start()
    {
        entityData = GetComponent<IEntityData>();
        UpdateTeam();
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
        DamageReceiver damageReceiver = collisionObject.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            damageReceiver.DealDamage(damage, gameObject);
        }
    }


    //Accessor methods
    public int GetDamage()
    {
        return damage;
    }
    public int GetTeam()
    {
        return team;
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
