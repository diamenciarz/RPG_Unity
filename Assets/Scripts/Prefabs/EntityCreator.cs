using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    [Header("Projectiles")]
    public GameObject laserPrefab;
    public GameObject splittingBulletPrefab;
    public GameObject rocketPrefab;
    public GameObject bombPrefab;
    public GameObject bombExplosionPrefab;
    public GameObject grenadePrefab;
    public GameObject grenadeExplosionPrefab;
    public GameObject bouncyLaserPrefab;

    [Header("Enemies")]
    public GameObject walkerPrefab;

    public enum BulletTypes
    {
        Nothing,
        SplittingBullet,
        Laser,
        Rocket,
        Bomb,
        BombExplosion,
        Grenade,
        GrenadeExplosion,
        BouncyLaser
    }
    public enum EntityTypes
    {
        Walker
    }

    #region Projectiles
    public void SummonProjectile(BulletTypes bulletType, Vector3 summonPosition, Quaternion summonRotation, int team, GameObject createdBy)
    {
        GameObject bulletToSummon = GetProjectilePrefab(bulletType);
        if (bulletToSummon != null)
        {
            GameObject summonedBullet = Instantiate(bulletToSummon, summonPosition, summonRotation);
            if (CanFitSummon(summonedBullet))
            {
                TrySetupStartingValues(summonedBullet, data);
            }
            else
            {
                Debug.Log("Bullet did not fit");
                Destroy(summonedBullet);
            }
        }
    }
    public void SummonProjectile(SummonedProjectileData data)
    {
        GameObject bulletToSummon = GetProjectilePrefab(data.bulletType);
        if (bulletToSummon != null)
        {
            GameObject summonedBullet = Instantiate(bulletToSummon, data.summonPosition, data.summonRotation);
            SetupStartingValues(summonedBullet, data.team, data.createdBy);
            CheckForRocket(summonedBullet, data);
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
        if (bulletType == BulletTypes.BombExplosion)
        {
            return bombExplosionPrefab;
        }
        if (bulletType == BulletTypes.Grenade)
        {
            return grenadePrefab;
        }
        if (bulletType == BulletTypes.GrenadeExplosion)
        {
            return grenadeExplosionPrefab;
        }
        if (bulletType == BulletTypes.BouncyLaser)
        {
            return bouncyLaserPrefab;
        }
        return null;
    }
    #endregion

    #region Entities
    public void SummonEntity(SummonedEntityData data)
    {
        GameObject entityToSummon = GetEntityPrefab(data.entityType);
        GameObject summonedEntity = Instantiate(entityToSummon, data.summonPosition, data.summonRotation);

        SetupStartingValues(summonedEntity, data.team, data.parent);
    }
    private GameObject GetEntityPrefab(EntityTypes entityType)
    {
        if (entityType == EntityTypes.Walker)
        {
            return walkerPrefab;
        }
        return null;
    }
    #endregion

    #region Helper methods
    private void SetupStartingValues(GameObject summonedObject, int team, GameObject parent)
    {
        TeamUpdater[] teamUpdaters = summonedObject.GetComponentsInChildren<TeamUpdater>();
        if (teamUpdaters.Length != 0)
        {
            foreach (TeamUpdater item in teamUpdaters)
            {
                item.SetTeam(team);
                SetCreatedBy(item, parent);
            }
        }
    }
    private void SetCreatedBy(TeamUpdater item, GameObject createdBy)
    {
        if (createdBy)
        {
            item.SetCreatedBy(createdBy);
        }
        else
        {
            item.SetCreatedBy(item.gameObject);
        }
    }
    private void CheckForRocket(GameObject summonedObject, SummonedProjectileData data)
    {
        RocketController rocketController = summonedObject.GetComponent<RocketController>();
        rocketController.SetTarget(data.target);
    }

    #region Unused
    private bool CanFitSummon(GameObject summonedObject)
    {
        Vector3 dir = HelperMethods.DirectionVectorNormalized(transform.rotation.eulerAngles.z);
        ContactFilter2D filter = CreateObstacleContactFilter();
        RaycastHit2D[] hits = new RaycastHit2D[0];

        Collider2D collider = summonedObject.GetComponent<Collider2D>();
        collider.Cast(dir, filter, hits);

        bool hitSomeObstacle = hits.Length != 0;
        if (hitSomeObstacle)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    private ContactFilter2D CreateObstacleContactFilter()
    {
        LayerMask layerMask = LayerMask.GetMask("Obstacles");
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(layerMask);

        return filter;
    } 
    #endregion

    #endregion
}
public class SummonedProjectileData
{
    public EntityCreator.BulletTypes bulletType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject createdBy;
    public GameObject target;
}
public class SummonedEntityData
{
    public EntityCreator.EntityTypes entityType;
    public Vector3 summonPosition;
    public Quaternion summonRotation;
    public int team;
    public GameObject parent;
    public GameObject target;
}