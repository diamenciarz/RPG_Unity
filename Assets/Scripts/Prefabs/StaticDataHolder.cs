using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticDataHolder
{
    public static List<GameObject> dashableObjectList = new List<GameObject>();

    private static List<GameObject> obstacleList = new List<GameObject>();
    private static List<GameObject> projectileList = new List<GameObject>();
    private static List<GameObject> playerProjectileList = new List<GameObject>();
    private static List<GameObject> entityList = new List<GameObject>();
    private static List<GameObject> objectsCollidingWithPlayerList = new List<GameObject>();

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
    public static List<GameObject> GetObstacleList()
    {
        return CloneList(obstacleList);
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
        return CloneList(dashableObjectList);
    }


    // Dashable Object Methods
    public static GameObject GetTheClosestDashableObject(Vector3 position)
    {
        List<GameObject> dashableObjectList = StaticDataHolder.GetDashableObjectList();
        return FindTheClosestObjectInList(dashableObjectList, position);
    }
    public static GameObject GetTheClosestDashableObject(Vector3 position, float range)
    {
        List<GameObject> dashableObjectList = StaticDataHolder.GetDashableObjectList();
        GameObject dashableObject = FindTheClosestObjectInList(dashableObjectList, position);

        float distance = (dashableObject.transform.position - position).magnitude;
        if (distance <= range)
        {
            return dashableObject;
        }
        return null;
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
    public static List<GameObject> GetProjectileList()
    {
        return CloneList(projectileList);
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
    public static List<GameObject> GetPlayerProjectileList()
    {
        return CloneList(playerProjectileList);
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
    public static List<GameObject> GetEntityList()
    {
        return CloneList(entityList);
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
        return CloneList(objectsCollidingWithPlayerList);
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
    public static Vector3 GetTranslatedMousePosition(Vector3 position)
    {
        Vector3 returnVector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        returnVector.z = position.z;
        return returnVector;

    }

    //Rotation
    public static Quaternion GetRotationFromToIn2D(Vector3 firstPosition, Vector3 secondPosition)
    {
        Vector3 deltaPosition = GetFromToVectorIn2D(firstPosition, secondPosition);

        float ratio = deltaPosition.y / deltaPosition.x;
        float zRotation = Mathf.Rad2Deg * Mathf.Atan(ratio);

        if (deltaPosition.x <= 0)
        {
            zRotation += 180;
        }
        return Quaternion.Euler(0, 0, zRotation);
    }
    public static Quaternion GetRandomRotationInRangeZ(float leftSpread, float rightSpread)
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
        return SubtractAlliesFromList(CloneList(entityList), myTeam);
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
        return SubtractMeAndEnemiesFromList(CloneList(entityList), myTeam, gameObjectToIgnore);
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
        if (possibleTargetList.Count != 0)
        {
            GameObject currentNearestTarget = possibleTargetList[0];
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
    public static List<GameObject> CloneList(List<GameObject> inputList)
    {
        List<GameObject> returnList = new List<GameObject>(inputList.Count);
        inputList.ForEach((item) => returnList.Add(item));
        return returnList;
    }
    public static void TryPlaySound(AudioClip sound, Vector3 soundPosition, float volume)
    {
        if (sound != null)
        {
            if (GetSoundCount() <= (GetSoundLimit() - 4))
            {
                AudioSource.PlayClipAtPoint(sound, soundPosition, volume);
                AddSoundDuration(sound.length);
            }
        }
    }
}
