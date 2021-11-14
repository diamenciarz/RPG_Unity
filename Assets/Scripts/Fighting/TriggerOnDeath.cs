using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOnDeath : TeamUpdater
{

    [Header("Upon Breaking")]
    [SerializeField] protected List<EntityCreator.BulletTypes> gameObjectsToTurnIntoList;

    [SerializeField] protected bool shootsAtEnemies; //Otherwise shoots forward
    [SerializeField] protected float basicDirection;
    [SerializeField] protected bool spreadProjectilesEvenly;
    [SerializeField] protected float spreadDegrees;
    [SerializeField] protected float leftBulletSpread;
    [SerializeField] protected float rightBulletSpread;

    
    private ICollidingEntityData entityData;
    protected EntityCreator entityCreator;
    private bool isDestroyed;

    protected override void Start()
    {
        base.Start();
        entityCreator = FindObjectOfType<EntityCreator>();
        entityData = GetComponent<ICollidingEntityData>();
    }


    //Death summon
    public void ObjectDestroyed()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CreateNewProjectiles();
        }
    }
    private void CreateNewProjectiles()
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
        Quaternion newBulletRotation = StaticDataHolder.GetRandomRotationInRangeZ(leftBulletSpread, rightBulletSpread);

        newBulletRotation *= transform.rotation * Quaternion.Euler(0, 0, basicDirection);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], transform.position, newBulletRotation, team, entityData.GetObjectThatCreatedThisProjectile());
    }
    private void ShootOnceForwardWithRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset + basicDirection);

        newBulletRotation *= transform.rotation;
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], transform.position, newBulletRotation, team, entityData.GetObjectThatCreatedThisProjectile());
    }
    private void ShootOnceTowardsPositionWithRandomSpread(int index, Vector3 shootAtPosition)
    {
        Quaternion newBulletRotation = StaticDataHolder.GetRandomRotationInRangeZ(leftBulletSpread, rightBulletSpread);
        Quaternion rotationToTarget = StaticDataHolder.GetRotationFromToIn2D(gameObject.transform.position, shootAtPosition);

        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, basicDirection);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], transform.position, newBulletRotation, team, entityData.GetObjectThatCreatedThisProjectile());
    }
    private void ShootOnceTowardsPositionWithRegularSpread(int index, Vector3 shootAtPosition)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = StaticDataHolder.GetRotationFromToIn2D(gameObject.transform.position, shootAtPosition);

        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, basicDirection);
        entityCreator.SummonProjectile(gameObjectsToTurnIntoList[index], transform.position, newBulletRotation, team, entityData.GetObjectThatCreatedThisProjectile());
    }
}
