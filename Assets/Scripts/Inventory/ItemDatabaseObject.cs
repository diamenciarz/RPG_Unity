using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Item Database")]

public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] itemObjectArray;
    public Dictionary<int, ItemObject> getItemObjectDictionary = new Dictionary<int, ItemObject>();

    public void OnAfterDeserialize()
    {
        getItemObjectDictionary = new Dictionary<int, ItemObject>();

        //Fills a dictionary with items from the array
        for (int i = 0; i < itemObjectArray.Length; i++)
        {
            itemObjectArray[i].itemId = i;
            getItemObjectDictionary.Add(i,itemObjectArray[i]);
        }
    }
    public void OnBeforeSerialize()
    {

    }
}
