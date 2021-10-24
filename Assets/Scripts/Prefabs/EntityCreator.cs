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
    [Header("Enemies")]
    public GameObject walkerPrefab;
    public enum EntityTypes
    {
        Walker
    }
    public enum BulletTypes
    {
        Laser
    }
    //Projectiles
    public void SummonProjectile(BulletTypes bulletType, Vector3 summonPosition, Quaternion summonRotation, int team, GameObject createdBy)
    {
        GameObject bulletToSummon = GetProjectilePrefab(bulletType);
        GameObject summonedBullet = Instantiate(bulletToSummon, summonPosition, summonRotation);

        TrySetupProjectileStartingValues(summonedBullet, team, createdBy);
        StaticDataHolder.projectileList.Add(summonedBullet);
    }
    private void TrySetupProjectileStartingValues(GameObject summonedBullet, int team, GameObject createdBy)
    {
        BasicProjectileController basicProjectileController = summonedBullet.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            basicProjectileController.SetBulletTeam(team);
            basicProjectileController.SetObjectThatCreatedThisProjectile(createdBy);
        }
    }
    private GameObject GetProjectilePrefab(BulletTypes bulletType)
    {
        if (bulletType == BulletTypes.Laser)
        {
            return laserPrefab;
        }
        return null;
    }
    //Entities
    public void SummonEntity(EntityTypes entityType, Vector3 summonPosition, Quaternion summonRotation, int team)
    {
        GameObject entityToSummon = GetEntityPrefab(entityType);
        GameObject summonedEntity = Instantiate(entityToSummon, summonPosition, summonRotation);

        TrySetupEntityStartingValues(summonedEntity, team);
        StaticDataHolder.entityList.Add(summonedEntity);
    }
    private void TrySetupEntityStartingValues(GameObject summonedBullet, int team)
    {
        /*
        BasicProjectileController basicProjectileController = summonedBullet.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            basicProjectileController.SetBulletTeam(team);
            basicProjectileController.SetObjectThatCreatedThisProjectile(createdBy);
        }*/
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
