using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMissingIcon : MonoBehaviour
{
    public GameObject objectToFollow;

    float xMin;
    float xMax;
    float yMin;
    float yMax;

    private float positionOffset = 0.3f;
    [SerializeField] [Range(0, 1)] float minimumSpriteSize = 0.4f;
    [SerializeField]
    [Tooltip("If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear")]
    float scaleFactor = 6f;
    [SerializeField] float spriteScale = 1;
    [SerializeField] float screenEdgeOffset;
    [SerializeField] float deltaRotation = 0;
    [SerializeField] Color allyColor;
    [SerializeField] Color enemyColor;


    //Private variables
    Camera mainCamera;
    SpriteRenderer mySpriteRenderer;

    void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        xMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + screenEdgeOffset;
        xMax = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - screenEdgeOffset;
        yMin = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0)).y + screenEdgeOffset;
        yMax = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - screenEdgeOffset;
    }
    public void TryFollowThisObject(GameObject followThis)
    {
        if (followThis != null)
        {
            objectToFollow = followThis;
            UpdateSpriteColor(followThis);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateSpriteColor(GameObject objectToFollow)
    {
        DamageReceiver damageReceiver;
        damageReceiver = objectToFollow.GetComponent<DamageReceiver>();
        if (damageReceiver != null)
        {
            if (damageReceiver.GetTeam() == 1)
            {
                GetComponent<SpriteRenderer>().color = allyColor;
                return;
            }
            if (damageReceiver.GetTeam() == 2)
            {
                GetComponent<SpriteRenderer>().color = enemyColor;
                return;
            }
        }

        BasicProjectileController basicProjectileController;
        basicProjectileController = objectToFollow.GetComponent<BasicProjectileController>();
        if (basicProjectileController != null)
        {
            if (basicProjectileController.GetTeam() == 1)
            {
                GetComponent<SpriteRenderer>().color = allyColor;
                return;
            }
            if (basicProjectileController.GetTeam() == 2)
            {
                GetComponent<SpriteRenderer>().color = enemyColor;
                return;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (objectToFollow != null)
        {
            UpdateVisibility();
            if (IsVisible())
            {
                UpdateTransform();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void UpdateTransform()
    {
        UpdateRotation();
        UpdatePosition();
        UpdateScale();
    }
    private void UpdateRotation()
    {
        transform.rotation = objectToFollow.transform.rotation * Quaternion.Euler(0, 0, deltaRotation);
    }
    private void UpdatePosition()
    {
        float newXPosition = objectToFollow.transform.position.x;
        float newYPosition = objectToFollow.transform.position.y;

        newXPosition = Mathf.Clamp(newXPosition, xMin - positionOffset, xMax + positionOffset);
        newYPosition = Mathf.Clamp(newYPosition, yMin - positionOffset, yMax + positionOffset);
        transform.position = new Vector2(newXPosition, newYPosition);
    }
    private void UpdateScale()
    {
        float distanceToObject = Mathf.Abs((transform.position - objectToFollow.transform.position).magnitude);
        //Sets the new scale ilamped in between <0,4;1>
        float newScale = (1 - (Mathf.Clamp((distanceToObject / scaleFactor), 0, 1f) * (1 - minimumSpriteSize))) * spriteScale;
        Vector3 newScaleVector3 = new Vector3(newScale, newScale, 0);

        transform.localScale = newScaleVector3;
    }
    private void UpdateVisibility()
    {
        bool isFollowedObjectOutsideCameraView = objectToFollow.transform.position.x > xMax + 0.5f
                || objectToFollow.transform.position.x < xMin - 0.5f
                || objectToFollow.transform.position.y > yMax + 0.5f
                || objectToFollow.transform.position.y < yMin - 0.5f;
        if (isFollowedObjectOutsideCameraView)
        {
            //If the followed object is further from the screen edge, than scaleFactor (in map units), the icon will disappear
            float distanceToFollowedObject = (transform.position - objectToFollow.transform.position).magnitude;
            bool isObjectTooFar = (distanceToFollowedObject / scaleFactor > 1f);
            if (isObjectTooFar)
            {
                SetVisibility(false);
            }
            else
            {
                SetVisibility(true);
            }
        }
        else
        {
            SetVisibility(false);
        }
    }
    private void SetVisibility(bool isVisible)
    {
        if (!isVisible)
        {
            mySpriteRenderer.enabled = isVisible;
        }
    }
    private bool IsVisible()
    {
        return mySpriteRenderer.enabled;
    }
}