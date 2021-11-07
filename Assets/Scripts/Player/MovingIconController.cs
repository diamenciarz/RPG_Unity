using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingIconController : MonoBehaviour
{
    [SerializeField] GameObject objectToFollow;
    private GameObject currentClosestGameObject;
    SpriteRenderer mySpriteRenderer;

    private float snapRange = 1;
    private bool isVisible = true;
    private bool isAbilityAvailable = true;
    // Start is called before the first frame update
    private void OnEnable()
    {
        EventManager.StartListening("SetPlayerGameObject", SetGameObjectToFollow);
        EventManager.StartListening("UpdateDashIcon", SetIsAbilityAvailable);
        EventManager.StartListening("UpdateDashSnapRange", SetDashSnapRange);
    }
    private void OnDisable()
    {
        EventManager.StopListening("SetPlayerGameObject", SetGameObjectToFollow);
        EventManager.StopListening("UpdateDashIcon", SetIsAbilityAvailable);
        EventManager.StopListening("UpdateDashSnapRange", SetDashSnapRange);
    }
    private void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isAbilityAvailable)
        {
            UpdatePosition();
            UpdateRotation();
            UpdateVisibility();
        }
        else
        {
            SetSpriteVisibility(false);
        }
    }
    private void UpdateVisibility()
    {
        if (isVisible && isAbilityAvailable)
        {
            SetSpriteVisibility(true);
        }
        else
        {
            SetSpriteVisibility(false);
        }
    }
    private void SetSpriteVisibility(bool isTrue)
    {
        mySpriteRenderer.enabled = isTrue;
    }
    private void UpdateRotation()
    {
        if (objectToFollow)
        {
            Vector3 myPosition = gameObject.transform.position;
            Vector3 otherPosition = objectToFollow.transform.position;
            gameObject.transform.rotation = StaticDataHolder.GetRotationFromToIn2D(myPosition, otherPosition);
        }
    }
    private void UpdatePosition()
    {
        currentClosestGameObject = StaticDataHolder.GetTheClosestDashableObject(objectToFollow.transform.position, snapRange);

        if (currentClosestGameObject != null)
        {
            isVisible = true;
            GoToGameObject(currentClosestGameObject);
        }
        else
        {
            isVisible = false;
        }
    }
    private void GoToGameObject(GameObject goTo)
    {
        Vector3 objectPosition = goTo.transform.position;
        objectPosition.z = 0;
        transform.position = objectPosition;
    }
    public void SetGameObjectToFollow(object inputObject)
    {
        try
        {
            objectToFollow = (GameObject)inputObject;
        }
        catch (System.Exception)
        {
            Debug.LogError("Object to follow was not an object");
            throw;
        }
    }
    private void SetDashSnapRange(object newValue)
    {
        snapRange = (float)newValue;
    }
    public bool GetIsVisible()
    {
        return isVisible;
    }
    public void SetIsAbilityAvailable(object inputBool)
    {
        bool isTrue = (bool)inputBool;
        isAbilityAvailable = isTrue;
    }
}
