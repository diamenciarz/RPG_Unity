using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode;

public class NodeReader : MonoBehaviour
{
    public DialogueGraph dialogueGraphTemporary;
    public DialogueGraph dialogueGraph;
    Coroutine readNodeCoroutine;

    public Text nameText;
    public Text dialogueText;
    public Text buttonText;
    public GameObject dialogueBoxGO;

    public Animator textAnimator;

    [HideInInspector]
    public bool isDisplayingMessage = false;
    //Private variables
    private bool nextSentence;
    private bool isThisTheLastMessage = false;
    private Coroutine textAnimationCoroutine;

    private void OnEnable()
    {
        EventManager.StartListening("Next Dalogue Sentence", NextSentence);
        EventManager.StartListening("PlayerLeftDialogue", EndDialogue);
    }
    private void OnDisable()
    {
        EventManager.StopListening("Next Dalogue Sentence", NextSentence);
        EventManager.StopListening("PlayerLeftDialogue", EndDialogue);

    }
    public void NextSentence()
    {
        nextSentence = true;
    }
    //This is called by a dialogue trigger on an NPC
    public IEnumerator StartDialogue(DialogueGraph inputDialogueGraph)
    {
        //Set text to default
        nameText.text = dialogueGraph.name;
        dialogueText.text = "";
        buttonText.text = "Continue...";
        //Initialize variables
        dialogueGraph = inputDialogueGraph;
        isDisplayingMessage = true;
        isThisTheLastMessage = false;
        dialogueBoxGO.SetActive(true);
        textAnimator.SetBool("isClosed", false);

        foreach (BaseNode bn in dialogueGraph.nodes)
        {
            if (bn.GetString() == "Start")
            {
                dialogueGraph.currentNode = bn;
                break;
            }
        }

        yield return new WaitForSeconds(0.6f);

        readNodeCoroutine = StartCoroutine(ReadNode());
    }

    IEnumerator ReadNode()
    {
        BaseNode baseNode = dialogueGraph.currentNode;
        string data = baseNode.GetString();
        string[] stringArray = data.Split('/');
        //Exception, when starting dialogue
        if (stringArray[0] == "Start")
        {
            GoToNextNodeThroughOutput("exit");

        }
        if (IsThisTheLastMessage())
        {
            isThisTheLastMessage = true;
            buttonText.text = "End conversation";
        }
        //
        if (stringArray[0] == "DialogueNode")
        {
            UpdateName(stringArray[1]);
            UpdateText(stringArray[2]);

            //Wait for one click, before going further with the dialogue
            yield return new WaitUntil(() => nextSentence == true);
            nextSentence = false;

            GoToNextNodeThroughOutput("exit");
        }
    }
    private bool IsThisTheLastMessage()
    {
        bool AreAllPortsDisconnected = true;
        foreach (NodePort nodePort in dialogueGraph.currentNode.Outputs)
        {
            bool isCurrentNodeConnected = nodePort.IsConnected == true;
            if (isCurrentNodeConnected)
            {
                AreAllPortsDisconnected = false;
                break;
            }
        }
        return AreAllPortsDisconnected;
    }
    private void UpdateName(string newName)
    {
        nameText.text = newName;
    }
    private void UpdateText(string nextSentenceToSay)
    {
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        textAnimationCoroutine = StartCoroutine(AnimateSentence(nextSentenceToSay));
    }
    IEnumerator AnimateSentence(string inputSentence)
    {
        dialogueText.text = "";
        foreach (char letter in inputSentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void GoToNextNodeThroughOutput(string outputName)
    {
        StopTheLastReadNodeCoroutine();
        if (!isThisTheLastMessage)
        {
            FindAndSetNextNodeUsingOutputPortName(outputName);

            //Start a new ReadNode coroutine
            readNodeCoroutine = StartCoroutine(ReadNode());
        }
        else
        {
            EndDialogue();
        }
    }
    private void StopTheLastReadNodeCoroutine()
    {
        if (readNodeCoroutine != null)
        {
            StopCoroutine(readNodeCoroutine);
            readNodeCoroutine = null;
        }
    }
    private void FindAndSetNextNodeUsingOutputPortName(string outputName)
    {
        foreach (NodePort nodePort in dialogueGraph.currentNode.Ports)
        {
            if (nodePort.fieldName == outputName)
            {
                //Set a new graph as current
                dialogueGraph.currentNode = nodePort.Connection.node as BaseNode;
                break;
            }
        }
    }
    public void EndDialogue()
    {
        textAnimator.SetBool("isClosed", true);
        isDisplayingMessage = false;
        EventManager.TriggerEvent("StartPopupCoroutine");
    }

}
