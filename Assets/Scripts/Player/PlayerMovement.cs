using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 10f;

    private RaycastHit2D moveHit2D;
    private Rigidbody2D myRigidbody2D;
    private BoxCollider2D myCollider2D;
    private Vector3 moveVectorThisFrame;
    private float moveDistanceThisFrame;
    // Start is called before the first frame update
    void Start()
    {
        myCollider2D = GetComponent<BoxCollider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveVectorThisFrame = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),0);
        moveDistanceThisFrame = moveVectorThisFrame.magnitude * Time.deltaTime * playerSpeed;

        RotateTowardsMoveVector(moveVectorThisFrame);
        MoveIfPossible();
    }
    private void RotateTowardsMoveVector(Vector3 moveVector)
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up,moveVector);
    }
    private void MoveIfPossible()
    {
        //Check for collisions on the X-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(moveVectorThisFrame.x, 0), moveDistanceThisFrame, LayerMask.GetMask("Actors","Obstacles"));
        if (moveHit2D.collider == null)
        {
            transform.position += new Vector3(moveVectorThisFrame.x * Time.deltaTime * playerSpeed,0,0);
        }

        //Check for collisions on theY-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(0, moveVectorThisFrame.y), moveDistanceThisFrame, LayerMask.GetMask("Actors", "Obstacles"));
        if (moveHit2D.collider == null)
        {
            transform.position +=new Vector3(0, moveVectorThisFrame.y * Time.deltaTime * playerSpeed, 0);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Dialogue")
        {
            Debug.Log("Hey");
        }
    }
}
