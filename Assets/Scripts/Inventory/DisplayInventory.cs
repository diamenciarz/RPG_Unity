using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    public GameObject inventoryPrefab;
    public InventoryObject inventoryToDisplay;

    public int xOffsetOfItemSlots;
    public int yOffsetOfItemSlots;
    public int amountOfColumns;

    public int xDisplayStart;
    public int yDisplayStart;

    Dictionary<GameObject, InventorySlot> displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();
    private void Start()
    {
        CreateSlots();
    }
    private void Update()
    {
        //UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        /*
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotList.Count; i++)
        {
            InventorySlot slot = inventoryToDisplay.inventory.inventorySlotList[i];
            if (displayedItemsDictionary.ContainsKey(slot))
            {
                displayedItemsDictionary[slot].GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0");
            }
            else
            {
                var itemSlot = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
                itemSlot.transform.GetChild(0).GetComponent<Image>().sprite = inventoryToDisplay.itemDatabase.getItemDictionary[slot.item.itemID].itemSprite;
                itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
                itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0");
                displayedItemsDictionary.Add(slot,itemSlot);
            }
        }
        */
    }

    private void CreateSlots()
    {
        displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotList.Length; i++)
        {
            var itemSlot = Instantiate(inventoryPrefab, Vector3.zero,Quaternion.identity,transform);
            itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            Debug.Log("Created slot");
            displayedItemsDictionary.Add(itemSlot, inventoryToDisplay.inventory.inventorySlotList[i]);
        }

        /*
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotList.Count; i++)
        {
            InventorySlot slot = inventoryToDisplay.inventory.inventorySlotList[i];

            itemSlot.transform.GetChild(0).GetComponent<Image>().sprite = inventoryToDisplay.itemDatabase.getItemDictionary[slot.item.itemID].itemSprite;
            itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = slot.amount.ToString("n0");
            displayedItemsDictionary.Add(slot, itemSlot);
        }*/
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots*(positionNumber / amountOfColumns)),0f);
    }
}
