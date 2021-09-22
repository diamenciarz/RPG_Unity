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

    public void AddItemToInventory(Item inputItem, int itemAmount)
    {
        bool hasItem = false;
        for (int i = 0; i < inventory.inventorySlotContainer.Count; i++)
        {
            if (inventory.inventorySlotContainer[i].item == inputItem)
            {
                inventory.inventorySlotContainer[i].AddAmount(itemAmount);
                hasItem = true;
                break;
            }
        }
        if (hasItem == false)
        {
            inventory.inventorySlotContainer.Add(new InventorySlot(inputItem.itemID, inputItem, itemAmount));


        }
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
        inventory = new Inventory();
    }
}
[System.Serializable]
public class Inventory
{
    public List<InventorySlot> inventorySlotContainer = new List<InventorySlot>();
}
[System.Serializable]
public class InventorySlot
{
    public int itemID;
    public Item item;
    public int amount;

    public InventorySlot(int inputItemID ,Item inputItem,int itemAmount)
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
