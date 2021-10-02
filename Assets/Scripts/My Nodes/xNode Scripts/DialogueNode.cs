using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class DialogueNode : BaseNode {

	[Input] public int entry;
	[Output] public int Choice1;
	[Output] public int Choice2;
	[Output] public int Choice3;
	[Output] public int Choice4;

	public string speakerName;
	public string dialogue;

    public override string GetString()
    {
        return ("DialogueNode/" + speakerName + "/" + dialogue);
    }
}