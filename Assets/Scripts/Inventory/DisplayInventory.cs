using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInventory : MonoBehaviour
{
    public InventoryObject inventoryToDisplay;

    public int xOffsetOfItemSlots;
    public int yOffsetOfItemSlots;
    public int amountOfColumns;

    public int xDisplayStart;
    public int yDisplayStart;

    Dictionary<InventorySlot, GameObject> displayedItemsDictionary = new Dictionary<InventorySlot, GameObject>();
    private void Start()
    {
        CreateDisplay();
    }
    private void Update()
    {
        UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        for (int i = 0; i < inventoryToDisplay.inventorySlotContainer.Count; i++)
        {
            if (displayedItemsDictionary.ContainsKey(inventoryToDisplay.inventorySlotContainer[i]))
            {
                displayedItemsDictionary[inventoryToDisplay.inventorySlotContainer[i]].GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventorySlotContainer[i].amount.ToString("n0");
            }
            else
            {
                var itemSlot = Instantiate(inventoryToDisplay.inventorySlotContainer[i].item.itemPrefab, Vector3.zero, Quaternion.identity, transform);
                itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
                itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventorySlotContainer[i].amount.ToString("n0");
                displayedItemsDictionary.Add(inventoryToDisplay.inventorySlotContainer[i],itemSlot);
            }
        }
    }

    private void CreateDisplay()
    {
        for (int i = 0; i < inventoryToDisplay.inventorySlotContainer.Count; i++)
        {
            var itemSlot = Instantiate(inventoryToDisplay.inventorySlotContainer[i].item.itemPrefab,Vector3.zero,Quaternion.identity,transform);
            itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventorySlotContainer[i].amount.ToString("n0");
            displayedItemsDictionary.Add(inventoryToDisplay.inventorySlotContainer[i], itemSlot);
        }
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots*(positionNumber / amountOfColumns)),0f);
    }
}
