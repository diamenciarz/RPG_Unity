using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityController : MonoBehaviour
{
    [SerializeField] bool controlledByPlayer = true;

    #region Private variables
    IEntityMover myVehicle;
    #endregion

    #region Startup
    void Start()
    {
        StartupMethods();
    }
    private void StartupMethods()
    {
        myVehicle = GetComponent<IEntityMover>();
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
