using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] bool controlledByPlayer;

    #region Private variables
    IVehicleMover myVehicle;
    #endregion

    #region Startup
    void Start()
    {
        StartupMethods();
    }
    private void StartupMethods()
    {
        myVehicle = GetComponent<IVehicleMover>();
    }
    #endregion

    #region Update
    void Update()
    {
        CheckInputs();
    }
    private void CheckInputs()
    {
        if (controlledByPlayer)
        {
            CheckPlayerInputs();
        }
    }
    private void CheckPlayerInputs()
    {
        CheckMovementInputs();
        CheckRotationInputs();
    }
    private void CheckMovementInputs()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            myVehicle.StartAccelerating();
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            myVehicle.StopAccelerating();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            myVehicle.StartBraking();
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            myVehicle.StopBraking();
        }
    }
    private void CheckRotationInputs()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            myVehicle.StartTurningLeft();
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            myVehicle.StopTurningLeft();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            myVehicle.StartTurningRight();
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            myVehicle.StopTurningRight();
        }
    }
    #endregion
}
