using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    #region Trace following
    private void FollowTrace()
    {
        if (startedHunting == false)
        {
            FollowFirstTrace();
            return;
        }
        CheckFresherTraces();
        CheckNextTrace();
    }
    private void FollowFirstTrace()
    {
        if (lastTargetPositions.Count > 0)
        {
            startedHunting = true;
            lastTargetPosition.transform.position = lastTargetPositions[0];
        }
    }
    private void CheckNextTrace()
    {
        Vector3 deltaPositionToTrace = transform.position - lastTargetPositions[0];
        float stopDistance = aiPath.endReachedDistance * 2;
        bool isClose = deltaPositionToTrace.magnitude < stopDistance; //potentially switch to can see last target position
        if (isClose)
        {
            if (lastTargetPositions.Count > 1)
            {
                lastTargetPositions.RemoveAt(0);
                lastTargetPosition.transform.position = lastTargetPositions[0];
            }
        }
    }
    private void CheckFresherTraces()
    {
        if (lastTargetPositions.Count > 1)
        {
            int traceIndex = GetTheFreshestTraceInSight();
            bool fresherTraceFound = traceIndex >= 0;
            if (fresherTraceFound)
            {
                Vector3 newPosition = lastTargetPositions[traceIndex];
                DeleteOldTraces(traceIndex);
                lastTargetPosition.transform.position = newPosition;
            }
        }
    }
    private int GetTheFreshestTraceInSight()
    {
        int highestIndex = -1;
        for (int i = 0; i < lastTargetPositions.Count; i++)
        {
            Vector3 pos = lastTargetPositions[i];
            if (HelperMethods.CanSeeDirectly(transform.position, pos))
            {
                highestIndex = i;
            }
        }
        return highestIndex;
    }
    private void DeleteOldTraces(int upToIndex)
    {
        if (upToIndex >= 0)
        {
            for (int i = 0; i < upToIndex; i++)
            {
                lastTargetPositions.RemoveAt(i);
            }
        }
    }
    #endregion
}
