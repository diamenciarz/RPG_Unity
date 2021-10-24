using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticDataHolder
{
    public static List<GameObject> dashableObjectList = new List<GameObject>();
    public static GameObject currentDashObject;
    public static List<GameObject> projectileList = new List<GameObject>();
    public static List<GameObject> entityList = new List<GameObject>();
    public static List<float> soundDurationList = new List<float>();
    public static int soundLimit = 10;


    //List Methods
    public static void AddDashableObject(GameObject addObject)
    {
        dashableObjectList.Add(addObject);
    }
    public static void RemoveDashableObject(GameObject addObject)
    {
        if (dashableObjectList.Contains(addObject))
        {
            dashableObjectList.Remove(addObject);
        }
    }
    public static List<GameObject> GetDashableObjectList()
    {
        return dashableObjectList;
    }


    // Dash Object Methods
    public static void SetCurrentDashObject(GameObject setObject)
    {
        currentDashObject = setObject;
    }
    public static GameObject GetCurrentDashObject()
    {
        return currentDashObject;
    }


    //Projectile list methods
    public static void AddProjectile(GameObject projectile)
    {
        projectileList.Add(projectile);
    }
    public static void RemoveProjectile(GameObject projectile)
    {
        projectileList.Add(projectile);
    }


    //Entity list methods
    public static void AddEntity(GameObject entity)
    {
        entityList.Add(entity);
    }
    public static void RemoveEntity(GameObject entity)
    {
        entityList.Add(entity);
    }


    //Sound list methods
    public static void AddSoundDuration(float duration)
    {
        soundDurationList.Add(Time.time + duration);
    }
    public static int GetSoundCount()
    {
        RemoveOldSoundsFromList();
        return soundDurationList.Count;
    }
    private static void RemoveOldSoundsFromList()
    {
        for (int i = soundDurationList.Count - 1; i >= 0; i--)
        {
            if (soundDurationList[i] < Time.time)
            {
                soundDurationList.RemoveAt(i);
            }
        }
    }
    public static int GetSoundLimit()
    {
        return soundLimit;
    }


    //Helper functions
    public static float GetDistanceBetweenObjectsIn2D(GameObject firstObject, GameObject secondObject)
    {
        Vector3 playerPosition = firstObject.transform.position;
        playerPosition.z = 0;
        Vector3 objectPosition = secondObject.transform.position;
        objectPosition.z = 0;

        return (playerPosition - objectPosition).magnitude;
    }
    public static Vector3 GetDeltaPositionFromToIn2D(GameObject firstObject, GameObject secondObject)
    {
        Vector3 myPositionVector = firstObject.transform.position;
        myPositionVector.z = 0;
        Vector3 targetPosition = secondObject.transform.position;
        targetPosition.z = 0;

        return (targetPosition - myPositionVector);
    }
    public static void RotateFromToGameObjectIn2D(GameObject firstObject, GameObject secondObject)
    {
        Vector3 myPositionVector = firstObject.transform.position;
        myPositionVector.z = 0;
        Vector3 targetPosition = secondObject.transform.position;
        targetPosition.z = 0;

        Vector3 deltaPosition = targetPosition - myPositionVector;

        float zRotation = Mathf.Rad2Deg * Mathf.Atan(deltaPosition.y / deltaPosition.x);

        firstObject.transform.rotation = Quaternion.Euler(0, 0, zRotation);
    }
    public static Vector3 GetMoveVectorInDirection(float speed, float zDirectionInDegrees)
    {
        Vector3 returnVector = speed * GetNormalizedVectorInDirection(zDirectionInDegrees);
        return returnVector;

    }
    public static Vector3 GetNormalizedVectorInDirection(float zDirectionInDegrees)
    {
        float xStepMove = -Mathf.Sin(Mathf.Deg2Rad * zDirectionInDegrees);
        float yStepMove = Mathf.Cos(Mathf.Deg2Rad * zDirectionInDegrees);
        Vector3 returnVector = new Vector3(xStepMove, yStepMove, 0);
        return returnVector;
    }
}
