using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using XNode;
using System.Linq;
using TMPro;

public class NodeReader : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    [Header("Components")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Instances")]
    public GameObject dialogueBoxGO;
    public GameObject dialogueChoiceButtonPrefab;
    public GameObject endConversationButtonPrefab;
    public Animator textAnimator;
    [Header("Button placement")]
    [SerializeField] Vector2 firstButtonPosition;
    [SerializeField] int yOffset;
    [SerializeField] int xOffset;
    [Header("Called Event List")]
    public List<string> calledEventList;

    [HideInInspector]
    public bool isDisplayingMessage = false;
    //Private variables
    private Coroutine textAnimationCoroutine;
    private bool isThisTheLastMessage = false;
    private List<GameObject> dialogueChoiceButtonList = new List<GameObject>();
    private Dictionary<GameObject, DialogueButton> dialogueButtonDictionary = new Dictionary<GameObject, DialogueButton>();
    string[] dialogueMessagesArray;

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
    //This is called by a dialogue trigger on an NPC
    public void StartDialogue(DialogueGraph inputDialogueGraph)
    {
        //Enable the dialogue box
        dialogueBoxGO.SetActive(true);
        textAnimator.SetBool("isClosed", false);

        //Initialize variables
        dialogueGraph = inputDialogueGraph;
        isDisplayingMessage = true;
        isThisTheLastMessage = false;

        SetStartingNode();

        ReadCurrentNode();
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
        dialogueMessagesArray = data.Split('/');
        //Check, if this is the last dialogue option
        if (IsThisTheLastMessage())
        {
            isThisTheLastMessage = true;
        }
        //Exception, when starting dialogue
        if (dialogueMessagesArray[0] == "Start")
        {
            GoToNextNodeThroughOutputName("exit");
        }
        //Default option
        if (dialogueMessagesArray[0] == "DialogueNode")
        {
            UpdateName(dialogueMessagesArray[1]);
            UpdateText(dialogueMessagesArray[2]);

            CreateNewDialogueChoiceButtons();
        }
        if (dialogueMessagesArray[0] == "ActionNode")
        {
            TriggerNodeActions();
            GoToNextNodeThroughOutputName("exit");
        }
    }
    public void NextSentence(object obj)
    {
        GameObject buttonGO = (GameObject)obj;

        BaseNode nodeToJumpTo = dialogueButtonDictionary[buttonGO].GetNodeToJumpTo();

        SetCurrentNode(nodeToJumpTo);
        ReadCurrentNode();

    }


    //Create buttons block Start
    private void CreateNewDialogueChoiceButtons()
    {
        DeletePreviousButtons();
        //Debug.Log("Creating buttons");
        DialogueNode dialogueNode = dialogueGraph.currentNode as DialogueNode;
        List<NodePort> outputList = dialogueNode.GetEnabledConnectedOutputs(dialogueGraph.currentNode);

        if (outputList != null)
        {
            if (outputList.Count != 0)
            {
                if (outputList.Count <= 3)
                {
                    //Make up to 3 big buttons
                    for (int i = 0; i < outputList.Count; i++)
                    {
                        NodePort outputPortForButton = outputList[i];
                        string portName = outputPortForButton.fieldName;
                        Debug.Log("Button index: " + GetThisPortIndex(outputPortForButton) + 3);
                        string buttonName = dialogueMessagesArray[GetThisPortIndex(outputPortForButton) + 3];

                        CreateBigButton(i, buttonName, portName);
                    }
                }
                else
                {
                    for (int i = 0; i < outputList.Count; i++)
                    {
                        NodePort outputPortForButton = outputList[i];
                        string portName = outputPortForButton.fieldName;
                        string buttonName = dialogueMessagesArray[GetThisPortIndex(outputPortForButton) + 3];

                        CreateSmallButton(i, buttonName, portName);
                    }
                }
            }
            else
            {
                CreateEndConversationButton(dialogueMessagesArray[3]);
            }
        }
        else
        {
            Debug.Log("A node should never have no outputs");
        }
    }
    private int GetThisPortIndex(NodePort portToCheck)
    {
        BaseNode baseNode = portToCheck.node as BaseNode;
        DialogueNode dialogueNode = baseNode as DialogueNode;
        List<NodePort> outputList = dialogueNode.GetConnectedOutputs(baseNode);
        for (int i = 0; i < outputList.Count; i++)
        {
            bool correctPortFound = portToCheck.fieldName == outputList[i].fieldName;
            if (correctPortFound)
            {
                return i;
            }
        }
        return -1;
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
    private void CreateSmallButton(int buttonIndex, string message, string portName)
    {
        GameObject dialogueButtonGO = Instantiate(dialogueChoiceButtonPrefab, Vector3.zero, Quaternion.identity, dialogueBoxGO.transform);
        dialogueButtonGO.GetComponent<RectTransform>().localPosition = CountButtonPosition(buttonIndex);

        dialogueChoiceButtonList.Add(dialogueButtonGO);

        BaseNode nextNode = FindNextNodeUsingOutputPortName(dialogueGraph.currentNode, portName);
        //Debug.Log("Next node name: " + nextNode);

        if (nextNode != null)
        {
            DialogueButton dialogueButton = new DialogueButton(nextNode);
            dialogueButtonDictionary.Add(dialogueButtonGO, dialogueButton);

            dialogueButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = message;

            AddEvent(dialogueButtonGO, EventTriggerType.PointerClick, delegate { OnDialogueChoiceButtonClicked(dialogueButtonGO); });
        }
        else
        {
            Debug.Log("This node's value should never be null, as CreateDialogueChoiceButtons should take care of that");
        }
    }
    private void CreateBigButton(int buttonIndex, string message, string portName)
    {
        GameObject dialogueButtonGO = Instantiate(endConversationButtonPrefab, Vector3.zero, Quaternion.identity, dialogueBoxGO.transform);
        Vector3 specialPositionVector = new Vector3(firstButtonPosition.x + (xOffset * 0.5f), firstButtonPosition.y + yOffset * buttonIndex, 0);
        dialogueButtonGO.GetComponent<RectTransform>().localPosition = specialPositionVector;

        dialogueChoiceButtonList.Add(dialogueButtonGO);

        BaseNode nextNode = FindNextNodeUsingOutputPortName(dialogueGraph.currentNode, portName);
        //Debug.Log("Next node name: " + nextNode);

        if (nextNode != null)
        {
            DialogueButton dialogueButton = new DialogueButton(nextNode);
            dialogueButtonDictionary.Add(dialogueButtonGO, dialogueButton);

            dialogueButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = message;

            AddEvent(dialogueButtonGO, EventTriggerType.PointerClick, delegate { OnDialogueChoiceButtonClicked(dialogueButtonGO); });
        }
        else
        {
            Debug.Log("This node's value should never be null, as CreateDialogueChoiceButtons should take care of that");
        }
    }
    private void CreateEndConversationButton(string message)
    {
        GameObject dialogueButtonGO = Instantiate(endConversationButtonPrefab, Vector3.zero, Quaternion.identity, dialogueBoxGO.transform);
        Vector3 specialPositionVector = new Vector3(firstButtonPosition.x + (xOffset * 0.5f), firstButtonPosition.y, 0);
        dialogueButtonGO.GetComponent<RectTransform>().localPosition = specialPositionVector;

        dialogueChoiceButtonList.Add(dialogueButtonGO);

        dialogueButtonGO.GetComponentInChildren<TextMeshProUGUI>().text = message;
        AddEvent(dialogueButtonGO, EventTriggerType.PointerClick, delegate { EndDialogue(); });
    }
    private Vector3 CountButtonPosition(int index)
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
    public void OnDialogueChoiceButtonClicked(GameObject obj)
    {
        EventManager.TriggerEvent("Next Dalogue Sentence", obj);
    }
    //Create buttons block End


    // Go to next node block Start
    private void GoToNextNodeThroughOutputName(string outputName)
    {
        //Moves to a next dialogue node
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
    private void SetNextNodeUsingOutputPortName(string outputName)
    {
        BaseNode returnedNode = FindNextNodeUsingOutputPortName(dialogueGraph.currentNode, outputName);
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
    private BaseNode FindNextNodeUsingOutputPortName(BaseNode currentNode, string outputName)
    {
        foreach (NodePort nodePort in currentNode.Outputs)
        {
            bool thisPortIsConnectedToSomeNode = nodePort.Connection != null;
            if (thisPortIsConnectedToSomeNode)
            {
                bool thisIsThePortIAmLookingFor = nodePort.fieldName == outputName;
                if (thisIsThePortIAmLookingFor)
                {
                    BaseNode returnNode = nodePort.Connection.node as BaseNode;
                    return returnNode;
                }
            }
        }
        return null;
    }
    private void SetCurrentNode(BaseNode nextNode)
    {
        dialogueGraph.currentNode = nextNode;
    }
    // Go to next node block End


    //Display text functions block Start
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
    //Display text functions block End


    //Read node helper functions block Start
    private void TriggerNodeActions()
    {
        int actionNumber = dialogueMessagesArray.Length;
        for (int i = 1; i < actionNumber; i++)
        {
            string actionName = dialogueMessagesArray[i];
            TriggerActionByName(actionName);
        }
    }
    private void TriggerActionByName(string actionName)
    {
        EventManager.TriggerEvent(actionName);
        if (calledEventList.Contains(actionName) == false)
        {
            Debug.Log("Added action:" + actionName);
            calledEventList.Add(actionName);
        }
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
    //Read node helper functions block End


    //Get enabled outputs block start
    private List<NodePort> GetEnabledConnectedOutputs(BaseNode node)
    {
        List<NodePort> outputList = GetConnectedOutputs(node);

        DeleteDisabledOutputsFromList(outputList);

        return outputList;
    }
    private void DeleteDisabledOutputsFromList(List<NodePort> outputList)
    {
        List<NodePort> portsToDelete = new List<NodePort>();
        foreach (NodePort port in outputList)
        {

            if (!IsNodeEnabled(port.node as BaseNode))
            {
                portsToDelete.Add(port);
            }
        }
        foreach (NodePort portToDelete in portsToDelete)
        {
            outputList.Remove(portToDelete);
        }

    }
    private bool IsNodeEnabled(BaseNode nodeToCheck)
    {
        List<NodePort> nodeInputList = GetConnectedInputs(nodeToCheck);
        foreach (NodePort port in nodeInputList)
        {
            if (port.fieldName == "enablingEvents")
            {
                ActionNode enablingEventActionNode = FindPreviousNodeUsingInputPortName(nodeToCheck, "enablingEvents") as ActionNode;
                if (!DoesCalledEventListContainAll(enablingEventActionNode.eventList))
                {
                    return false;
                }
            }
            if (port.fieldName == "disablingEvents")
            {
                ActionNode disablingEventActionNode = FindPreviousNodeUsingInputPortName(nodeToCheck, "disablingEvents") as ActionNode;
                if (!DoesCalledEventListContainAtLeastOne(disablingEventActionNode.eventList))
                {
                    return false;
                }
            }
        }

        return true;
    }
    private BaseNode FindPreviousNodeUsingInputPortName(BaseNode currentNode, string outputName)
    {
        foreach (NodePort nodePort in currentNode.Inputs)
        {
            bool thisPortIsConnectedToSomeNode = nodePort.Connection != null;
            if (thisPortIsConnectedToSomeNode)
            {
                bool thisIsThePortIAmLookingFor = nodePort.fieldName == outputName;
                if (thisIsThePortIAmLookingFor)
                {
                    BaseNode returnNode = nodePort.Connection.node as BaseNode;
                    return returnNode;
                }
            }
        }
        return null;
    }
    private bool DoesCalledEventListContainAll(List<string> events)
    {
        foreach (string element in events)
        {
            bool containsThisEvent = calledEventList.Contains(element);
            if (!containsThisEvent)
            {
                return false;
            }
        }
        return true;
    }
    private bool DoesCalledEventListContainAtLeastOne(List<string> events)
    {
        foreach (string element in events)
        {
            bool containsThisEvent = calledEventList.Contains(element);
            if (containsThisEvent)
            {
                return true;
            }
        }
        return false;
    }
    private List<NodePort> GetConnectedOutputs(BaseNode node)
    {
        List<NodePort> outputList = node.Outputs.ToList();
        DeleteDisconnectedPortsFromList(outputList);
        return outputList;
    }
    private void DeleteDisconnectedPortsFromList(List<NodePort> outputCollection)
    {
        List<NodePort> savePortsToDelete = new List<NodePort>();
        foreach (NodePort port in outputCollection)
        {
            bool thisPortIsDisconnected = port.Connection == null;
            if (thisPortIsDisconnected)
            {
                savePortsToDelete.Add(port);
            }
        }
        foreach (NodePort portToDelete in savePortsToDelete)
        {
            outputCollection.Remove(portToDelete);
        }

    }
    private List<NodePort> GetConnectedInputs(BaseNode node)
    {
        List<NodePort> inputList = node.Inputs.ToList();
        DeleteDisconnectedPortsFromList(inputList);
        return inputList;
    }
    //Get enabled outputs block end


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
