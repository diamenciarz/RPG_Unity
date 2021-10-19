using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PopupController : MonoBehaviour
{

    private TextMeshProUGUI popupText;
    private LayoutElement myLayoutElement;
    private Vector2 textFieldSize;
    Coroutine animateTextCoroutine;
    private bool isAnimating;

    private void Awake()
    {
        popupText = GetComponentInChildren<TextMeshProUGUI>();
        myLayoutElement = GetComponentInChildren<LayoutElement>();
        textFieldSize = myLayoutElement.gameObject.GetComponent<RectTransform>().sizeDelta;
    }
    public void ChangeDisplayText(string newText)
    {
        animateTextCoroutine = StartCoroutine(AnimateSentence(newText));
    }

    IEnumerator AnimateSentence(string inputSentence)
    {
        isAnimating = true;
        popupText.text = "";

        foreach (char letter in inputSentence.ToCharArray())
        {
            popupText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }

        isAnimating = false;
        animateTextCoroutine = null;
    }

    public bool GetIsAnimating()
    {
        return isAnimating;
    }
    public void ResetPopupSize()
    {
        myLayoutElement.minWidth = 600;
        myLayoutElement.minHeight = 320;
        textFieldSize = new Vector2(600, 320);
    }
    /// <summary>
    /// Base size is height 320, width 600. Don't go for height less than 320 or width less than 200
    /// </summary>
    /// <param name="height"></param>
    /// <param name="width"></param>
    public void SetPopupSize(float height, float width)
    {
        myLayoutElement.minWidth = width;
        myLayoutElement.minHeight = height;
        textFieldSize = new Vector2(width, height);
    }
    public void SetPopupPosition(float xPosition, float yPosition)
    {
        transform.position = new Vector3(xPosition, yPosition, 0);
    }

}
