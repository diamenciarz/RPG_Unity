using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public DialogueGraph dialogueGraph;
    private Collider2D myCollider2D;
    public Transform popupTransform;

    private void Start()
    {
        myCollider2D = GetComponent<Collider2D>();
    }

    public void TriggerDialogue()
    {
        NodeReader foundClass = FindObjectOfType<NodeReader>();
        foundClass.StartDialogue(dialogueGraph, popupTransform);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            EventManager.TriggerEvent("ShowDialogue", this);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            EventManager.TriggerEvent("HideDialogue", this);
        }
    }
}
