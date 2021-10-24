using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionBarController : MonoBehaviour
{
    [SerializeField] Image healthBarImage;
    [SerializeField] GameObject objectToFollow;
    [SerializeField] Vector3 deltaPositionToObject;
    [SerializeField] bool useGradient = true;
    [SerializeField] Gradient barColorGradient;

    [SerializeField] [Range(0, 1)] float originalAlfa = 1f;
    [SerializeField] bool destroyWithoutParent = true;
    [SerializeField] bool rotateSameAsParent;
    private Quaternion deltaRotationFromParent;
    private bool isDestroyed;
    Color currentColor;
    private bool isShown = true;
    private float hideOverTime = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 1, 0);
        //originalAlfa = healthBarImage.color.a;
    }
    public void SetObjectToFollow(GameObject followGO)
    {
        objectToFollow = followGO;
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
    public void ShowBar(bool isTrue)
    {
        isShown = isTrue;
    }
    private void AdjustBarVisibility()
    {
        if (isShown)
        {
            float colorAlfa = healthBarImage.color.a;
            if (colorAlfa != originalAlfa)
            {
                float changeThisFrame = originalAlfa * Time.deltaTime / hideOverTime;
                colorAlfa = Mathf.MoveTowards(colorAlfa, originalAlfa, changeThisFrame);

                Color newColor = currentColor;
                newColor.a = colorAlfa;
                healthBarImage.color = newColor;
            }
        }
        else
        {
            float colorAlfa = healthBarImage.color.a;
            if (colorAlfa != 0)
            {
                float changeThisFrame = originalAlfa * Time.deltaTime / hideOverTime;
                colorAlfa = Mathf.MoveTowards(colorAlfa, 0, changeThisFrame);

                Color newColor = currentColor;
                newColor.a = colorAlfa;
                healthBarImage.color = newColor;
            }
        }
    }
}