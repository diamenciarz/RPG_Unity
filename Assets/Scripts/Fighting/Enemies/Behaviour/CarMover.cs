using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour, IVehicleMover
{
    #region Serialization
    [Tooltip("The highest speed that the vehicle can accelerate towards")]
    [SerializeField] float maxSpeed;
    [Tooltip("The speed change rate, when accelerating")]
    [SerializeField] float acceleratingSpeed;
    [Tooltip("The speed change rate, when braking")]
    [SerializeField] float braingSpeed;
    [Tooltip("The rotation angle change rate, when turning (in degrees)")]
    [SerializeField] float turningSpeed;
    #endregion

    #region Private variables
    //Objects
    Rigidbody2D myRigidbody2D;
    //Movement
    private bool isBraking = false;
    private bool isAccelerating = false;
    //Rotation
    private bool isTurningRight = false;
    private bool isTurningLeft = false;
    
    #endregion

    void Start()
    {
        SetupVariables();
    }
    private void SetupVariables()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
    }

    #region Update
    void FixedUpdate()
    {
        Rotate();
        Move();
    }
    #region Movement
    //Public methods
    public void StartAccelerating()
    {
        isAccelerating = true;
    }
    public void StopAccelerating()
    {
        isAccelerating = false;
    }
    public void StartBraking()
    {
        isBraking = true;
    }
    public void StopBraking()
    {
        isBraking = false;
    }
    //Private methods
    private void Move()
    {
        if (isAccelerating)
        {
            Accelerate();
        }
        if (isBraking)
        {
            Brake();
        }
    }
    private void Accelerate()
    {
        Vector2 force = transform.rotation.eulerAngles.normalized * acceleratingSpeed;
        myRigidbody2D.AddForce(force * Time.fixedDeltaTime);
    }
    private void Brake()
    {
        Vector2 force = -1 * transform.rotation.eulerAngles.normalized * braingSpeed;
        myRigidbody2D.AddForce(force * Time.fixedDeltaTime);
    }
    #endregion

    #region Rotation
    //Public methods
    public void StartTurningRight()
    {
        isTurningRight = true;
    }
    public void StopTurningRight()
    {
        isTurningRight = false;
    }
    public void StartTurningLeft()
    {
        isTurningLeft = true;
    }
    public void StopTurningLeft()
    {
        isTurningLeft = false;
    }
    //Private methods
    private void Rotate()
    {
        if (isTurningLeft)
        {
            TurnLeft();
        }
        if (isTurningRight)
        {
            TurnRight();
        }
    }
    private void TurnLeft()
    {
        myRigidbody2D.AddTorque(turningSpeed * Time.fixedDeltaTime);
    }
    private void TurnRight()
    {
        myRigidbody2D.AddTorque(-1 * turningSpeed * Time.fixedDeltaTime);
    }
    #endregion
    #endregion

    #region Accessor methods
    //Physics
    public Vector2 GetVelocity()
    {
        return myRigidbody2D.velocity;
    }
    public float GetSpeed()
    {
        return myRigidbody2D.velocity.magnitude;
    }
    //Movement
    public float GetBrakingSpeed()
    {
        return braingSpeed;
    }
    public float GetAcceleratingSpeed()
    {
        return acceleratingSpeed;
    }
    public float GetTurningSpeed()
    {
        return turningSpeed;
    }
    //State
    public bool GetIsTurningRight()
    {
        return isTurningRight;
    }
    public bool GetIsTurningLeft()
    {
        return isTurningLeft;
    }
    public bool GetIsAccelerating()
    {
        return isAccelerating;
    }
    public bool GetIsBraking()
    {
        return isBraking;
    }
    #endregion
}
