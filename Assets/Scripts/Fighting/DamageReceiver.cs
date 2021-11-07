using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] int health;
    [SerializeField] int team; //An entity must be in a team
    [SerializeField] bool isAnObstacle; //An obstacle doesn't have a team

    private bool isDestroyed = false;

    private void Start()
    {
        AddToLists();
    }
    public void ReceiveDamage(int damage)
    {
        health -= damage;
        CheckHealth();
    }
    private void CheckHealth()
    {
        if (!isDestroyed)
        {
            if (health <= 0)
            {
                RemoveFromLists();
                isDestroyed = true;
                Destroy(gameObject);
            }
        }
    }
    private void AddToLists()
    {
        if (isAnObstacle)
        {
            StaticDataHolder.AddObstacle(gameObject);
        }
        else
        {
            StaticDataHolder.AddEntity(gameObject);
        }
    }
    private void RemoveFromLists()
    {
        if (isAnObstacle)
        {
            StaticDataHolder.RemoveObstacle(gameObject);
        }
        else
        {
            StaticDataHolder.RemoveEntity(gameObject);
        }
    }
    public int GetCurrentHealth()
    {
        return health;
    }
    public int GetTeam()
    {
        return team;
    }
}
