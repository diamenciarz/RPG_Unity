using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionBarController : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] Image healthBarImage;
    [SerializeField] GameObject objectToFollow;
    [Header("Display Settings")]
    [SerializeField] Vector3 deltaPositionToObject;
    [SerializeField] bool useGradient = true;
    [SerializeField] Gradient barColorGradient;
    [SerializeField] [Range(0, 1)] float originalAlfa = 1f;
    [SerializeField] protected float hideOverTime = 0.5f;

    [Header("Transform Settings")]
    [SerializeField] bool destroyWithoutParent = true;
    [SerializeField] bool rotateSameAsParent;
    private Quaternion deltaRotationFromParent;
    private bool isDestroyed;
    Color currentColor;
    private bool isShown = true;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 1, 0);
        //originalAlfa = healthBarImage.color.a;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    public void SetObjectToFollow(GameObject followGO)
    {
        objectToFollow = followGO;
    }
    public void SetRotateSameAsParent(bool input)
    {
        rotateSameAsParent = input;
    }
    public void SetDeltaPositionToObject(Vector3 newDeltaPosition)
    {
        deltaPositionToObject = newDeltaPosition;
    }
    public void SetDeltaRotationToObject(Quaternion newDeltaRotation)
    {
        deltaRotationFromParent = newDeltaRotation;
    }

    // Update is called once per frame
    void Update()
    {
        FollowParent();
        AdjustBarVisibility();
    }

    private void FollowParent()
    {
        if (objectToFollow != null)
        {
            transform.position = objectToFollow.transform.position + deltaPositionToObject;
            if (rotateSameAsParent)
            {
                transform.rotation = objectToFollow.transform.rotation * deltaRotationFromParent;
            }
        }
        else
        {
            if (destroyWithoutParent)
            {
                isDestroyed = true;
                Destroy(gameObject);
            }
        }
    }

    public void UpdateProgressionBar(float newHP, float maxHP)
    {
        if (!isDestroyed)
        {

            if (newHP < 0)
            {
                newHP = 0;
            }
            if (newHP > maxHP)
            {
                newHP = maxHP;
            }
            float newRatio = newHP / maxHP;
            newRatio = Mathf.Clamp(newRatio, 0, 1);
            if (!double.IsNaN(newRatio))
            {
                healthBarImage.fillAmount = newRatio;
                if (useGradient)
                {
                    Color newColor = barColorGradient.Evaluate(newRatio);
                    newColor.a = originalAlfa;
                    healthBarImage.color = newColor;

                    currentColor = healthBarImage.color;
                }
            }
            else
            {
                Debug.Log("Bar ratio: " + newRatio + " object to follow:  " + objectToFollow);
                Debug.LogError("NaN");

            }
        }
    }
    public void IsVisible(bool isTrue)
    {
        isShown = isTrue;
    }
    private void AdjustBarVisibility()
    {
        if (isShown)
        {
            ChangeAlfaTowards(originalAlfa);
        }
        else
        {
            ChangeAlfaTowards(0);
        }
    }
    private void ChangeAlfaTowards(float alfa)
    {
        float colorAlfa = healthBarImage.color.a;
        if (colorAlfa != alfa)
        {
            float changeThisFrame = originalAlfa * Time.deltaTime / hideOverTime;
            Debug.Log("Change this frame: "+ changeThisFrame);
            colorAlfa = Mathf.MoveTowards(colorAlfa, alfa, changeThisFrame);

            Color newColor = currentColor;
            newColor.a = colorAlfa;
            healthBarImage.color = newColor;
        }
    }
}