using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] int health;
    [SerializeField] int team;

    private bool isDestroyed = false;
    
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
                isDestroyed = true;
                Destroy(gameObject);
            }
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
