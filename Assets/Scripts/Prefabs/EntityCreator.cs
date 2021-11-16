using UnityEngine;

public class EntityCreator : MonoBehaviour
{
    private void OnEnable()
    {
        //EventManager.StartListening("SummonProjectile", SummonProjectile);
        //This was a proble, because there were too many inputs for the create method to puth through an object
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
        }
    }
    private void TrySetupProjectileStartingValues(GameObject summonedBullet, int team, GameObject createdBy)
    {
        BasicProjectileController basicProjectileController = summonedBullet.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            basicProjectileController.SetObjectThatCreatedThisProjectile(createdBy);
        }
        else
        {
            Debug.Log("Did not find BasicProjectileController script");
        }
        TeamUpdater[] teamUpdaters = summonedBullet.GetComponentsInChildren<TeamUpdater>();
        if (teamUpdaters.Length != 0)
        {
            foreach (TeamUpdater item in teamUpdaters)
            {
                Debug.Log("Set team of " + item.name + " to " + team);
                item.SetTeam(team);
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
    }
    private void TrySetupEntityStartingValues(GameObject summonedEntity, int team, GameObject parent)
    {
        TeamUpdater[] teamUpdaters = summonedEntity.GetComponentsInChildren<TeamUpdater>();
        if (teamUpdaters.Length != 0)
        {
            foreach (TeamUpdater item in teamUpdaters)
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
