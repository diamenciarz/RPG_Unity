using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushController : MonoBehaviour
{
    private void OnEnable()
    {
        StaticDataHolder.AddDashableObject(gameObject);
    }
    private void OnDisable()
    {
        StaticDataHolder.RemoveDashableObject(gameObject);
    }
    private void OnDestroy()
    {
        StaticDataHolder.RemoveDashableObject(gameObject);
    }
}
