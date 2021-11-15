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

    //Private variables
    private float originalSize;

    protected override void Start()
    {
        base.Start();
        SetupStartingValues();
    }
    protected override void Update()
    {
        base.Update();
        SetNewSize();
    }


    private void SetupStartingValues()
    {
        originalSize = transform.localScale.x;
    }
    private void SetNewSize()
    {
        float newSize = (Time.time - creationTime) / timeToExpire * (bombSize - originalSize) + originalSize;
        gameObject.transform.localScale = new Vector3(newSize, newSize, 0);

        if (Time.time - creationTime > timeToExpire)
        {
            gameObject.transform.localScale = new Vector3(bombSize, bombSize, 0);

            Destroy(gameObject);
        }
    }
}


