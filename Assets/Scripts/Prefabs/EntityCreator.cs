using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    private void OnEnable()
    {
        //EventManager.StartListening("SummonProjectile", SummonProjectile);
    }
    private void OnDisable()
    {
        //EventManager.StopListening("SummonProjectile", SummonProjectile);

    }
    [Header("Projectiles")]
    public GameObject laserPrefab;
    public GameObject splittingBulletPrefab;
    public GameObject rocketPrefab;
    public GameObject bombPrefab;
    public GameObject explosionPrefab;
    [Header("Enemies")]
    public GameObject walkerPrefab;
    public enum EntityTypes
    {
        Walker
    }
    public enum BulletTypes
    {
        Nothing,
        SplittingBullet,
        Laser,
        Rocket,
        Bomb,
        Explosion
    }
    //Projectiles
    public void SummonProjectile(BulletTypes bulletType, Vector3 summonPosition, Quaternion summonRotation, int team, GameObject createdBy)
    {
        GameObject bulletToSummon = GetProjectilePrefab(bulletType);
        if (bulletToSummon != null)
        {
            GameObject summonedBullet = Instantiate(bulletToSummon, summonPosition, summonRotation);

            TrySetupProjectileStartingValues(summonedBullet, team, createdBy);
            StaticDataHolder.GetProjectileList().Add(summonedBullet);
        }
    }
    private void TrySetupProjectileStartingValues(GameObject summonedBullet, int team, GameObject createdBy)
    {
        BasicProjectileController basicProjectileController = summonedBullet.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            basicProjectileController.SetTeam(team);
            basicProjectileController.SetObjectThatCreatedThisProjectile(createdBy);
        }
    }
    private GameObject GetProjectilePrefab(BulletTypes bulletType)
    {
        if (bulletType == BulletTypes.Laser)
        {
            return laserPrefab;
        }
        if (bulletType == BulletTypes.SplittingBullet)
        {
            return splittingBulletPrefab;
        }
        if (bulletType == BulletTypes.Rocket)
        {
            return rocketPrefab;
        }
        if (bulletType == BulletTypes.Bomb)
        {
            return bombPrefab;
        }
        if (bulletType == BulletTypes.Explosion)
        {
            return explosionPrefab;
        }
        return null;
    }
    public bool IsThisProjectileARocket(BulletTypes projectile)
    {
        if (projectile == BulletTypes.Rocket)
        {
            return true;
        }
        return false;
    }


    //Entities
    public void SummonEntity(EntityTypes entityType, Vector3 summonPosition, Quaternion summonRotation, int team, GameObject parent)
    {
        GameObject entityToSummon = GetEntityPrefab(entityType);
        GameObject summonedEntity = Instantiate(entityToSummon, summonPosition, summonRotation, parent.transform);

        TrySetupEntityStartingValues(summonedEntity, team, parent);
        StaticDataHolder.GetEntityList().Add(summonedEntity);
    }
    private void TrySetupEntityStartingValues(GameObject summonedEntity, int team, GameObject parent)
    {
        TeamUpdater[] damageReceivers = summonedEntity.GetComponentsInChildren<TeamUpdater>();
        if (damageReceivers.Length != 0)
        {
            foreach (TeamUpdater item in damageReceivers)
            {
                item.SetTeam(team);
            }
        }
    }
    private GameObject GetEntityPrefab(EntityTypes entityType)
    {
        if (entityType == EntityTypes.Walker)
        {
            return walkerPrefab;
        }
        return null;
    }
}
