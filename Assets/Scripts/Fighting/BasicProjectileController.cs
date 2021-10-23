using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : MonoBehaviour
{
    [SerializeField] protected float speed;
    [SerializeField] protected int bulletDamage;

    protected bool isDestroyed = false;

    protected abstract void Update();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageReceiver damageReceiver = collision.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            damageReceiver.ReceiveDamage(bulletDamage);
            DestroyProjectile();
        }
        if (collision.tag == "Obstacle")
        {
            DestroyProjectile();
        }
    }

    public void DestroyProjectile()
    {
        Destroy(gameObject);
    }
}
