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
    private DialogueTrigger dialogueCaller;

    private void Awake()
    {
        //onDialogue = new UnityAction(onDialogue);
    }

    private void OnEnable()
    {
        EventManager.StartListening("ShowDialogue", ShowPopupTextField);
        EventManager.StartListening("HideDialogue", HidePopupTextField);
    }
    private void OnDisable()
    {
        EventManager.StopListening("ShowDialogue", ShowPopupTextField);
        EventManager.StopListening("HideDialogue", HidePopupTextField);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && isPopupShown)
        {
            dialogueCaller.TriggerDialogue();
            HidePopupTextField();
        }
    }

    // Other methods
    public void ShowPopupTextField(object dialogueTriggerScriptReference)
    {
        dialogueCaller = (DialogueTrigger)dialogueTriggerScriptReference;

        string popupText = "Press [E]";
        popupTextField.text = popupText;
        popupTextField.enabled = true;
        isPopupShown = true;
        Debug.Log("Show Popup field");
    }
    public void HidePopupTextField()
    {
        popupTextField.enabled = false;
        isPopupShown = false;
        dialogueCaller = null;
        Debug.Log("Hide Popup field");
    }
    private void OnDialogue(object dialogueTriggerScriptReference)
    {
        dialogueCaller = (DialogueTrigger)dialogueTriggerScriptReference;
        
    }
}
