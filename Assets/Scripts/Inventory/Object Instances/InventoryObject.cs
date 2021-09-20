using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]

public class InventoryObject : ScriptableObject
{ 
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
            inventorySlotContainer.Add(new InventorySlot(inputItem,itemAmount));
        }
    }
    public void ClearInventory()
    {
        inventorySlotContainer.Clear();
    }
}
[System.Serializable]
public class InventorySlot
{
    public ItemObject item;
    public int amount;

    public InventorySlot(ItemObject inputItem,int itemAmount)
    {
        item = inputItem;
        amount = itemAmount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}
