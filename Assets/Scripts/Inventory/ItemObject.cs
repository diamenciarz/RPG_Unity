using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Tool,
    Food,
    UsableItem
}

public abstract class ItemObject : ScriptableObject
{
    public GameObject itemPrefab;
    public ItemType itemType;
    [TextArea(15,20)]
    public string itemDescription;
    
}
