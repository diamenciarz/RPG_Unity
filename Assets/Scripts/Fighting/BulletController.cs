using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{

    [Header("Bullet Settings")]
    [SerializeField] protected bool isPiercing;


    //Private variables
    protected int currentDamageLeft;

    protected override void Update()
    {
        base.Update();
    }
    protected override void SetupStartingValues()
    {
        base.SetupStartingValues();
        if (isPiercing)
        {
            currentDamageLeft = damage;
        }
    }
}


