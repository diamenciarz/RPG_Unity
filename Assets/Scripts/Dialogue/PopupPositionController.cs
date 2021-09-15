using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupPositionController : MonoBehaviour
{
    public GameObject playerGameObject;
    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = playerGameObject.transform.position;
    }
}
