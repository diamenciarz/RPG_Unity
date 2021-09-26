using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueNode : BaseNode {

	[Input] public int entry;
	[Output] public int exit;

	public string speakerName;
	public string dialogue;

    public override string GetString()
    {
        return ("DialogueNode/" + speakerName + "/" + dialogue);
    }
}