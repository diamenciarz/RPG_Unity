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
                TrySetupStartingValues(summonedBullet, team, createdBy);
            }
            else
            {
                Debug.Log("Bullet did not fit");
                Destroy(summonedBullet);
            }
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
    public void SummonEntity(EntityTypes entityType, Vector3 summonPosition, Quaternion summonRotation, int team, GameObject parent)
    {
        GameObject entityToSummon = GetEntityPrefab(entityType);
        GameObject summonedEntity = Instantiate(entityToSummon, summonPosition, summonRotation, parent.transform);

        TrySetupStartingValues(summonedEntity, team, parent);
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
    private void TrySetupStartingValues(GameObject summonedObject, int team, GameObject createdBy)
    {
        TeamUpdater[] teamUpdaters = summonedObject.GetComponentsInChildren<TeamUpdater>();
        if (teamUpdaters.Length != 0)
        {
            foreach (TeamUpdater item in teamUpdaters)
            {
                item.SetTeam(team);

                if (createdBy)
                {
                    item.SetCreatedBy(createdBy);
                }
                else
                {
                    item.SetCreatedBy(summonedObject);
                }
            }
        }
    }
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
}
