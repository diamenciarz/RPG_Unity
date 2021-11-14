using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamage
{
    public abstract int GetDamage();
    public abstract int GetTeam();
    public abstract List<OnCollisionDamage.TypeOfDamage> GetDamageTypes();
    public abstract bool DamageTypeContains(OnCollisionDamage.TypeOfDamage damageType);
    public abstract bool IsAProjectile();
    public abstract Vector3 GetVelocityVector3();
    public abstract bool GetIsPushing();
    public abstract Vector3 GetPushVector();
}
