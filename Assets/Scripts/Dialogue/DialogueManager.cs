using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    //Public variables
    public Text nameText;
    public Text dialogueText;
    public Text buttonText;
    public GameObject dialogueBoxGO;

    public Animator textAnimator;

    [HideInInspector]
    public bool isDisplayingMessage = false;
    //Private variables
    private Queue<string> sentences;
    private Coroutine textAnimationCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    private void OnEnable()
    {

    }
    private void OnDisable()
    {
    }



    public IEnumerator StartDialogue(Dialogue dialogueInput)
    {
        yield return new WaitForEndOfFrame();

        sentences.Clear();

        foreach (string sentence in dialogueInput.sentences)
        {
            sentences.Enqueue(sentence);
        }
        
        DisplayNextSentence();
    }
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            //EndDialogue();
            return;
        }
        if (sentences.Count == 1)
        {
            buttonText.text = "End conversation";
        }

        string nextSentenceToSay = sentences.Dequeue();
        
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
}
