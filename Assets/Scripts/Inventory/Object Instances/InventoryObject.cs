using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]

public class InventoryObject : ScriptableObject
{
    public string savePath;
    public ItemDatabaseObject itemDatabase;
    public Inventory inventory;
    public int inventorySize = 28;

    private void Awake()
    {
        //Fill inventory with empty slots up to inventorySize
        ClearInventory();
    }
    public void AddItemToInventory(Item inputItem, int itemAmount)
    {
        //Debug.Log("Added " + inputItem.itemName + " to inventory.");
        //If the item has properties, then add it to an empty slot
        if (inputItem.itemBuffs.Length > 0)
        {
            SetEmptySlot(inputItem, itemAmount);
            return;
        }
        //If the item is stackable
        //Check if this item is already in the inventory
        for (int i = 0; i < inventory.inventorySlotArray.Length; i++)
        {
            //Check if the slot is not empty
            if (inventory.inventorySlotArray[i].amount != 0)
            {
                //Check if the current slot contains the item we are looking to add to inventory
                if (inventory.inventorySlotArray[i].item.itemID == inputItem.itemID)
                {
                    //Add an amount of it
                    inventory.inventorySlotArray[i].AddAmount(itemAmount);
                    return;
                }
            }
        }
        //If no instance was found, then just add it to an empty slot
        SetEmptySlot(inputItem, itemAmount);
    }
    private int ReturnFirstFreeInventorySlotIndex()
    {
        int returnIndex = -1;
        for (int i = 0; i < inventory.inventorySlotArray.Length; i++)
        {
            if (inventory.inventorySlotArray[i].item.itemID <= -1)
            {
                returnIndex = i;
            }
        }
        return returnIndex;
    }
    public InventorySlot SetEmptySlot(Item inputItem, int inputAmount)
    {
        for (int i = 0; i < inventory.inventorySlotArray.Length; i++)
        {
            if (inventory.inventorySlotArray[i].item.itemID <= -1)
            {
                inventory.inventorySlotArray[i].UpdateSlot(inputItem,inputAmount);
                return inventory.inventorySlotArray[i];
            }
        }
        Debug.Log("Inventory full");
        return null;
    }
    public void SwapItemsInSlots(InventorySlot itemToMove, InventorySlot moveToSlot)
    {
        InventorySlot saveSecondSlot = new InventorySlot(moveToSlot.item, moveToSlot.amount);
        moveToSlot.UpdateSlot(itemToMove.item,itemToMove.amount);
        itemToMove.UpdateSlot(saveSecondSlot.item, saveSecondSlot.amount);
        EventManager.TriggerEvent("Update Inventory Display");
    }
    public void DeleteItemFromSlot(InventorySlot itemSlot)
    {
        itemSlot.UpdateSlot(null,0);
        EventManager.TriggerEvent("Update Inventory Display");
    }
    [ContextMenu("Save Inventory")]
    public void SaveInventory()
    {
        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file, saveData);
        file.Close();
        EventManager.TriggerEvent("Update Inventory Display");
    }
    [ContextMenu("Load Inventory")]
    public void LoadInventory()
    {
        if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            //Load data from JSON into newInventory
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
            /* //This turns out to not be needed
            //Create a temporary inventory
            Inventory newInventory = new Inventory();
            //Loop through the current inventory and update it slot by slot
            for (int i = 0; i < inventory.inventorySlotArray.Length; i++)
            {
                inventory.inventorySlotArray[i].UpdateSlot();
            }*/
            file.Close();
        }
        EventManager.TriggerEvent("Update Inventory Display");
    }
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        inventory.inventorySlotArray = new InventorySlot[inventorySize];
        EventManager.TriggerEvent("Update Inventory Display");
    }
    public void UpdateInventory()
    {

    }
}
[System.Serializable]
public class Inventory
{
    public InventorySlot[] inventorySlotArray;
}
[System.Serializable]
public class InventorySlot
{
    public int amount;
    public Item item;
    public InventorySlot()
    {
        item = null;
        amount = 0;
    }
    public InventorySlot(Item inputItem, int itemAmount)
    {
        item = inputItem;
        amount = itemAmount;
    }
    public void UpdateSlot(Item inputItem, int itemAmount)
    {
        item = inputItem;
        amount = itemAmount;
    }
    public void AddAmount(int value)
    {
        amount += value;
    }
}
