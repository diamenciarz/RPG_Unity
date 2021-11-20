using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Single Shot", menuName = "Shots/Salvo")]
public class SalvoScriptableObject : ScriptableObject, ISerializationCallbackReceiver
{
    public SingleShotScriptableObject[] shots;
    public List<float> delayAfterEachShot;
    public List<float> reloadDelays;

    public float additionalReloadTime;

    #region Serialization
    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        if (delayAfterEachShot.Count < shots.Length)
        {
            delayAfterEachShot.Add(0);
        }
        if (reloadDelays.Count < shots.Length)
        {
            reloadDelays.Add(0);
        }
        if (reloadDelays.Count > shots.Length)
        {
            reloadDelays.RemoveAt(reloadDelays.Count -1);
        }
        if (delayAfterEachShot.Count > shots.Length)
        {
            delayAfterEachShot.RemoveAt(delayAfterEachShot.Count - 1);
        }
    }
    #endregion
}
