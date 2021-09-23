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
    public int inventorySize = 24;

    public void AddItemToInventory(Item inputItem, int itemAmount)
    {
        //If the item has properties, then add it to an empty slot
        if (inputItem.itemBuffs.Length > 0)
        {
            SetEmptySlot(inputItem, itemAmount);
            return;
        }
        //If the item is stackable
        //Check if this item is already in the inventory
        for (int i = 0; i < inventory.inventorySlotList.Length; i++)
        {
            if (inventory.inventorySlotList[i].item.itemID == inputItem.itemID)
            {
                //Add an amount of it
                inventory.inventorySlotList[i].AddAmount(itemAmount);
                break;
            }
        }
        //If no instance was found, then just add it to an empty slot
        SetEmptySlot(inputItem, itemAmount);
    }
    private int ReturnFreeInventorySlotIndex()
    {
        int returnIndex = -1;
        for (int i = 0; i < inventory.inventorySlotList.Length; i++)
        {
            if (inventory.inventorySlotList[i].itemID <= -1)
            {
                returnIndex = i;
            }
        }
        return returnIndex;
    }
    public InventorySlot SetEmptySlot(Item inputItem, int inputAmount)
    {
        for (int i = 0; i < inventory.inventorySlotList.Length; i++)
        {
            if (inventory.inventorySlotList[i].itemID <= -1)
            {
                inventory.inventorySlotList[i].UpdateSlot(inputItem.itemID, inputItem,inputAmount);
                return inventory.inventorySlotList[i];
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
    }
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        inventory.inventorySlotList = new InventorySlot[28];
    }
}
[System.Serializable]
public class Inventory
{
    public InventorySlot[] inventorySlotList = new InventorySlot[28];
}
[System.Serializable]
public class InventorySlot
{
    public int itemID;
    public int amount;
    public Item item;
    public InventorySlot()
    {

        itemID = -1;
        item = null;
        amount = 0;
    }
    public InventorySlot(int inputItemID, Item inputItem, int itemAmount)
    {

        itemID = inputItemID;
        item = inputItem;
        amount = itemAmount;
    }
    public void UpdateSlot(int inputItemID, Item inputItem, int itemAmount)
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
