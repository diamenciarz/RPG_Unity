using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BasicProjectileController : MonoBehaviour, IEntityData
{
    [Header("Projectile Properties")]
    [SerializeField] protected int team;
    [SerializeField] protected List<Sprite> spriteList;
    [SerializeField] protected float startingSpeed = 2f;
    [SerializeField] protected int damage;

    [Header("Physics settings")]
    public bool isPushing = true;
    public float pushingPower;

    //Private variables
    [HideInInspector]
    public bool isAPlayerBullet = false;
    protected bool isDestroyed = false;
    protected Vector2 velocityVector;
    protected float creationTime;
    //Objects
    protected EntityCreator entityCreator;
    protected SpriteRenderer mySpriteRenderer;
    protected GameObject objectThatCreatedThisProjectile;


    protected virtual void Awake()
    {
        mySpriteRenderer = FindObjectOfType<SpriteRenderer>();
        entityCreator = FindObjectOfType<EntityCreator>();

        SetupStartingValues();
    }
    protected virtual void SetupStartingValues()
    {
        velocityVector = StaticDataHolder.GetDirectionVector(startingSpeed, transform.rotation.eulerAngles.z);
        creationTime = Time.time;
        SetSpriteAccordingToTeam();
    }
    protected virtual void Update()
    {
        MoveOneStep();
    }
    private void MoveOneStep()
    {
        transform.position += new Vector3(velocityVector.x, velocityVector.y, 0) * Time.deltaTime;
    }

    //Set values
    public void SetTeam(int newTeam)
    {
        team = newTeam;
        SetSpriteAccordingToTeam();
    }
    public void SetObjectThatCreatedThisProjectile(GameObject parentGameObject)
    {
        objectThatCreatedThisProjectile = parentGameObject;
    }
    private void SetSpriteAccordingToTeam()
    {
        if (spriteList.Count >= team && team != 0)
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
    public Vector3 GetVelocityVector3()
    {
        return new Vector3(velocityVector.x, velocityVector.y, 0);
    }
    public int GetDamage()
    {
        return damage;
    }
    public int GetTeam()
    {
        return team;
    }
    public GameObject GetObjectThatCreatedThisProjectile()
    {
        return objectThatCreatedThisProjectile;
    }
}
