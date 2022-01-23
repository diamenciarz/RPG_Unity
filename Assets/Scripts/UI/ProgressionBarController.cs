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
    [Tooltip("Time, after which the bar will disappear, after being shown (-1 for never)")]
    [SerializeField] [Range(0, 100)] private float hideDelay = 0;

    [Header("Transform Settings")]
    [SerializeField] bool destroyWithoutParent = true;
    [SerializeField] bool rotateSameAsParent;

    private Quaternion deltaRotationFromParent;
    private bool isDestroyed;
    Color currentColor;
    private bool isShown = true;
    private double lastUsedTime;

    void Start()
    {
        transform.rotation = Quaternion.Euler(0, 1, 0);
        //originalAlfa = healthBarImage.color.a;
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    #region Mutator methods
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
    public void IsVisible(bool isTrue)
    {
        isShown = isTrue;
        UpdateLastUsedTime();
    }

    #region Update Bar
    public void UpdateProgressionBar(float newHP, float maxHP)
    {
        if (!isDestroyed)
        {
            if (maxHP != 0)
            {
                float newRatio = newHP / maxHP;
                newRatio = Mathf.Clamp(newRatio, 0, 1);

                UpdateBarRatio(newRatio);
            }
            else
            {
                Debug.LogError("MaxHP was 0 and the ratio was NaN! Followed object: " + objectToFollow);
            }
        }
    }
    private void UpdateBarRatio(float ratio)
    {
        UpdateLastUsedTime();
        healthBarImage.fillAmount = ratio;

        UpdateGradientColor(ratio);
    }
    private void UpdateLastUsedTime()
    {
        lastUsedTime = Time.time;
    }
    private void UpdateGradientColor(float ratio)
    {
        if (useGradient)
        {
            Color newColor = barColorGradient.Evaluate(ratio);
            newColor.a = originalAlfa;
            healthBarImage.color = newColor;

            currentColor = healthBarImage.color;
        }
    }
    #endregion

    #endregion

    #region Update
    void Update()
    {
        CheckForParent();

        CheckHideDelay();
        ChangeBarVisibility();
    }

    #region Transform
    private void CheckForParent()
    {
        if (objectToFollow != null)
        {
            FollowParent();
            RotateSameAsParent();
        }
        else
        {
            HandleDestroy();
        }
    }
    private void FollowParent()
    {
        Vector3 parentPosition = objectToFollow.transform.position;
        transform.position = parentPosition + deltaPositionToObject;
    }
    private void RotateSameAsParent()
    {
        if (rotateSameAsParent)
        {
            Quaternion parentRotation = objectToFollow.transform.rotation;
            transform.rotation = parentRotation * deltaRotationFromParent;
        }
    }
    private void HandleDestroy()
    {
        if (destroyWithoutParent)
        {
            isDestroyed = true;
            Destroy(gameObject);
        }
    }
    #endregion

    #region Change visibility
    private void ChangeBarVisibility()
    {
        if (isShown)
        {
            MoveAlfaTowards(originalAlfa);
        }
        else
        {
            MoveAlfaTowards(0);
        }
    }
    private void MoveAlfaTowards(float targetAlfa)
    {
        float colorAlfa = healthBarImage.color.a;
        if (colorAlfa != targetAlfa)
        {
            Color newColor = currentColor;
            newColor.a = CountNewAlfa(targetAlfa);
            SetColor(newColor);
        }
    }
    private float CountNewAlfa(float targetAlfa)
    {
        //In how much time it could change from 0 to max value
        float changeRate = originalAlfa / hideOverTime;
        float changeThisFrame = changeRate * Time.deltaTime;
        float newColorAlfa = Mathf.MoveTowards(healthBarImage.color.a, targetAlfa, changeThisFrame);
        return newColorAlfa;
    }
    private void SetColor(Color newColor)
    {
        healthBarImage.color = newColor;
    }
    #endregion
    
    private void CheckHideDelay()
    {
        bool shouldHide = hideDelay > 0;
        if (shouldHide)
        {
            if (isShown)
            {
                bool pastHideCooldown = Time.time > lastUsedTime + hideDelay;
                if (pastHideCooldown)
                {
                    isShown = false;
                }
            }
        }
    }
    #endregion
}