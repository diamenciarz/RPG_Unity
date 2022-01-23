using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{
    [Header("Timed destroy")]
    [Tooltip("Destroy the bullet after it has existed for this long. -1 for infinity")]
    [SerializeField] float destroyDelay = 5f;
    [Tooltip("Destroy the bullet after it has travelled this much distance. -1 for infinity")]
    [SerializeField] float destroyDistance = 1;

    [Header("Wall bouncing")]
    public List<BreaksOn> bounceEnum = new List<BreaksOn>();
    [Tooltip("Min angle from the collision's normal to reflect")]
    [SerializeField] float minAngleToReflect = 45;
    [Tooltip("-1 for infinite bounces")]
    [SerializeField] int maxReflections = 3;
    [Tooltip("How much time to add to the bullet's lifetime after a reflection")]
    [SerializeField] float timeToAdd = 2f;

    private float destroyTime;
    private int bounces;
    private Vector3 collisionNormal;
    private CapsuleCollider2D myCollider2D;
    private bool timedDestroy = false;

    protected override void Start()
    {
        base.Start();

        SetupStartingVariables();
        if (timedDestroy)
        {
            StartCoroutine(CheckDestroyDelay());
        }
    }
    private void SetupStartingVariables()
    {
        myCollider2D = GetComponent<CapsuleCollider2D>();
        SetupDestroyTime();
    }

    #region Destroy
    private void SetupDestroyTime()
    {
        float destroyDistanceDelay = CountDistanceDelay();
        bool distanceDelayExists = destroyDistanceDelay != -1;
        bool destroyTimeExists = destroyDelay != -1;
        destroyTime = Time.time;

        if (destroyTimeExists && distanceDelayExists)
        {
            if (destroyDistanceDelay < destroyDelay)
            {
                destroyTime += destroyDistanceDelay;
            }
            else
            {
                destroyTime += destroyDelay;
            }
            timedDestroy = true;
            return;
        }
        if (destroyTimeExists)
        {
            destroyTime += destroyDelay;
            timedDestroy = true;
            return;
        }
        if (distanceDelayExists)
        {
            destroyTime += destroyDistanceDelay;
            timedDestroy = true;
            return;
        }
        destroyTime += 100;
    }
    private float CountDistanceDelay()
    {
        if (destroyDistance != -1)
        {
            return destroyDistance / myRigidbody2D.velocity.magnitude;
        }
        else
        {
            return -1;
        }

    }
    private IEnumerator CheckDestroyDelay()
    {
        yield return new WaitUntil(() => destroyTime < Time.time);
        DestroyObject();
    }
    #endregion

    #region Collisions
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);
        BounceCheck(collision);
        StartCoroutine(WaitAndUpdateRotation());
    }
    private void BounceCheck(Collision2D collision)
    {
        if (!ShouldReflect(collision))
        {
            DestroyObject();
        }
    }
    private IEnumerator WaitAndUpdateRotation()
    {
        yield return new WaitForEndOfFrame();
        UpdateRotationToFaceForward();
    }
    private bool ShouldReflect(Collision2D collision)
    {
        Vector3 collisionNormal = collision.GetContact(0).normal;
        float hitAngle = Vector3.Angle(GetVelocityVector3(), collisionNormal);
        //The collision angle has to be bigger than "minAngleToReflect" for the bullet to not get destroyed
        bool isAngleBigEnough = Mathf.Abs(hitAngle) >= minAngleToReflect;
        //Some bullets have a limited number of bounces
        bool hasBouncesLeft = maxReflections == -1 || bounces < maxReflections;

        return hasBouncesLeft && isAngleBigEnough;
    }
    #endregion
}


