using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class ActionNode : BaseNode {

	[Input] public int entry;
	[Output] public int exit;

    [Header("Actions")]
    public List<string> eventList = new List<string>();

    public override string GetString()
    {
        string actions = "";
        foreach (string actionString in eventList)
        {
            actions += "/";
            actions += actionString;
        }
        return ("ActionNode" + actions);
    }
}