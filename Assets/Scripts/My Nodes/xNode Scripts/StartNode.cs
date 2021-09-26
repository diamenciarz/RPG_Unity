using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

public class StartNode : BaseNode {

	[Output] public int exit;

    public override string GetDialogue()
    {
        return "Start";
    }
}