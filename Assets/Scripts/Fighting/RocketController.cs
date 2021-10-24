using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : BasicProjectileController
{
    public GameObject targetGameObject;
    [SerializeField] GameObject objectMissingIconGameObject;
    public bool hurtsPlayer;

    [Header("Rocket Flight Settings")]
    public float timeToExpire;
    [SerializeField] float targetRocketSpeed;
    [SerializeField] float speedChangeRatePerSecond = 1f;
    public float currentRocketSpeed = -1f;
    public float rocketRotationSpeed;

    [Header("Explosion Settings")]
    [SerializeField] int howManyBulletsAtOnce;
    [SerializeField] bool shootsAtPlayer;

    [SerializeField] float leftBulletSpread;
    [SerializeField] float rightBulletSpread;
    [SerializeField] int damage = 0;

    GameObject newBullet;

    protected void Start()
    {
        CreateMiaIcon();
        if (currentRocketSpeed == -1)
        {
            currentRocketSpeed = targetRocketSpeed;
        }
    }
    public void SetTarget(GameObject target)
    {
        targetGameObject = target;
    }
    protected void CheckForTarget()
    {
        if (myTeam != 0)
        {
            if (targetGameObject == null)
            {
                targetGameObject = StaticDataHolder.GetTheNearestEnemy(transform.position, myTeam);
            }
        }
    }
}
