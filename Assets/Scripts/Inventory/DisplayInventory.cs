using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class DisplayInventory : MonoBehaviour
{
    public MouseItem mouseItem = new MouseItem();

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
        EventManager.StartListening("Update Inventory Display", UpdateDisplay);
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
        //Debug.Log("Called Update Inventory");
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            foreach (KeyValuePair<GameObject, InventorySlot> slot in displayedItemsDictionary)
            {
                //If the slot is not empty
                if (slot.Value.amount> 0)
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
            var itemGameObject = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            itemGameObject.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            //Debug.Log("Created slot");
            displayedItemsDictionary.Add(itemGameObject, inventoryToDisplay.inventory.inventorySlotArray[i]);

            //Hook up each slot to unity events
            AddEvent(itemGameObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.PointerExit, delegate { OnExit(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.BeginDrag, delegate { OnBeginDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.EndDrag, delegate { OnEndDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.Drag, delegate { OnDrag(itemGameObject); });
        }
    }
    private void OnEnter(GameObject obj)
    {
        mouseItem.hoverMouseGO = obj;
        if (displayedItemsDictionary.ContainsKey(obj))
        {
            mouseItem.hoverItemSlot = displayedItemsDictionary[obj];
        }
    }
    private void OnExit(GameObject obj)
    {
        mouseItem.hoverMouseGO = null;
        mouseItem.hoverItemSlot = null;
    }
    private void OnBeginDrag(GameObject obj)
    {
        GameObject mouseObject = new GameObject();
        //MouseItem mouseItem = new MouseItem(mouseObject); //?
        RectTransform rectTransform = mouseObject.AddComponent<RectTransform>();

        //Make sure the size of the created clone item is the same as the size of the original itemSlot
        Vector2 itemSize = obj.GetComponent<RectTransform>().sizeDelta;
        rectTransform.sizeDelta = itemSize;
        mouseObject.transform.SetParent(transform.parent);

        if (displayedItemsDictionary[obj].item.itemID >= 0)
        {
            Image image = mouseObject.AddComponent<Image>();
            image.sprite = inventoryToDisplay.itemDatabase.getItemDictionary[displayedItemsDictionary[obj].item.itemID].itemSprite;
            image.raycastTarget = false;
        }
        mouseItem.beginMouseGO = mouseObject;
        mouseItem.beginItemSlot = displayedItemsDictionary[obj];
    }
    private void OnEndDrag(GameObject obj)
    {
        if (mouseItem.hoverMouseGO != null)
        {
            inventoryToDisplay.SwapItemsInSlots(mouseItem.beginItemSlot, mouseItem.hoverItemSlot);
        }
        else
        {
            inventoryToDisplay.DeleteItemFromSlot(mouseItem.beginItemSlot);
        }

        //Cleanup
        Destroy(mouseItem.beginMouseGO);
        mouseItem.beginItemSlot = null;
    }
    private void OnDrag(GameObject obj)
    {
        if (mouseItem.beginMouseGO != null)
        {
            mouseItem.beginMouseGO.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
    private void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots * (positionNumber / amountOfColumns)), 0f);
    }
}
public class MouseItem
{
    public GameObject beginMouseGO;
    public InventorySlot beginItemSlot;
    public InventorySlot hoverItemSlot;
    public GameObject hoverMouseGO;
}
