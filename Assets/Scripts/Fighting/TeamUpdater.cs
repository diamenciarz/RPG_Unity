using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamUpdater : MonoBehaviour
{
    protected int team;

    protected virtual void Start()
    {
        UpdateTeam();
    }
    private void OnEnable()
    {
        EventManager.StartListening("ChangedObjectTeam", CheckTeamChange);
    }
    private void OnDisable()
    {
        EventManager.StopListening("ChangedObjectTeam", CheckTeamChange);
    }
    private void CheckTeamChange(object changedObject)
    {
        if ((GameObject)changedObject == gameObject)
        {
            UpdateTeam();
        }
    }
    private void UpdateTeam()
    {
        team = -1;
        DamageReceiver damageReceiver = GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            team = damageReceiver.GetTeam();
            return;
        }
        DamageReceiver damageReceiverInParent = GetComponentInParent<DamageReceiver>();
        if (damageReceiverInParent != null)
        {
            team = damageReceiverInParent.GetTeam();
            return;
        }
        else
        {
            Debug.LogError("Entity has no team component");
        }
    }


    //Set methods
    public virtual void SetTeam(int newTeam)
    {
        team = newTeam;
        EventManager.TriggerEvent("ChangedObjectTeam", gameObject);
    }


    //Accessor Methods
    public int GetTeam()
    {
        return team;
    }
}
