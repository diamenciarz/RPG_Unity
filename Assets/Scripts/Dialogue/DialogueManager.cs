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
    //Private variables
    private Queue<string> sentences;
    private Coroutine textAnimationCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public IEnumerator StartDialogue(Dialogue dialogueInput)
    {
        dialogueBoxGO.SetActive(true);
        textAnimator.SetBool("isClosed", false);

        nameText.text = dialogueInput.name;
        buttonText.text = "Continue...";

        sentences.Clear();

        foreach (string sentence in dialogueInput.sentences)
        {
            sentences.Enqueue(sentence);
        }
        dialogueText.text = "";

        yield return new WaitForSeconds(0.6f);
        DisplayNextSentence();
    }
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        if (sentences.Count == 1)
        {
            buttonText.text = "End conversation";
        }

        string nextSentenceToSay = sentences.Dequeue();
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
        }
        textAnimationCoroutine = StartCoroutine(animateSentence(nextSentenceToSay));
    }
    void EndDialogue()
    {
        textAnimator.SetBool("isClosed", true);
    }
    IEnumerator animateSentence(string inputSentence) 
    {
        dialogueText.text = "";
        foreach (char letter in inputSentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
