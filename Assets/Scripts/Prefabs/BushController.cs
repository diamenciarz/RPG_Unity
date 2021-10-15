using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BushController : MonoBehaviour
{
    private void Awake()
    {
        StaticDataHolder.AddDashableObject(gameObject);
    }
}
