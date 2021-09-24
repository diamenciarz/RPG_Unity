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
            if (inventory.inventorySlotArray[i].item.itemID == inputItem.itemID)
            {
                //Add an amount of it
                inventory.inventorySlotArray[i].AddAmount(itemAmount);
                return;
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
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath), FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(), this);
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
