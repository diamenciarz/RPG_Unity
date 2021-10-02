using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class NodeReader : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    Coroutine reader;

    public Text speakerName;
    public Text dialogue;

    private void Start()
    {
        foreach (BaseNode bn in dialogueGraph.nodes)
	    {
            if (bn.GetString() == "Start")
            {
                dialogueGraph.current = bn;
                break;
            }
	    }
        reader = StartCoroutine(ReadNode());
    }
    IEnumerator ReadNode()
    {
        BaseNode baseNode = dialogueGraph.current;
        string data = baseNode.GetString();
        string[] stringArray = data.Split('/');
        //Exception, when starting dialogue
        if (stringArray[0] == "Start")
        {
            GoToNextNodeThroughOutput("Exit");

        }
        //
        if (stringArray[0] == "DialogueNode")
        {
            speakerName.text = stringArray[1];
            dialogue.text = stringArray[2];
            //Wait for one click, before going further with the dialogue
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
            yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

            GoToNextNodeThroughOutput("Exit");
        }
    }
    private void GoToNextNodeThroughOutput(string outputName)
    {
        if (reader != null)
        {
            StopCoroutine(reader);
            reader = null;
        }
        foreach (NodePort np in dialogueGraph.current.Ports)
        {
            if (np.fieldName == outputName)
            {
                //Set 
                dialogueGraph.current = np.Connection.node as BaseNode;
                break;
            }
        }
        reader = StartCoroutine(ReadNode());
    }
}
