using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerObjectController : MonoBehaviour
{
    [SerializeField] string nextStateTriggerMessage;
    [SerializeField] string setStateTriggerMessage;
    [SerializeField] List<GameObject> nextStateGOs;
    private int currentStateIndex = 0;
    private void Start()
    {
        SetState(currentStateIndex);
    }
    private void OnEnable()
    {
        EventManager.StartListening(nextStateTriggerMessage, NextState);
        EventManager.StartListening(setStateTriggerMessage, TrySetState);
    }
    private void OnDisable()
    {
        EventManager.StopListening(nextStateTriggerMessage, NextState);
        EventManager.StartListening(setStateTriggerMessage, TrySetState);

    }
    public void NextState()
    {
        SetState(currentStateIndex + 1);
    }
    /// <summary>
    /// Tries to set state value to an int.
    /// Throws an exception, if input is not an integer
    /// </summary>
    public void TrySetState(object inputObject)
    {
        try
        {
            string newIndexString = (string)inputObject;
            int newIndex = -1;
            if (Int32.TryParse(newIndexString, out newIndex))
            {
                SetState(newIndex);
            }
            else
            {
                Debug.LogError("Trigger value - "+ newIndexString + " - was not an integer");
            }
        }
        catch (System.Exception)
        {
            Debug.LogError("Trigger value was not an integer");
            throw;
        }
    }

    private void SetState(int newIndex)
    {
        nextStateGOs[currentStateIndex].SetActive(false);
        currentStateIndex = newIndex;
        nextStateGOs[currentStateIndex].SetActive(true);
    }
    public void ChangeNextStateTriggerMessage(string newTriggerMessage)
    {
        nextStateTriggerMessage = newTriggerMessage;
    }
    public void ChangeSetStateTriggerMessage(string newTriggerMessage)
    {
        setStateTriggerMessage = newTriggerMessage;
    }
}
