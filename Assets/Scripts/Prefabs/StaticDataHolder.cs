using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticDataHolder
{
    public static List<GameObject> dashableObjectList = new List<GameObject>();
    public static GameObject currentDashableObject;

    public static List<GameObject> obstacleList = new List<GameObject>();
    public static List<GameObject> projectileList = new List<GameObject>();
    public static List<GameObject> playerProjectileList = new List<GameObject>();
    public static List<GameObject> entityList = new List<GameObject>();
    public static List<GameObject> objectsCollidingWithPlayerList = new List<GameObject>();

    public static List<float> soundDurationList = new List<float>();
    public static int soundLimit = 10;

    //Obstacle ListMethods
    public static void AddObstacle(GameObject addObject)
    {
        obstacleList.Add(addObject);
    }
    public static void RemoveObstacle(GameObject removeObject)
    {
        if (obstacleList.Contains(removeObject))
        {
            obstacleList.Remove(removeObject);
        }
    }

    //Dashable Object List Methods
    public static void AddDashableObject(GameObject addObject)
    {
        dashableObjectList.Add(addObject);
    }
    public static void RemoveDashableObject(GameObject removeObject)
    {
        if (dashableObjectList.Contains(removeObject))
        {
            dashableObjectList.Remove(removeObject);
        }
    }
    public static List<GameObject> GetDashableObjectList()
    {
        return dashableObjectList;
    }


    // Dashable Object Methods
    public static void SetCurrentDashObject(GameObject setObject)
    {
        currentDashableObject = setObject;
    }
    public static GameObject GetCurrentDashObject()
    {
        return currentDashableObject;
    }


    //Projectile list methods
    public static void AddProjectile(GameObject projectile)
    {
        projectileList.Add(projectile);
    }
    public static void RemoveProjectile(GameObject projectile)
    {
        projectileList.Remove(projectile);
    }


    //Player projectile list methods
    public static void AddPlayerProjectile(GameObject projectile)
    {
        playerProjectileList.Add(projectile);
    }
    public static void RemovePlayerProjectile(GameObject projectile)
    {
        playerProjectileList.Remove(projectile);
    }


    //Entity list methods
    public static void AddEntity(GameObject entity)
    {
        entityList.Add(entity);
    }
    public static void RemoveEntity(GameObject entity)
    {
        entityList.Remove(entity);
    }


    //Objects colliding with player list methods
    public static void AddCollidingObject(GameObject collidingObject)
    {
        objectsCollidingWithPlayerList.Add(collidingObject);
    }
    public static void RemoveCollidingObject(GameObject collidingObject)
    {
        if (objectsCollidingWithPlayerList.Contains(collidingObject))
        {
            objectsCollidingWithPlayerList.Remove(collidingObject);
        }
    }
    public static List<GameObject> GetCollidingObjectList()
    {
        return objectsCollidingWithPlayerList;
    }
    public static bool IsCollidingWithABush()
    {
        if (objectsCollidingWithPlayerList.Count != 0)
        {
            foreach (GameObject item in objectsCollidingWithPlayerList)
            {
                if (item.tag == "Dashable")
                {
                    return true;
                }
            }
        }
        return false;
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


    //Helper functions -------------------------------------------------------------
    //Vectors
    public static float GetDistanceBetweenObjectsIn2D(GameObject firstObject, GameObject secondObject)
    {
        return GetDeltaPositionFromToIn2D(firstObject, secondObject).magnitude;
    }
    public static Vector3 GetDeltaPositionFromToIn2D(GameObject firstObject, GameObject secondObject)
    {
        Vector3 myPositionVector = firstObject.transform.position;
        myPositionVector.z = 0;
        Vector3 targetPosition = secondObject.transform.position;
        targetPosition.z = 0;

        return (targetPosition - myPositionVector);
    }
    public static Vector3 GetFromToVectorIn2D(Vector3 firstPosition, Vector3 secondPosition)
    {
        firstPosition.z = 0;
        secondPosition.z = 0;

        return (secondPosition - firstPosition);
    }
    public static Vector3 GetDirectionVector(float speed, float zDirectionInDegrees)
    {
        Vector3 returnVector = speed * GetNormalizedDirectionVector(zDirectionInDegrees);
        return returnVector;

    }
    public static Vector3 GetNormalizedDirectionVector(float zDirectionInDegrees)
    {
        float xStepMove = -Mathf.Sin(Mathf.Deg2Rad * zDirectionInDegrees);
        float yStepMove = Mathf.Cos(Mathf.Deg2Rad * zDirectionInDegrees);
        Vector3 returnVector = new Vector3(xStepMove, yStepMove, 0);
        return returnVector.normalized;
    }


    //Rotation
    public static Quaternion GetRotationFromToIn2D(Vector3 firstPosition, Vector3 secondPosition)
    {
        Vector3 deltaPosition = GetFromToVectorIn2D(firstPosition, secondPosition);

        float zRotation = Mathf.Rad2Deg * Mathf.Atan(deltaPosition.y / deltaPosition.x);

        return Quaternion.Euler(0, 0, zRotation);
    }
    public static Quaternion GetRandomRotationInRange(float leftSpread, float rightSpread)
    {
        Quaternion returnRotation = Quaternion.Euler(0, 0, Random.Range(-rightSpread, leftSpread));
        return returnRotation;
    }


    //Find the closest entities
    //Enemies
    public static GameObject GetTheNearestEnemy(Vector3 positionVector, int myTeam)
    {
        return FindTheClosestObjectInList(GetMyEnemyList(myTeam), positionVector);
    }
    public static List<GameObject> GetMyEnemyList(int myTeam)
    {
        return SubtractAlliesFromList(entityList, myTeam);
    }
    public static List<GameObject> SubtractAlliesFromList(List<GameObject> inputList, int myTeam)
    {
        for (int i = inputList.Count - 1; i >= 0; i--)
        {
            DamageReceiver damageReceiver = inputList[i].GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                if (damageReceiver.GetTeam() == myTeam)
                {
                    inputList.Remove(inputList[i]);
                }
            }
        }
        return inputList;
    }

    //Allies
    public static GameObject GetTheNearestAlly(Vector3 positionVector, int myTeam, GameObject gameObjectToIgnore)
    {
        return FindTheClosestObjectInList(GetMyAllyList(myTeam, gameObjectToIgnore), positionVector);
    }
    public static List<GameObject> GetMyAllyList(int myTeam, GameObject gameObjectToIgnore)
    {
        return SubtractMeAndEnemiesFromList(entityList, myTeam, gameObjectToIgnore);
    }
    public static List<GameObject> SubtractMeAndEnemiesFromList(List<GameObject> inputList, int myTeam, GameObject gameObjectToIgnore)
    {
        for (int i = inputList.Count - 1; i >= 0; i--)
        {
            DamageReceiver damageReceiver = inputList[i].GetComponent<DamageReceiver>();
            if (damageReceiver != null)
            {
                if (damageReceiver.GetTeam() != myTeam)
                {
                    inputList.Remove(inputList[i]);
                }
                //Remove itself from ally list
                if (inputList[i] == gameObjectToIgnore)
                {
                    inputList.Remove(inputList[i]);
                }
            }
        }
        return inputList;
    }


    //Useful
    public static GameObject FindTheClosestObjectInList(List<GameObject> possibleTargetList, Vector3 positionVector)
    {
        GameObject currentNearestTarget = null;

        if (possibleTargetList.Count != 0)
        {
            foreach (var item in possibleTargetList)
            {
                if (currentNearestTarget == null)
                {
                    currentNearestTarget = item;
                }
                bool currentTargetIsCloser = (positionVector - item.transform.position).magnitude < (positionVector - currentNearestTarget.transform.position).magnitude;
                if (currentTargetIsCloser)
                {
                    currentNearestTarget = item;
                }
            }
            return currentNearestTarget;
        }
        else
        {
            return null;
        }
    }
}
