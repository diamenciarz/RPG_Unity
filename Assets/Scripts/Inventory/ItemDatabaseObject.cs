using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory System/Items/Item Database")]

public class ItemDatabaseObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemObject[] Items;
    public Dictionary<ItemObject, int> getItemIDDictionary = new Dictionary<ItemObject, int>();
    public Dictionary<int, ItemObject> getItemDictionary = new Dictionary<int, ItemObject>();

    public void OnAfterDeserialize()
    {
        getItemIDDictionary = new Dictionary<ItemObject, int>();
        getItemDictionary = new Dictionary<int, ItemObject>();

        //Fills a dictionary with items from the array
        for (int i = 0; i < Items.Length; i++)
        {
            getItemIDDictionary.Add(Items[i],i);
            getItemDictionary.Add(i,Items[i]);
        }
    }
    public void OnBeforeSerialize()
    {

    }
}
