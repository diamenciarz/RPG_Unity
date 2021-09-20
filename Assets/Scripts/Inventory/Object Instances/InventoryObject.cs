using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]

public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public ItemDatabaseObject itemDatabase;
    public List<InventorySlot> inventorySlotContainer = new List<InventorySlot>();

    public void AddItemToInventory(ItemObject inputItem, int itemAmount)
    {
        bool hasItem = false;
        for (int i = 0; i < inventorySlotContainer.Count; i++)
        {
            if (inventorySlotContainer[i].item == inputItem)
            {
                inventorySlotContainer[i].AddAmount(itemAmount);
                hasItem = true;
                break;
            }
        }
        if (hasItem == false)
        {
            inventorySlotContainer.Add(new InventorySlot(itemDatabase.getItemIDDictionary[inputItem], inputItem, itemAmount));
        }
    }
    public void ClearInventory()
    {
        inventorySlotContainer.Clear();
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < inventorySlotContainer.Count; i++)
        {
            inventorySlotContainer[i].item = itemDatabase.getItemDictionary[inventorySlotContainer[i].itemID];
        }
    }

    public void OnBeforeSerialize()
    {
    }
}
[System.Serializable]
public class InventorySlot
{
    public int itemID;
    public ItemObject item;
    public int amount;

    public InventorySlot(int inputItemID ,ItemObject inputItem,int itemAmount)
    {

        itemID = inputItemID;
        item = inputItem;
        amount = itemAmount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}
