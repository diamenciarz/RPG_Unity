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
    private DialogueBoxController dialogueBoxController;
    public GameObject dialogueChoiceButtonPrefab;
    public GameObject endConversationButtonPrefab;
    public GameObject bigPopupWindowPrefab;
    public GameObject smallPopupWindowPrefab;
    public Animator textWindowAnimator;
    [Header("Button placement")]
    [SerializeField] Vector2 firstButtonPosition;
    [SerializeField] int yOffset;
    [SerializeField] int xOffset;
    [Header("Called Event List")]
    public List<string> calledEventList;

    [HideInInspector]
    public bool isDisplayingMessage = false;
    //Private variables
    private Transform popupTransformForPlacement;
    private Coroutine textAnimationCoroutine;
    private bool isThisTheLastMessage = false;
    private List<GameObject> dialogueChoiceButtonList = new List<GameObject>();
    private Dictionary<GameObject, DialogueButton> dialogueButtonDictionary = new Dictionary<GameObject, DialogueButton>();
    string[] dialogueMessagesArray;
    string[] dialogueValuesArray;

    private void Awake()
    {
        dialogueBoxController = dialogueBoxGO.GetComponent<DialogueBoxController>();
    }
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
    public void StartDialogue(DialogueGraph inputDialogueGraph, Transform popupTransform)
    {
        //Enable the dialogue box
        dialogueBoxGO.SetActive(true);

        //Initialize variables
        popupTransformForPlacement = popupTransform;
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
        SaveNodeMessages();

        //Check, if dialogue box should be closed
        if (dialogueMessagesArray[0] != "DialogueNode")
        {
            CloseDialogueWindow();
        }
        //Check, if this is the last dialogue option
        if (IsThisTheLastMessage())
        {
            isThisTheLastMessage = true;
        }
        //Exception, when starting dialogue
        if (dialogueMessagesArray[0] == "Start")
        {
            HandleStartNode();
            return;
        }
        //Default option
        if (dialogueMessagesArray[0] == "DialogueNode")
        {
            UpdateDialogueBox();
            return;
        }
        if (dialogueMessagesArray[0] == "PopupNode")
        {
            CreateTextPopup();
            return;
        }
        if (dialogueMessagesArray[0] == "ActionNode")
        {
            TriggerNodeActions();
            GoToNextNodeThroughOutputName("exit");
            return;
        }
    }
    private void SaveNodeMessages()
    {
        BaseNode baseNode = dialogueGraph.currentNode;
        string data = baseNode.GetString();
        dialogueMessagesArray = data.Split('/');
    }
    public void NextSentence(object obj)
    {
        GameObject buttonGO = (GameObject)obj;

        BaseNode nodeToJumpTo = dialogueButtonDictionary[buttonGO].GetNodeToJumpTo();

        SetCurrentNode(nodeToJumpTo);
        ReadCurrentNode();
    }
    private void GoToRandomNextNode(List<NodePort> outputPortList)
    {
        int index = Random.Range(0, outputPortList.Count);
        NodePort outputPortForButton = outputPortList[index];
        string portName = outputPortForButton.fieldName;

        GoToNextNodeThroughOutputName(portName);
    }


    //Create text popup
    private void CreateTextPopup()
    {
        GameObject popupInstance = Instantiate(smallPopupWindowPrefab, popupTransformForPlacement.position, Quaternion.identity, popupTransformForPlacement);
        SetPopupTextMessages(popupInstance);
        //Start destroy coroutine
        float destroyDelay;
        bool destroyDurationCorrect = float.TryParse(dialogueMessagesArray[1], out destroyDelay);
        if (destroyDurationCorrect)
        {
            StartCoroutine(DestroyPopupAfterTime(destroyDelay, popupInstance));
        }
        else
        {
            GoToNextNodeThroughOutputName("nextNode");
        }
    }
    private void SetPopupTextMessages(GameObject popupInstance)
    {
        PopupController popupController = popupInstance.GetComponent<PopupController>();

        popupController.ChangeNameText(dialogueMessagesArray[2]);
        popupController.ChangeDialogueText(dialogueMessagesArray[3]);
    }
    IEnumerator DestroyPopupAfterTime(float delay, GameObject popup)
    {
        yield return new WaitForSeconds(delay);
        Destroy(popup);
        GoToNextNodeThroughOutputName("nextNode");
    }
    //Create text popup


    //Create buttons 
    private void UpdateDialogueBox()
    {
        OpenDialogueWindow();

        dialogueBoxController.UpdateName(dialogueMessagesArray[1]);
        dialogueBoxController.UpdateText(dialogueMessagesArray[2]);

        CreateNewDialogueChoiceButtons();
    }
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
                List<NodePort> allOutputList = dialogueNode.GetConnectedOutputs(dialogueGraph.currentNode);
                if (allOutputList.Count == 0)
                {
                    CreateEndConversationButton(dialogueMessagesArray[3]);
                }
                else
                {
                    CreateEndConversationButton("[No choices here yet]");
                }
            }
        }
        else
        {
            Debug.LogError("A node should never have a null amount of outputs");
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
        dialogueButtonGO.GetComponent<RectTransform>().localPosition = ReturnSmallButtonPosition(buttonIndex);

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
        Vector3 specialPositionVector = ReturnBigButtonPosition(buttonIndex);
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
    private Vector3 ReturnSmallButtonPosition(int index)
    {
        if (index > 5)
        {
            Debug.Log("Can only make max 6 small buttons");
            return new Vector3(firstButtonPosition.x, firstButtonPosition.y, 0);
        }
        else
        {
            Vector3 returnVector = new Vector3(firstButtonPosition.x + xOffset * (index % 2), firstButtonPosition.y + yOffset * (index / 2), 0);
            return returnVector;
        }
    }
    private Vector3 ReturnBigButtonPosition(int index)
    {
        if (index > 3)
        {
            Debug.Log("Can only make max 3 big buttons");
            return new Vector3(firstButtonPosition.x, firstButtonPosition.y, 0);
        }
        else
        {
            Vector3 returnVector = new Vector3(firstButtonPosition.x + (xOffset * 0.5f), firstButtonPosition.y + yOffset * index, 0);
            return returnVector;
        }
    }
    public void OnDialogueChoiceButtonClicked(GameObject obj)
    {
        EventManager.TriggerEvent("Next Dalogue Sentence", obj);
    }
    //Create buttons 


    // Go to next node 
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
        foreach (NodePort nodePort in dialogueGraph.currentNode.Outputs)
        {
            bool isCurrentNodeConnected = nodePort.IsConnected == true;
            if (isCurrentNodeConnected)
            {
                return false;
            }
        }
        return true;
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
            Debug.Log("Next Node wasn't found - name: " + outputName + " doesn't exist");
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
    private void OpenDialogueWindow()
    {
        if (textWindowAnimator.GetBool("isClosed"))
        {
            textWindowAnimator.SetBool("isClosed", false);
        }
    }
    private void CloseDialogueWindow()
    {
        if (!textWindowAnimator.GetBool("isClosed"))
        {
            textWindowAnimator.SetBool("isClosed", true);
        }
    }
    // Go to next node 


    //Action node
    private void TriggerNodeActions()
    {
        SaveNodeTriggerValues();

        int actionNumber = dialogueMessagesArray.Length;
        int valueNumber = dialogueValuesArray.Length;
        for (int i = 1; i < actionNumber; i++)
        {
            string actionName = dialogueMessagesArray[i];

            bool checkValueListLength = valueNumber > i;
            if (checkValueListLength)
            {
                string triggerValue = dialogueValuesArray[i];
                bool triggerValueNotNull = triggerValue != "-1";
                if (triggerValueNotNull)
                {
                    TriggerActionByNameAndValue(actionName, triggerValue);
                    continue;
                }
            }
            TriggerActionByName(actionName);

        }
    }
    private void SaveNodeTriggerValues()
    {
        BaseNode baseNode = dialogueGraph.currentNode;
        string data = baseNode.GetValues();
        dialogueValuesArray = data.Split('/');
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

    
    private void TriggerActionByNameAndValue(string actionName, string value)
    {
        EventManager.TriggerEvent(actionName, value);
        Debug.Log("Triggered event:" + actionName);
        if (calledEventList.Contains(actionName) == false)
        {
            calledEventList.Add(actionName);
        }
    }
    //Action node


    //Read node helper functions 
    public void EndDialogue()
    {
        textWindowAnimator.SetBool("isClosed", true);
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
    //Read node helper functions 

    
    //Start node
    private void HandleStartNode()
    {
        BaseNode baseNode = dialogueGraph.currentNode;
        StartNode dialogueNode = baseNode as StartNode;
        List<NodePort> outputList = dialogueNode.GetEnabledConnectedOutputs(baseNode);
        //Debug.Log("Output count: " + outputList.Count);
        GoToRandomNextNode(outputList);
    }
    //Start node


    //Get enabled outputs 
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
    //Get enabled outputs 


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
