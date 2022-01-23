using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVehicleMover
{
    #region Movement
    public abstract void StartAccelerating();
    public abstract void StopAccelerating();
    public abstract void StartBraking();
    public abstract void StopBraking();
    #endregion

    #region Rotation
    public abstract void StartTurningRight();
    public abstract void StopTurningRight();
    public abstract void StartTurningLeft();
    public abstract void StopTurningLeft();
    #endregion

}
