using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiver : MonoBehaviour
{
    [SerializeField] int health;

    private bool isDestroyed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ReceiveDamage(int damage)
    {
        health -= damage;
        CheckHealth();
    }
    private void CheckHealth()
    {
        if (health <= 0)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
}
