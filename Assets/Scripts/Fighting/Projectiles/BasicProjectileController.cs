using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : OnCollisionDamage, ICollidingEntityData
{
    [Header("Projectile Properties")]
    [SerializeField] protected List<Sprite> spriteList;
    [SerializeField] protected float startingSpeed = 2f;

    
    //Private variables
    protected Vector2 velocityVector;
    //Objects
    protected EntityCreator entityCreator;
    protected GameObject objectThatCreatedThisProjectile;
    //Components
    protected SpriteRenderer mySpriteRenderer;
    protected Rigidbody2D myRigidbody2D;


    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    protected virtual void Start()
    {
        SetVelocityVector(StaticDataHolder.GetDirectionVector(startingSpeed, transform.rotation.eulerAngles.z));
        SetSpriteAccordingToTeam();
    }
    private void SetupStartingValues()
    {
        mySpriteRenderer = FindObjectOfType<SpriteRenderer>();
        entityCreator = FindObjectOfType<EntityCreator>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        
        creationTime = Time.time;
    }
    protected virtual void Update()
    {
        //MoveOneStep(); //now moving through rigidbody2D
    }
    private void MoveOneStep()
    {
        transform.position += new Vector3(velocityVector.x, velocityVector.y, 0) * Time.deltaTime;
    }

    //Set values
    public void SetObjectThatCreatedThisProjectile(GameObject parentGameObject)
    {
        objectThatCreatedThisProjectile = parentGameObject;
    }
    private void SetSpriteAccordingToTeam()
    {
        if (spriteList.Count >= team && team > 0)
        {
            try
            {
                mySpriteRenderer.sprite = spriteList[team - 1];
            }
            catch (System.Exception)
            {
                Debug.LogError("Bullet sprite list out of bounds. Index: " + (team - 1));
                throw;
            }
        }
    }
    public void SetVelocityVector(Vector2 newVelocityVector)
    {
        velocityVector = newVelocityVector;
        myRigidbody2D.velocity = newVelocityVector;
    }
    public void ModifyVelocityVector3(Vector3 deltaVector)
    {
        SetVelocityVector(GetVelocityVector3() + deltaVector);
    }

    //Accessor methods
    public Vector2 GetVelocityVector2()
    {
        return velocityVector;
    }
    public GameObject GetObjectThatCreatedThisProjectile()
    {
        return objectThatCreatedThisProjectile;
    }
}
