using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    [SerializeField] bool controlledByPlayer = true;

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
            ApplyPlayerInputs();
        }
    }
    private void ApplyPlayerInputs()
    {
        Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        myVehicle.SetInputVector(inputVector);
    }
    #endregion
}
