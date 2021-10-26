using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrawer : MonoBehaviour
{
    [SerializeField] GameObject lineHolderPrefab;

    public void DrawLine(GameObject from, GameObject to, float lineDuration, float maxLineLength)
    {
        GameObject newLineHolder = Instantiate(lineHolderPrefab, from.transform.position, transform.rotation);

        newLineHolder.GetComponent<LaserHolder>().ReceiveCoords(from, to, lineDuration, maxLineLength);
    }
}