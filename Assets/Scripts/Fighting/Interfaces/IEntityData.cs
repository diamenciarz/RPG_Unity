using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityData
{
    public abstract int GetTeam();
    public abstract GameObject GetObjectThatCreatedThisProjectile();
    public abstract Vector3 GetVelocityVector3();
    public abstract void ModifyVelocityVector3(Vector3 deltaVector);
}
