using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombController : BasicProjectileController
{

    [Header("Bomb Settings")]
    // Ustawienia dla bomby
    public bool hurtsPlayer;
    public float timeToExpire;
    [SerializeField] float bombSize; // Sprite scale
    [SerializeField] float rotateDuringLifetime;

    //Private variables
    private float originalSize;
    private float originalZRotation;

    protected override void Awake()
    {
        base.Awake();
        SetupStartingValues();
    }
    private void SetupStartingValues()
    {
        originalSize = transform.localScale.x;
        originalZRotation = transform.rotation.eulerAngles.z;
    }


    protected override void Update()
    {
        base.Update();
        UpdateTransform();
        CheckLifetime();
    }
    private void UpdateTransform()
    {
        float lifetimePercentage = (Time.time - creationTime) / timeToExpire;
        UpdateScale(lifetimePercentage);
        UpdateRotation(lifetimePercentage);
    }
    private void UpdateScale(float lifetimePercentage)
    {
        float newSize = lifetimePercentage * (bombSize - originalSize) + originalSize;
        gameObject.transform.localScale = new Vector3(newSize, newSize, 0);
    }
    private void UpdateRotation(float lifetimePercentage)
    {
        Quaternion newRotation = Quaternion.Euler(0, 0, originalZRotation + lifetimePercentage * rotateDuringLifetime);
        transform.rotation = newRotation;
    }
    private void CheckLifetime()
    {
        if (Time.time - creationTime > timeToExpire)
        {
            gameObject.transform.localScale = new Vector3(bombSize, bombSize, 0);

            Destroy(gameObject);
        }
    }
}


