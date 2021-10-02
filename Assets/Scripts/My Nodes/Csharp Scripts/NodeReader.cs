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
    private Coroutine readNodeCoroutine;
    private bool nextSentence;
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
    public void NextSentence()
    {
        nextSentence = true;
    }
    //This is called by a dialogue trigger on an NPC
    public IEnumerator StartDialogue(DialogueGraph inputDialogueGraph)
    {
        float waitTimeToEliminateLag = 1;
        yield return new WaitForSeconds(waitTimeToEliminateLag);
        //Set text to default
        nameText.text = dialogueGraph.name;
        dialogueText.text = "";
        //Create new dialogue choice buttons
        CreateDialogueChoiceButtons();
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
            CreateDialogueChoiceButtons();
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
    private void CreateDialogueChoiceButtons()
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
        GameObject dialogueButtonGO = Instantiate(dialogueChoiceButtonPrefab, ReturnButtonPosition(buttonIndex), Quaternion.identity, transform);
        dialogueChoiceButtonList.Add(dialogueButtonGO);
        DialogueButton dialogueButton = new DialogueButton(FindNextNodeUsingOutputPortName(portName));
        dialogueButtonDictionary.Add(dialogueButtonGO, dialogueButton);


        dialogueButtonGO.GetComponent<Text>().text = message;

        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnDialogueChoiceButtonClicked(gameObject); }); 
    }
    private void DeletePreviousButtons()
    {
        foreach (GameObject button in dialogueChoiceButtonList)
        {
            Destroy(button);
        }
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
            SetNextNodeUsingOutputPortName(outputName);

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
    private BaseNode FindNextNodeUsingOutputPortName(string outputName)
    {

        foreach (NodePort nodePort in dialogueGraph.currentNode.Outputs)
        {
            if (nodePort.fieldName == outputName)
            {
                //Set a new graph as current
                return nodePort.Connection.node as BaseNode;
            }
        }
        return null;
    }
    private void SetNextNodeUsingOutputPortName(string outputName)
    {
        BaseNode returnedNode = FindNextNodeUsingOutputPortName(outputName);
        if (returnedNode != null)
        {
            dialogueGraph.currentNode = returnedNode;
        }
        else
        {
            Debug.Log("Next Node wasn't found - name doesn't exist");
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
