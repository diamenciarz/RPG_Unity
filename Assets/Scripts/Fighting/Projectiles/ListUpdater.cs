using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ListUpdater : MonoBehaviour
{
    [Header("Add to lists")]
    [SerializeField] List<AddToLists> putInLists = new List<AddToLists>();

    public enum AddToLists
    {
        Projectile,
        PlayerProjectile,
        Entity,
        Obstacle,
        DashableObject
    }


    protected void OnEnable()
    {
        AddObjectToLists();
    }
    protected void OnDisable()
    {
        RemoveObjectFromLists();
    }
    protected void OnDestroy()
    {
        RemoveObjectFromLists();
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

    public bool ListContains(AddToLists element)
    {
        if (putInLists.Contains(element))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
