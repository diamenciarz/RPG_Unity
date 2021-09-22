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
    public int itemId;
    public Sprite itemSprite;
    public ItemType itemType;
    [TextArea(15,20)]
    public string itemDescription;
}
[System.Serializable]
public class Item
{
    public string itemName;
    public int itemID;

    public Item(ItemObject itemObject)
    {
        itemName = itemObject.name;
        itemID = itemObject.itemId;
    }
}
