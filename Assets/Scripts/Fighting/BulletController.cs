using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : BasicProjectileController
{

    [Header("Bullet Settings")]
    // Ustawienia dla bomby
    public bool isABomb;
    public bool hurtsPlayer;
    public float timeToExpire;
    [SerializeField] float bombSize;

    public float directionalBulletSpeed = 5f;
    

    //Private variables
    private float originalSize;
    // Update is called once per frame
    protected override void Update()
    {
        MoveOneStep();

        if (isABomb)
        {
            SetNewSize();
        }
    }
    private void MoveOneStep()
    {
        transform.position += new Vector3(velocityVector.x, velocityVector.y, 0) * Time.deltaTime;
    }
    protected override void SetupStartingValues()
    {
        velocityVector = StaticDataHolder.GetVectorRotatedInDirection(speed, transform.rotation.eulerAngles.z);
        creationTime = Time.time;
        if (isABomb)
        {
            originalSize = transform.localScale.x;
        }
    }

    private void SetNewSize()
    {
        // Wylicza now¹ skalê z pomoc¹ aktualnej skali, maksymalnej skali, czasu roœniêcia oraz Time.deltatime;
        float newSize = (Time.time - creationTime) / timeToExpire * (bombSize - originalSize) + originalSize;
        //Ustawia skalê na w³aœnie wyliczon¹
        gameObject.transform.localScale = new Vector3(newSize, newSize, 0);
        if (Time.time - creationTime > timeToExpire)
        {
            gameObject.transform.localScale = new Vector3(bombSize, bombSize, 0);

            DestroyProjectile();
        }
    }

    public void SetNewBulletSpeedVector(Vector2 newVelocityVector)
    {
        velocityVector = newVelocityVector;
    }
    public Vector2 GetVelocityVector()
    {
        return velocityVector;
    }
}


