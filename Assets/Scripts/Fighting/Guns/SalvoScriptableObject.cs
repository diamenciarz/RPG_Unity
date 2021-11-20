using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Single Shot", menuName = "Shots/Salvo")]
public class SalvoScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    public SingleShotScriptableObject[] shots;
    public float[] delayAfterEachShot;

    public float additionalReloadTime;

    #region Serialization
    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {

    }
    #endregion
}
