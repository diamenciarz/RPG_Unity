using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOnDeath : MonoBehaviour
{
    [Header("Add to lists")]
    [SerializeField] List<AddToLists> putInLists = new List<AddToLists>();

    [Header("Upon Breaking")]
    [SerializeField] protected List<EntityCreator.BulletTypes> gameObjectsToTurnIntoList;

    [SerializeField] protected bool shootsAtEnemies; //Otherwise shoots forward
    [SerializeField] protected float basicDirection;
    [SerializeField] protected bool spreadProjectilesEvenly;
    [SerializeField] protected float spreadDegrees;
    [SerializeField] protected float leftBulletSpread;
    [SerializeField] protected float rightBulletSpread;

    public enum AddToLists
    {
        Projectile,
        PlayerProjectile,
        Entity,
        Obstacle,
        DashableObject
    }

    private IEntityData entityData;


    protected EntityCreator entityCreator;
    private int team;

    private void Start()
    {
        entityCreator = FindObjectOfType<EntityCreator>();
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


    private void OnEnable()
    {
        AddObjectToLists();
    }
    private void OnDisable()
    {
        RemoveObjectFromLists();
    }


    //Destroy methods
    public void DestroyObject()
    {
        RemoveObjectFromLists();
        CreateNewProjectiles();
        StartCoroutine(DestroyAtTheEndOfFrame());
    }
    private IEnumerator DestroyAtTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }


    //Modify lists
    protected void AddObjectToLists()
    {
        if (putInLists.Contains(AddToLists.Projectile))
        {
            StaticDataHolder.AddProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.PlayerProjectile))
        {
            StaticDataHolder.AddPlayerProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.Obstacle))
        {
            StaticDataHolder.AddObstacle(gameObject);
        }
        if (putInLists.Contains(AddToLists.Entity))
        {
            StaticDataHolder.AddEntity(gameObject);
        }
        if (putInLists.Contains(AddToLists.DashableObject))
        {
            StaticDataHolder.AddDashableObject(gameObject);
        }
    }
    protected void RemoveObjectFromLists()
    {
        if (putInLists.Contains(AddToLists.Projectile))
        {
            StaticDataHolder.RemoveProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.PlayerProjectile))
        {
            StaticDataHolder.RemovePlayerProjectile(gameObject);
        }
        if (putInLists.Contains(AddToLists.Obstacle))
        {
            StaticDataHolder.RemoveObstacle(gameObject);
        }
        if (putInLists.Contains(AddToLists.Entity))
        {
            StaticDataHolder.RemoveEntity(gameObject);
        }
        if (putInLists.Contains(AddToLists.DashableObject))
        {
            StaticDataHolder.RemoveDashableObject(gameObject);
        }
    }


    //Death summon
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
