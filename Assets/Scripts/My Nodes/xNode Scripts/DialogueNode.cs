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
	[TextArea(3,100)]
	public string dialogue;
	[Header("Player's Answers")]
	public string choice1Text = "Null";
	public string choice2Text = "Null";
	public string choice3Text = "Null";
	public string choice4Text = "Null";

    public override string GetString()
    {
        return ("DialogueNode/" + speakerName + "/" + dialogue + "/" + choice1Text + "/" + choice2Text + "/" + choice3Text + "/" + choice4Text);
    }
}