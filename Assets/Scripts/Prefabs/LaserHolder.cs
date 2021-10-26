using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserHolder : MonoBehaviour
{
    GameObject from;
    GameObject to;
    float lineDuration;
    float maxLength;
    bool wasCreated = false;
    [SerializeField] bool destroyOnTargetDestroyed;

    Vector3 lastToPosition;

    LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        if (from != null)
        {
            lineRenderer.SetPosition(0, from.transform.position);
        }
        else
        {
            Destroy(gameObject);
        }
        if (to != null)
        {
            lastToPosition = to.transform.position;
            lineRenderer.SetPosition(1, lastToPosition);
        }
        else
        {
            //Do nothing
            if (destroyOnTargetDestroyed)
            {
                Destroy(gameObject);
            }
        }
    }
    public void ReceiveCoords(GameObject newFrom, GameObject newTo, float newLineDuration, float maxLineLength)
    {
        maxLength = maxLineLength;
        from = newFrom;
        to = newTo;
        lastToPosition = to.transform.position;
        lineDuration = newLineDuration;
        //Nastaw czas zniszczenia
        Destroy(gameObject, lineDuration);

        wasCreated = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (wasCreated)
        {
            if (to != null)
            {
                lastToPosition = to.transform.position;
                lineRenderer.SetPosition(1, lastToPosition);
            }
            else
            {
                //Do nothing
                if (destroyOnTargetDestroyed)
                {
                    Destroy(gameObject);
                }
            }

            if (from != null)
            {
                if (Mathf.Abs((from.transform.position - lastToPosition).magnitude) > maxLength)
                {
                    Destroy(gameObject);
                }
                lineRenderer.SetPosition(0, from.transform.position);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}