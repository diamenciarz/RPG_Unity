using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float defaultPlayerSpeed = 10f;
    [SerializeField] float bushSpeedModifier = 0.4f;
    public bool canDash = true;

    private float playerSpeed;
    private int forceMultiplier = 100;
    private float moveDistanceThisFrame;

    private Vector3 dashVector;
    public Vector3 moveVectorThisFrame;

    private Rigidbody2D myRigidbody2D;
    private BoxCollider2D myCollider2D;
    private List<GameObject> collidingObjectsList = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        playerSpeed = defaultPlayerSpeed;
        myCollider2D = GetComponent<BoxCollider2D>();
        myRigidbody2D = GetComponent<Rigidbody2D>();
        EventManager.TriggerEvent("SetPlayerGameObject", gameObject);

    }


    // Update is called once per frame
    void FixedUpdate()
    {
        AdjustMovementSpeed();

        Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
        moveVectorThisFrame = ((inputVector * playerSpeed) + dashVector) * Time.deltaTime;
        moveDistanceThisFrame = moveVectorThisFrame.magnitude;

        RotateTowardsMoveVector(moveVectorThisFrame);
        MoveIfPossible();
        //MoveUsingPhysics();
        //BounceOffWalls();
    }
    private void RotateTowardsMoveVector(Vector3 moveVector)
    {
        transform.rotation = Quaternion.FromToRotation(Vector3.up, moveVector);
    }
    private void MoveUsingPhysics()
    {
        myRigidbody2D.AddForce(new Vector2(moveVectorThisFrame.x * Time.deltaTime * playerSpeed * forceMultiplier, moveVectorThisFrame.y * Time.deltaTime * playerSpeed * forceMultiplier));
    }
    private void BounceOffWalls()
    {
        RaycastHit2D moveHit2D;

        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(moveVectorThisFrame.x, 0), moveDistanceThisFrame, LayerMask.GetMask("Actors", "Obstacles"));
        if (moveHit2D.collider != null)
        {
            myRigidbody2D.velocity = new Vector2(-myRigidbody2D.velocity.x, myRigidbody2D.velocity.y);
        }
        if (moveHit2D.collider != null)
        {
            myRigidbody2D.velocity = new Vector2(myRigidbody2D.velocity.x, -myRigidbody2D.velocity.y);
        }
    }
    private void MoveIfPossible()
    {
        RaycastHit2D moveHit2D;
        //Check for collisions on the X-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(moveVectorThisFrame.x, 0), moveDistanceThisFrame, LayerMask.GetMask("Actors", "Obstacles"));
        if (IsBlocked(moveHit2D))
        {
            transform.position += new Vector3(moveVectorThisFrame.x, 0, 0);
        }

        //Check for collisions on theY-axis
        moveHit2D = Physics2D.BoxCast(transform.position, myCollider2D.size, 0, new Vector2(0, moveVectorThisFrame.y), moveDistanceThisFrame, LayerMask.GetMask("Actors", "Obstacles"));
        if (IsBlocked(moveHit2D))
        {
            transform.position += new Vector3(0, moveVectorThisFrame.y, 0);
        }
    }
    private bool IsBlocked(RaycastHit2D moveHit2D)
    {
        bool canMove = moveHit2D.collider == null || moveHit2D.collider.tag == "Dashable";
        return canMove;
    }
    private void AdjustMovementSpeed()
    {
        if (IsCollidingWithABush())
        {
            playerSpeed = defaultPlayerSpeed * bushSpeedModifier;
        }
        else
        {
            playerSpeed = defaultPlayerSpeed;
        }
    }
    private bool IsCollidingWithABush()
    {
        foreach (GameObject item in collidingObjectsList)
        {
            if (item.tag == "Dashable")
            {
                return true;
            }
        }
        return false;
    }


    //Collision handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
        AddCollidingObject(collision.gameObject);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        RemoveCollidingObject(collision.gameObject);
    }
    private void AddCollidingObject(GameObject objectToAdd)
    {
        collidingObjectsList.Add(objectToAdd);
    }
    private void RemoveCollidingObject(GameObject objectToAdd)
    {
        if (collidingObjectsList.Contains(objectToAdd))
        {
            collidingObjectsList.Remove(objectToAdd);
        }
    }
}
