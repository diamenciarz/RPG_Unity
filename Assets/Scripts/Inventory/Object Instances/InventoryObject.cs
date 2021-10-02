using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEditor;

[CreateAssetMenu(fileName = "New Inventory", menuName = "Inventory System/Inventory")]

public class InventoryObject : ScriptableObject
{
    public int inventorySize;
    public string savePath;
    public ItemDatabaseObject itemDatabase;
    public Inventory inventory;

    private void Awake()
    {
        //Fill inventory with empty slots up to inventorySize
        ClearInventory();
    }
    public void AddItemToInventory(ItemDataForSlots inputItem, int itemAmount)
    {
        //If the item has properties, then add it to an empty slot
        bool isStackable = inputItem.itemBuffs.Length <= 0;
        if (!isStackable)
        {
            InsertItemToEmptySlot(inputItem, itemAmount);
            return;
        }
        //If the item is stackable

        int inventorySizeToCheck = inventory.inventorySlotArray.Length;
        //Check if this item is already in the inventory
        for (int i = 0; i < inventorySizeToCheck; i++)
        {
            bool isTheCurrentSlotEmpty = inventory.inventorySlotArray[i].amount != 0;
            //Check if the slot is not empty
            if (!isTheCurrentSlotEmpty)
            {
                bool areBothIDsMatching = (inventory.inventorySlotArray[i].item.itemID == inputItem.itemID);
                //Check if the current slot contains the item we are looking to add to inventory
                if (areBothIDsMatching)
                {
                    inventory.inventorySlotArray[i].AddItemAmount(itemAmount);
                    return;
                }
            }
        }
        //If this item is not in the inventory yet, then just add it to an empty slot
        InsertItemToEmptySlot(inputItem, itemAmount);
    }
    public void InsertItemToEmptySlot(ItemDataForSlots inputItem, int inputAmount)
    {
        int freeSlotIndex = GetFirstFreeInventorySlotIndex();
        if (freeSlotIndex > -1)
        {
            inventory.inventorySlotArray[freeSlotIndex].UpdateSlotItem(inputItem, inputAmount);
            return;
        }
        Debug.Log("Inventory full - item was lost");
    }
    public InventorySlot GetFirstFreeInventorySlotFromInventory()
    {
        int freeSlotIndex = GetFirstFreeInventorySlotIndex();
        return inventory.inventorySlotArray[freeSlotIndex];
    }
    private int GetFirstFreeInventorySlotIndex()
    {
        int returnIndex = -1;

        int inventoryLength = inventory.inventorySlotArray.Length;
        for (int i = 0; i < inventoryLength; i++)
        {
            bool isTheSlotEmpty = inventory.inventorySlotArray[i].item.itemID <= -1;
            if (isTheSlotEmpty)
            {
                //Return empty slot index from the inventory
                return i;
            }
        }
        return returnIndex;
    }
    public void TryToSwapItemsInSlots(InventorySlot slotToMoveFrom, InventorySlot moveToSlot)
    {
        //Check if is moving to a slot
        if (moveToSlot != null && slotToMoveFrom != null)
        {
            //Don't swap an empty slot
            if (slotToMoveFrom.amount > 0)
            {
                //
                ItemDataForSlots secondItemData = GetItemDataFromSlot(moveToSlot);
                //Check slot item types to see, if can swap at all
                if (IsItemTypeMatchingSlot(slotToMoveFrom, moveToSlot) && IsItemTypeMatchingSlot(moveToSlot, slotToMoveFrom))
                {
                    //Check if should swap items in slots or add items from one slot to another
                    bool areBothIDsSame = slotToMoveFrom.item.itemID == secondItemData.itemID;
                    bool isFirstSlotItemStackable = slotToMoveFrom.item.itemBuffs.Length == 0;
                    bool isSecondSlotItemStackable = secondItemData.itemBuffs.Length == 0;

                    if (areBothIDsSame && isFirstSlotItemStackable && isSecondSlotItemStackable)
                    {
                        //Add items from one slot to another
                        moveToSlot.AddItemAmount(slotToMoveFrom.amount);
                        slotToMoveFrom.RemoveItem();
                    }
                    else
                    {
                        //Swap items in slots
                        InventorySlot saveSecondSlot = new InventorySlot(secondItemData, moveToSlot.amount);
                        moveToSlot.UpdateSlotItem(slotToMoveFrom.item, slotToMoveFrom.amount);
                        slotToMoveFrom.UpdateSlotItem(saveSecondSlot.item, saveSecondSlot.amount);
                    }
                    Debug.Log("Swapped items in two slots");
                }
            }
        }
    }
    private bool IsItemTypeMatchingSlot(InventorySlot inventorySlotToCheck, InventorySlot inventorySlotForItemData)
    {
        ItemDataForSlots firstItemData = GetItemDataFromSlot(inventorySlotForItemData);
        if (firstItemData.itemID == -1)
        {
            return true;
        }
        else
        {
            return inventorySlotToCheck.CanPlaceItemInSlot(itemDatabase.getItemObjectDictionary[firstItemData.itemID]);
        }
    }
    private ItemDataForSlots GetItemDataFromSlot(InventorySlot slot)
    {
        ItemDataForSlots itemData;

        if (slot.item == null)
        {
            Debug.Log("Item data was inded null and this method is useful");
            itemData = new ItemDataForSlots();
        }
        else
        {
            itemData = slot.item;
        }
        return itemData;
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

            file.Close();
        }
        EventManager.TriggerEvent("Update Inventory Display");
    }
    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        CorrectInventoryArrayLength();
        inventory.ClearInventory();
        //EventManager.TriggerEvent("Update Inventory Display");
    }
    public void CorrectInventoryArrayLength()
    {
        bool isInventoryTheRightLength = inventory.inventorySlotArray.Length == inventorySize;
        if (!isInventoryTheRightLength)
        {
            inventory.inventorySlotArray = new InventorySlot[inventorySize];
        }
    }
}
[System.Serializable]
public class Inventory
{
    public InventorySlot[] inventorySlotArray;
    public void ClearInventory()
    {
        for (int i = 0; i < inventorySlotArray.Length; i++)
        {
            inventorySlotArray[i].UpdateSlotItem(new ItemDataForSlots(), 0);
        }
    }
}
[System.Serializable]
public class InventorySlot
{
    public ItemType[] allowedItemTypes = new ItemType[0];
    public GameObject displayGameObject;
    public UserInterface parent;
    public int amount;
    public ItemDataForSlots item;
    public InventorySlot()
    {
        item = new ItemDataForSlots();
        amount = 0;
    }
    public InventorySlot(ItemDataForSlots inputItem, int itemAmount)
    {
        item = inputItem;
        amount = itemAmount;
    }
    public void UpdateSlotItem(ItemDataForSlots inputItem, int itemAmount)
    {
        item = inputItem;
        amount = itemAmount;
        EventManager.TriggerEvent("Update Item Display", displayGameObject);
        Debug.Log("Updated item slot");
    }
    public void AddItemAmount(int value)
    {
        amount += value;
        EventManager.TriggerEvent("Update Item Display", displayGameObject);
    }

    public void RemoveItem()
    {
        item = new ItemDataForSlots();
        amount = 0;
        EventManager.TriggerEvent("Update Item Display", displayGameObject);

    }
    public bool CanPlaceItemInSlot(ItemObject itemToCheck)
    {
        if (itemToCheck != null)
        {
            bool isAnyItemTypeAllowed = allowedItemTypes.Length <= 0;
            if (isAnyItemTypeAllowed)
            {
                //Debug.Log("Can swap items");
                return true;
            }

            for (int i = 0; i < allowedItemTypes.Length; i++)
            {
                bool doItemTypesMatch = allowedItemTypes[i] == itemToCheck.itemType;
                if (doItemTypesMatch)
                {
                    Debug.Log("Can swap items");
                    return true;
                }
            }
        }
        return false;
    }
    public void SetDisplayGameObject(GameObject _object)
    {
        Debug.Log("Set display game object to: " + _object);
        displayGameObject = _object;
    }
}
