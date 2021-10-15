using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashIconController : MonoBehaviour
{

    [SerializeField] float snapRange = 5f;

    bool isVisible = true;
    public GameObject playerToFollow;
    public GameObject currentClosestGameObject;
    SpriteRenderer mySpriteRenderer;
    // Start is called before the first frame update
    private void OnEnable()
    {
        EventManager.StartListening("SetPlayerGameObject", SetGameObjectToFollow);
    }
    private void OnDisable()
    {
        EventManager.StopListening("SetPlayerGameObject", SetGameObjectToFollow);
    }
    private void Start()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePosition();
        UpdateRotation();
        UpdateVisibility();
    }
    private void UpdateVisibility()
    {
        if (isVisible)
        {
            mySpriteRenderer.enabled = true;
        }
        else
        {
            mySpriteRenderer.enabled = false;
        }
    }
    private void UpdateRotation()
    {
        StaticDataHolder.RotateFromToGameObjectIn2D(gameObject, playerToFollow);
    }
    private void UpdatePosition()
    {
        currentClosestGameObject = FindTheClosestDashableObject();
        if (currentClosestGameObject != null)
        {
            isVisible = true;
            GoToTheClosestDashableGameObject(currentClosestGameObject);
        }
        else
        {
            isVisible = false;
        }
    }
    private void GoToTheClosestDashableGameObject(GameObject goTo)
    {
        Vector3 objectPosition = goTo.transform.position;
        objectPosition.z = 0;
        transform.position = objectPosition;
    }
    private GameObject FindTheClosestDashableObject()
    {
        GameObject returnObject = null;
        float currentShortestDistance = 1f;

        foreach (GameObject dashableObject in StaticDataHolder.GetDashableObjectList())
        {
            float playerDistanceToGameObject = StaticDataHolder.GetPositionBetweenObjectsIn2D(dashableObject, playerToFollow);
            //Debug.Log("Distance:" + playerDistanceToGameObject);

            if (playerDistanceToGameObject <= snapRange)
            {
                if (returnObject == null)
                {
                    currentShortestDistance = playerDistanceToGameObject;
                    returnObject = dashableObject;
                    continue;
                }
                if (playerDistanceToGameObject < currentShortestDistance)
                {
                    //Debug.Log("Found: " + dashableObject);
                    currentShortestDistance = playerDistanceToGameObject;
                    returnObject = dashableObject;
                }
            }
        }
        return returnObject;
    }
    public void SetGameObjectToFollow(object inputObject)
    {
        try
        {
            playerToFollow = (GameObject)inputObject;
        }
        catch (System.Exception)
        {
            Debug.LogError("Object to follow was not an object");
            throw;
        }
    }
    public bool GetIsVisible()
    {
        return isVisible;
    }
}
