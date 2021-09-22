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
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotContainer.Count; i++)
        {
            if (displayedItemsDictionary.ContainsKey(inventoryToDisplay.inventory.inventorySlotContainer[i]))
            {
                displayedItemsDictionary[inventoryToDisplay.inventory.inventorySlotContainer[i]].GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventory.inventorySlotContainer[i].amount.ToString("n0");
            }
            else
            {
                var itemSlot = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
                itemSlot.transform.GetChild(0).GetComponent<Image>().sprite = inventoryPrefab.GetComponent<Image>().sprite;
                itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
                itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventory.inventorySlotContainer[i].amount.ToString("n0");
                displayedItemsDictionary.Add(inventoryToDisplay.inventory.inventorySlotContainer[i],itemSlot);
            }
        }
    }

    private void CreateDisplay()
    {
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotContainer.Count; i++)
        {
            var itemSlot = Instantiate(inventoryPrefab, Vector3.zero,Quaternion.identity,transform);
            itemSlot.transform.GetChild(0).GetComponent<Image>().sprite = inventoryPrefab.GetComponent<Image>().sprite;
            itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            itemSlot.GetComponentInChildren<TextMeshProUGUI>().text = inventoryToDisplay.inventory.inventorySlotContainer[i].amount.ToString("n0");
            displayedItemsDictionary.Add(inventoryToDisplay.inventory.inventorySlotContainer[i], itemSlot);
        }
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots*(positionNumber / amountOfColumns)),0f);
    }
}
