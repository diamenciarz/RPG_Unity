using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{
    [Header("Wall bouncing")]
    [SerializeField] bool bounceOffObstacles = false;
    [SerializeField] float destroyDelay = 5f;
    [SerializeField] int maxBounces = 3;
    [Tooltip("How much time to add to the bullet's lifetime after a bounce")]
    [SerializeField] float timeToAdd = 2f;

    private float destroyTime;
    private int bounces;
    private Vector3 collisionNormal;
    private CapsuleCollider2D myCollider2D;

    protected override void Start()
    {
        base.Start();

        SetupStartingVariables();
        StartCoroutine(CheckDestroyDelay());
        StartCoroutine(FirstFrameCollisionCheck());
    }
    protected void FixedUpdate()
    {
        UpdateCollisionNormal();
    }
    private void SetupStartingVariables()
    {
        myCollider2D = GetComponent<CapsuleCollider2D>();
        SetupDestroyTime();
    }

    #region Destroy
    private void SetupDestroyTime()
    {
        destroyTime = Time.time + destroyDelay;
    }
    private IEnumerator CheckDestroyDelay()
    {
        yield return new WaitUntil(() => destroyTime < Time.time);
        Destroy(gameObject);
    }
    private bool AddBounceTime()
    {
        if (bounces < maxBounces)
        {
            bounces++;
            destroyTime += timeToAdd;
            return true;
        }
        else
        {
            Destroy(gameObject);
            return false;
        }
    }
    #endregion

    #region Collisions
    private IEnumerator FirstFrameCollisionCheck()
    {
        yield return new WaitForEndOfFrame();
        if (myCollider2D.IsTouchingLayers(-1))
        {
            Debug.Log("Is touching");
            DestroyObject();
        }
    }
    private void UpdateCollisionNormal()
    {
        RaycastHit2D hit2D = Physics2D.Raycast(transform.position, GetVelocityVector3(), GetVelocityVector3().magnitude);
        if (hit2D.collider)
        {
            collisionNormal = hit2D.normal;
            Debug.DrawRay(transform.position, GetVelocityVector3() * hit2D.fraction, Color.red, 0.1f);
        }
    }

    #region OnCollisionEnter
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        BounceCheck(collision);
    }
    private void BounceCheck(Collider2D collision)
    {
        if (BouncesOffObstacle(collision.gameObject))
        {
            if (AddBounceTime())
            {
                Bounce();
            }
        }
    }
    private bool BouncesOffObstacle(GameObject collisionObject)
    {
        bool isAnObstacle = false;
        ListUpdater listUpdater = collisionObject.GetComponent<ListUpdater>();

        if (listUpdater)
        {
            isAnObstacle = listUpdater.ListContains(ListUpdater.AddToLists.Obstacle);
        }

        isAnObstacle = isAnObstacle || collisionObject.tag == "Obstacle";
        return isAnObstacle && bounceOffObstacles;
    }
    private void Bounce()
    {
        transform.position -= GetVelocityVector3() * Time.deltaTime;
        Vector3 newVelocity = Vector3.Reflect(GetVelocityVector3(), collisionNormal);
        SetVelocityVector(newVelocity);
    }
    #endregion

    #endregion
}


