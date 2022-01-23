using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOnDeath : TeamUpdater
{
    [Header("Upon Breaking")]
    [SerializeField] protected List<EntityCreator.BulletTypes> gameObjectsToTurnIntoList;

    [SerializeField] protected bool shootsAtEnemies; //Otherwise shoots forward
    [Tooltip("The direction of the shot, when not shooting at enemies")]
    [SerializeField] protected float shotDirection = 0;
    [SerializeField] protected bool spreadProjectilesEvenly;
    [SerializeField] protected float spreadDegrees;
    [SerializeField] protected float leftBulletSpread;
    [SerializeField] protected float rightBulletSpread;

    protected EntityCreator entityCreator;
    private bool isDestroyed;
    protected float deltaRotationToTarget = -90;
    private GameObject target;
    private void Awake()
    {
        entityCreator = FindObjectOfType<EntityCreator>();
    }

    #region OnDestroy
    public void DestroyObject()
    {
        if (!isDestroyed)
        {
            isDestroyed = true;
            CreateNewProjectiles();
            StartCoroutine(DestroyAtTheEndOfFrame());
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
                    target = StaticDataHolder.GetClosestEnemy(transform.position, team);
                    if (target != null)
                    {
                        ShootAtTarget(target.transform.position, i);
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
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }
    #endregion

    #region OneShot
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
        SummonedProjectileData data = new SummonedProjectileData();
        data.summonRotation = RotForwardRandomSpread();
        data.summonPosition = transform.position;
        data.team = team;
        data.createdBy = createdBy;
        data.bulletType = gameObjectsToTurnIntoList[index];
        data.target = target;
        
        entityCreator.SummonProjectile(data);
    }
    private Quaternion RotForwardRandomSpread()
    {
        Quaternion newBulletRotation = HelperMethods.RandomRotationInRange(leftBulletSpread, rightBulletSpread);
        newBulletRotation *= transform.rotation * Quaternion.Euler(0, 0, shotDirection);
        return newBulletRotation;
    }

    private void ShootOnceForwardWithRegularSpread(int index)
    {
        SummonedProjectileData data = new SummonedProjectileData();
        data.summonRotation = RotForwardRegularSpread(index);
        data.summonPosition = transform.position;
        data.team = team;
        data.createdBy = createdBy;
        data.bulletType = gameObjectsToTurnIntoList[index];
        data.target = target;

        entityCreator.SummonProjectile(data);
    }
    private Quaternion RotForwardRegularSpread(int index)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset + shotDirection);
        newBulletRotation *= transform.rotation;
        return newBulletRotation;
    }

    private void ShootOnceTowardsPositionWithRandomSpread(int index, Vector3 shootAtPosition)
    {
        SummonedProjectileData data = new SummonedProjectileData();
        data.summonRotation = RotToPosRandomSpread(shootAtPosition);
        data.summonPosition = transform.position;
        data.team = team;
        data.createdBy = createdBy;
        data.bulletType = gameObjectsToTurnIntoList[index];
        data.target = target;

        entityCreator.SummonProjectile(data);
    }
    private Quaternion RotToPosRandomSpread(Vector3 shootAtPosition)
    {
        Quaternion newBulletRotation = HelperMethods.RandomRotationInRange(leftBulletSpread, rightBulletSpread);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(transform.position, shootAtPosition);
        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, deltaRotationToTarget);
        return newBulletRotation;
    }

    private void ShootOnceTowardsPositionWithRegularSpread(int index, Vector3 shootAtPosition)
    {
        SummonedProjectileData data = new SummonedProjectileData();
        data.summonRotation = RotToPosRegularSpread(index,shootAtPosition);
        data.summonPosition = transform.position;
        data.team = team;
        data.createdBy = createdBy;
        data.bulletType = gameObjectsToTurnIntoList[index];
        data.target = target;

        entityCreator.SummonProjectile(data);
    }
    private Quaternion RotToPosRegularSpread(int index,Vector3 shootAtPosition)
    {
        float bulletOffset = (spreadDegrees * (index - (gameObjectsToTurnIntoList.Count - 1f) / 2));
        Quaternion newBulletRotation = Quaternion.Euler(0, 0, bulletOffset);
        Quaternion rotationToTarget = HelperMethods.DeltaPositionRotation(transform.position, shootAtPosition);
        newBulletRotation *= rotationToTarget * Quaternion.Euler(0, 0, deltaRotationToTarget);
        return newBulletRotation;
    }
    #endregion
}
