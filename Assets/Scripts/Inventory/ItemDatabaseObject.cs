using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] Items;
    public Dictionary<ItemObject, int> itemIDDictionary = new Dictionary<ItemObject, int>();
    public void OnAfterDeserialize()
    {
        itemIDDictionary = new Dictionary<ItemObject, int>();
        //Fills a dictionary with items from the array
        for (int i = 0; i < Items.Length; i++)
        {

        }
    }
}
