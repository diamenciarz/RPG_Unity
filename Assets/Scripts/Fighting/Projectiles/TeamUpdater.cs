using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamUpdater : MonoBehaviour
{
    //[HideInInspector]

    public int team = -1;


    //Set methods
    /// <summary>
    /// Change team of the whole gameObject. Use ChangeTeamTo() to change team of this script
    /// </summary>
    /// <param name="newTeam"></param>
    public virtual void SetTeam(int newTeam)
    {
        team = newTeam;
        UpdateTeam(newTeam);
    }
    private void UpdateTeam(int newTeam)
    {
        TeamUpdater[] teamUpdater = GetComponentsInChildren<TeamUpdater>();
        foreach (TeamUpdater item in teamUpdater)
        {
            item.ChangeTeamTo(newTeam);
        }
        DamageReceiver[] damageReceivers = GetComponentsInChildren<DamageReceiver>();
        foreach (DamageReceiver item in damageReceivers)
        {
            item.ChangeTeamTo(newTeam);
        }
    }
    /// <summary>
    /// Change team of this script. Use SetTeam() to change team of the whole gameObject
    /// </summary>
    /// <param name="newTeam"></param>
    public void ChangeTeamTo(int newTeam)
    {
        team = newTeam;
    }

    //Accessor Methods
    public int GetTeam()
    {
        return team;
    }

    #region Serialization
    public void OnBeforeSerialize()
    {
        DamageReceiver damageReceiver = GetComponentInParent<DamageReceiver>();
        if (damageReceiver)
        {
            team = damageReceiver.GetTeam();
            return;

        }
        BasicProjectileController basicProjectileController = GetComponentInParent<BasicProjectileController>();
        if (basicProjectileController)
        {
            team = basicProjectileController.GetTeam();
            return;
        }
    }
    public void OnAfterDeserialize()
    {

    }
    #endregion
}