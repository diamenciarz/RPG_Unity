using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]

public class InventoryObject : ScriptableObject, ISerializationCallbackReceiver
{
    public string savePath;
    private ItemDatabaseObject itemDatabase;
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
    private void OnEnable()
    {
        #if UNITY_EDITOR
        itemDatabase = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath("Assets/Resources/Item Database.asset",typeof(ItemDatabaseObject));
        #else
        itemDatabase = Resources.Load<ItemDatabaseObject>("Database");
        #endif
    }
    public void ClearInventory()
    {
        inventorySlotContainer.Clear();
    }
    
    public void SaveInventory()
    {
        string saveData = JsonUtility.ToJson(this, true);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(string.Concat(Application.persistentDataPath, savePath));
        bf.Serialize(file,savePath);
        file.Close();
        Debug.Log("Inventory saved");

    }
    public void LoadInventory()
    {
        if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(string.Concat(Application.persistentDataPath, savePath),FileMode.Open);
            JsonUtility.FromJsonOverwrite(bf.Deserialize(file).ToString(),this);
            file.Close();
            Debug.Log("Inventory loaded");
        }
    }

    public void OnAfterDeserialize()
    {
        for (int i = 0; i < inventorySlotContainer.Count; i++)
        {
            if (inventorySlotContainer[i].itemID < itemDatabase.getItemDictionary.Count)
            {
                inventorySlotContainer[i].item = itemDatabase.getItemDictionary[inventorySlotContainer[i].itemID];
            }
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
