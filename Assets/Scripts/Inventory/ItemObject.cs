using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    Tool,
    Food,
    UsableItem
}

public enum ItemAttributes
{
    Vision,
    Speed,
    Dash,
    Attack,
    Defence
}

public abstract class ItemObject : ScriptableObject
{
    public int itemId;
    public Sprite itemSprite;
    public ItemType itemType;
    [TextArea(15,20)]
    public string itemDescription;
    public ItemBuff[] itemBuffs;

    public Item CreateNewItem()
    {
        Item returnItem = new Item(this);
        return returnItem;
    }
}
[System.Serializable]
public class Item
{
    public string itemName;
    public int itemID;
    public ItemBuff[] itemBuffs;

    public Item()
    {
        itemID = -1;
    }
    public Item(ItemObject itemObject)
    {
        itemName = itemObject.name;
        itemID = itemObject.itemId;

        itemBuffs = new ItemBuff[itemObject.itemBuffs.Length];
        for (int i = 0; i < itemObject.itemBuffs.Length; i++)
        {
            ItemAttributes itemAttribute = itemObject.itemBuffs[i].itemAttributesEnumerator;
            //Set random value boundaries for the created object and the enumerator value
            itemBuffs[i] = new ItemBuff(itemObject.itemBuffs[i].minValue, itemObject.itemBuffs[i].maxValue, itemAttribute);
        }
    }
}

[System.Serializable]
public class ItemBuff
{
    public ItemAttributes itemAttributesEnumerator;
    public int attributeValue;
    public int minValue;
    public int maxValue;
    public ItemBuff(int inputMinValue, int inputMaxValue, ItemAttributes itemAttribute)
    {
        itemAttributesEnumerator = itemAttribute;
        minValue = inputMinValue;
        maxValue = inputMaxValue;
        GenerateAttributeValue();
    }
    public void GenerateAttributeValue()
    {
        attributeValue = UnityEngine.Random.Range(minValue,maxValue);
    }
}
