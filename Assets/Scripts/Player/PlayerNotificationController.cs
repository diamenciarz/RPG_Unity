using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerNotificationController : MonoBehaviour
{
    public Text popupTextField;

    //private UnityAction onDialogue;
    private bool isPopupShown = false;
    private NodeReader nodeReaderSingleton;
    //[HideInInspector]
    public List<DialogueTrigger> dialogueTriggerList;
    private DialogueTrigger activeDialogueTrigger;

    Coroutine popupWaitCoroutine;

    private void Awake()
    {
        nodeReaderSingleton = FindObjectOfType<NodeReader>();
        //onDialogue = new UnityAction(EnteredDialogueTrigger);
    }

    private void OnEnable()
    {
        EventManager.StartListening("ShowDialogue", EnteredDialogueTrigger);
        EventManager.StartListening("HideDialogue", LeftDialogueTrigger);
        EventManager.StartListening("StartPopupCoroutine", StartPopupCoroutine);
    }
    private void OnDisable()
    {
        EventManager.StopListening("ShowDialogue", EnteredDialogueTrigger);
        EventManager.StopListening("HideDialogue", LeftDialogueTrigger);
        EventManager.StopListening("StartPopupCoroutine", StartPopupCoroutine);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPopupShown)
        {
            if (dialogueTriggerList.Count != 0)
            {
                activeDialogueTrigger = dialogueTriggerList[dialogueTriggerList.Count - 1];
                activeDialogueTrigger.TriggerDialogue();
                HidePopupTextField();

            }
        }
    }

    // List methods
    public void EnteredDialogueTrigger(object dialogueTriggerScript)
    {
        DialogueTrigger dialogueTrigger = (DialogueTrigger)dialogueTriggerScript;
        dialogueTriggerList.Add(dialogueTrigger);
        if (popupWaitCoroutine == null)
        {
            StartPopupCoroutine();
        }
    }
    IEnumerator PopupCoroutine()
    {
        yield return new WaitUntil(() => (!nodeReaderSingleton.isDisplayingMessage)&&(dialogueTriggerList.Count != 0));

        ShowPopupTextField();
        popupWaitCoroutine = null;
    }
    public void LeftDialogueTrigger(object dialogueTriggerScript)
    {
        DialogueTrigger dialogueTrigger = (DialogueTrigger)dialogueTriggerScript;
        if (dialogueTriggerList.Contains(dialogueTrigger))
        {
            dialogueTriggerList.Remove(dialogueTrigger);

            if (dialogueTrigger == activeDialogueTrigger)
            {
                EventManager.TriggerEvent("PlayerLeftDialogue");
            }
        }
        if (dialogueTriggerList.Count == 0)
        {
            HidePopupTextField();
        }
    }
    // "Press E" shower and hider
    private void HidePopupTextField()
    {
        popupTextField.enabled = false;
        isPopupShown = false;
        //Debug.Log("Hide Popup field");
    }
    private void ShowPopupTextField()
    {
        string popupText = "Press [E]";
        popupTextField.text = popupText;
        popupTextField.enabled = true;
        isPopupShown = true;
        //Debug.Log("Show Popup field");
    }
    private void StartPopupCoroutine()
    {
        popupWaitCoroutine = StartCoroutine(PopupCoroutine());
    }
}
