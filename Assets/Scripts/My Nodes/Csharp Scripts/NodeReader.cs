using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using XNode;

public class NodeReader : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    [Header("Components")]
    public Text nameText;
    public Text dialogueText;

    [Header("Instances")]
    public GameObject dialogueBoxGO;
    public GameObject dialogueChoiceButtonPrefab;
    public Animator textAnimator;
    [Header("Button placement")]
    [SerializeField] Vector2 firstButtonPosition;
    [SerializeField] int yOffset;
    [SerializeField] int xOffset;

    [HideInInspector]
    public bool isDisplayingMessage = false;
    //Private variables
    private Coroutine textAnimationCoroutine;
    private bool isThisTheLastMessage = false;
    private List<GameObject> dialogueChoiceButtonList = new List<GameObject>();
    private Dictionary<GameObject, DialogueButton> dialogueButtonDictionary = new Dictionary<GameObject, DialogueButton>();

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
    public void NextSentence(object obj)
    {
        GameObject buttonGO = (GameObject)obj;

        BaseNode nodeToJumpTo = dialogueButtonDictionary[buttonGO].GetNodeToJumpTo();
        SetCurrentNode(nodeToJumpTo);
    }
    //This is called by a dialogue trigger on an NPC
    public IEnumerator StartDialogue(DialogueGraph inputDialogueGraph)
    {
        //Wait for the screen to get ready
        float waitTimeToEliminateLag = 1;
        dialogueBoxGO.SetActive(true);
        textAnimator.SetBool("isClosed", false);

        //Set text to default
        nameText.text = dialogueGraph.name;
        dialogueText.text = "";

        //Create new dialogue choice buttons

        //Initialize variables
        dialogueGraph = inputDialogueGraph;
        isDisplayingMessage = true;
        isThisTheLastMessage = false;

        SetStartingNode();

        ReadCurrentNode();
        yield return new WaitForSeconds(waitTimeToEliminateLag);
    }
    private void SetStartingNode()
    {
        foreach (BaseNode baseNode in dialogueGraph.nodes)
        {
            if (baseNode.GetString() == "Start")
            {
                dialogueGraph.currentNode = baseNode;
                break;
            }
        }
    }

    public void ReadCurrentNode()
    {
        BaseNode baseNode = dialogueGraph.currentNode;
        string data = baseNode.GetString();
        string[] stringArray = data.Split('/');
        //Check, if this is the last dialogue option
        if (IsThisTheLastMessage())
        {
            isThisTheLastMessage = true;
        }
        //Exception, when starting dialogue
        if (stringArray[0] == "Start")
        {
            Debug.Log("Starting dialogue");
            GoToNextNodeThroughOutputName("exit");

        }
        //Default option
        if (stringArray[0] == "DialogueNode")
        {
            UpdateName(stringArray[1]);
            UpdateText(stringArray[2]);

            CreateNewDialogueChoiceButtons();
        }
    }
    private void CreateNewDialogueChoiceButtons()
    {
        DeletePreviousButtons();

        ICollection outputCollection = dialogueGraph.currentNode.Outputs as ICollection;
        if (outputCollection != null)
        {
            for (int i = 0; i < outputCollection.Count; i++)
            {
                string portName = "Choice" + (i + 1);
                CreateSingleButton(i, "Choice", portName);
            }
        }
        else
        {
            string portName = "Choice1";
            CreateSingleButton(1, "End conversation", portName);
        }
    }
    private void CreateSingleButton(int buttonIndex, string message, string portName)
    {
        GameObject dialogueButtonGO = Instantiate(dialogueChoiceButtonPrefab, Vector3.zero, Quaternion.identity);

        dialogueBoxGO.GetComponent<RectTransform>().position = ReturnButtonPosition(buttonIndex);
        dialogueButtonGO.transform.SetParent(dialogueBoxGO.transform);
        //dialogueBoxGO.GetComponent<RectTransform>().

        dialogueChoiceButtonList.Add(dialogueButtonGO);

        BaseNode nextNode = FindNextNodeUsingOutputPortName(portName);
        if (nextNode != null)
        {
            DialogueButton dialogueButton = new DialogueButton(nextNode);
            dialogueButtonDictionary.Add(dialogueButtonGO, dialogueButton);

            dialogueButtonGO.GetComponentInChildren<Text>().text = message;

            AddEvent(dialogueButtonGO, EventTriggerType.PointerClick, delegate { OnDialogueChoiceButtonClicked(dialogueButtonGO); }); 
        }
        else
        {
            Debug.Log("This node's value should never be null, as CreateDialogueChoiceButtons should take care of that");
        }


    }
    private void DeletePreviousButtons()
    {
        foreach (GameObject button in dialogueChoiceButtonList)
        {
            Destroy(button);
        }
        dialogueButtonDictionary = new Dictionary<GameObject, DialogueButton>();
        dialogueChoiceButtonList.Clear();
    }
    private Vector3 ReturnButtonPosition(int index)
    {
        if (index > 5)
        {
            Debug.Log("Can only make max 6 buttons");
            return new Vector3(firstButtonPosition.x, firstButtonPosition.y, 0);
        }
        else
        {
            Vector3 returnVector = new Vector3(firstButtonPosition.x + xOffset * (index % 2), firstButtonPosition.y + yOffset * (index / 2), 0);
            return returnVector;
        }
    }
    protected void OnDialogueChoiceButtonClicked(GameObject obj)
    {
        EventManager.TriggerEvent("Next Dalogue Sentence", obj);
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
    //Moves to a next dialogue node - important
    private void GoToNextNodeThroughOutputName(string outputName)
    {
        if (!isThisTheLastMessage)
        {
            SetNextNodeUsingOutputPortName(outputName);
            ReadCurrentNode();
        }
        else
        {
            EndDialogue();
        }
    }
    private void SetNextNodeUsingOutputPortName(string outputName)
    {
        BaseNode returnedNode = FindNextNodeUsingOutputPortName(outputName);
        if (returnedNode != null)
        {
            //Set a new graph as current
            SetCurrentNode(returnedNode);
        }
        else
        {
            Debug.Log("Next Node wasn't found - name doesn't exist");
        }
    }
    private BaseNode FindNextNodeUsingOutputPortName(string outputName)
    {

        foreach (NodePort nodePort in dialogueGraph.currentNode.Outputs)
        {
            if (nodePort.fieldName == outputName)
            {
                Debug.Log("Connection name: " + nodePort.Connection.fieldName);
                BaseNode returnNode = nodePort.Connection.node as BaseNode;
                return returnNode;
            }
        }
        return null;
    }
    private void SetCurrentNode(BaseNode nextNode)
    {
        dialogueGraph.currentNode = nextNode;
    }
    public void EndDialogue()
    {
        textAnimator.SetBool("isClosed", true);
        isDisplayingMessage = false;
        EventManager.TriggerEvent("StartPopupCoroutine");
    }
    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    public class DialogueButton
    {
        private BaseNode nodeToJumpTo;

        public DialogueButton(BaseNode inputNode)
        {
            nodeToJumpTo = inputNode;
        }
        public BaseNode GetNodeToJumpTo()
        {
            return nodeToJumpTo;
        }
    }
}
