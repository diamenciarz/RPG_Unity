using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class DialogueBoxController : MonoBehaviour
{

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    //Private variables
    private Coroutine textAnimationCoroutine;


    //Display text functions 
    public void UpdateName(string newName)
    {
        nameText.text = newName;
    }
    public void UpdateText(string nextSentenceToSay)
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
    //Display text functions 
}
