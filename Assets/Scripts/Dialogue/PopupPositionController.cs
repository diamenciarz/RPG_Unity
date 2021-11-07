using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPositionController : MonoBehaviour
{
    public GameObject playerGameObject;
    // Update is called once per frame
    void LateUpdate()
    {
        if (playerGameObject)
        {
            transform.position = playerGameObject.transform.position;
        }
    }
}
