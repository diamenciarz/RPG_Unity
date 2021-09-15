using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private Collider2D myCollider2D;

    private void Start()
    {
        myCollider2D = GetComponent<Collider2D>();
            Debug.Log("Player entered the trigger area");
    }

    public void TriggerDialogue()
    {
        StartCoroutine(FindObjectOfType<DialogueManager>().StartDialogue(dialogue));
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            EventManager.TriggerEvent("ShowDialogue",this);
            Debug.Log("Player entered the trigger area");

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            EventManager.TriggerEvent("HideDialogue", this);
            Debug.Log("Player left the trigger area");

        }
    }
}
