using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DisplayInventory : MonoBehaviour
{
    public GameObject inventoryPrefab;
    public Sprite nullImage;
    public InventoryObject inventoryToDisplay;

    public int xOffsetOfItemSlots;
    public int yOffsetOfItemSlots;
    public int amountOfColumns;

    public int xDisplayStart;
    public int yDisplayStart;

    Dictionary<GameObject, InventorySlot> displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

    private void OnEnable()
    {
        EventManager.StartListening("Update Inventory Display",UpdateDisplay);
    }
    private void OnDisable()
    {
        EventManager.StopListening("Update Inventory Display", UpdateDisplay);
    }
    private void Start()
    {
        CreateSlots();
        UpdateDisplay();
    }
    private void Update()
    {
        //UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            foreach (KeyValuePair<GameObject, InventorySlot> slot in displayedItemsDictionary)
            {
                //If the slot is not empty
                if (slot.Value.itemID >= 0)
                {
                    //Display the item
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventoryToDisplay.itemDatabase.getItemDictionary[slot.Value.item.itemID].itemSprite;
                    slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = slot.Value.amount == 1 ? "" : slot.Value.amount.ToString("n0");
                }
                else
                {
                    //Display an empty sprite
                    slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = nullImage;
                    slot.Key.GetComponentInChildren<TextMeshProUGUI>().text = "";
                }
            }
        }
    }

    private void CreateSlots()
    {
        displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            var itemSlot = Instantiate(inventoryPrefab, Vector3.zero,Quaternion.identity,transform);
            itemSlot.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            //Debug.Log("Created slot");
            displayedItemsDictionary.Add(itemSlot, inventoryToDisplay.inventory.inventorySlotArray[i]);
        }
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots*(positionNumber / amountOfColumns)),0f);
    }
}
