using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public abstract class UserInterface : MonoBehaviour
{

    public Sprite nullImage;
    public InventoryObject inventoryToDisplay;

    protected Dictionary<GameObject, InventorySlot> displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

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

    private void UpdateDisplay()
    {
        //Debug.Log("Called Update Inventory");
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            foreach (KeyValuePair<GameObject, InventorySlot> slot in displayedItemsDictionary)
            {
                //If the slot is not empty
                if (slot.Value.amount > 0)
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

    public abstract void CreateSlots();
    protected void OnEnter(GameObject obj)
    {
        DataHolder.mouseItem.hoverMouseGO = obj;
        if (displayedItemsDictionary.ContainsKey(obj))
        {
            DataHolder.mouseItem.hoverItemSlot = displayedItemsDictionary[obj];
        }
    }
    protected void OnExit(GameObject obj)
    {
        DataHolder.mouseItem.hoverMouseGO = null;
        DataHolder.mouseItem.hoverItemSlot = null;
    }
    protected void OnBeginDrag(GameObject obj)
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
        DataHolder.mouseItem.beginMouseGO = mouseObject;
        DataHolder.mouseItem.beginItemSlot = displayedItemsDictionary[obj];
    }
    protected void OnEndDrag(GameObject obj)
    {
        if (DataHolder.mouseItem.hoverMouseGO != null)
        {
            inventoryToDisplay.SwapItemsInSlots(DataHolder.mouseItem.beginItemSlot, DataHolder.mouseItem.hoverItemSlot);
        }
        else
        {
            inventoryToDisplay.DeleteItemFromSlot(DataHolder.mouseItem.beginItemSlot);
        }

        //Cleanup
        Destroy(DataHolder.mouseItem.beginMouseGO);
        DataHolder.mouseItem.beginItemSlot = null;
    }
    protected void OnDrag(GameObject obj)
    {
        if (DataHolder.mouseItem.beginMouseGO != null)
        {
            DataHolder.mouseItem.beginMouseGO.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
}
public class MouseItem
{
    public GameObject beginMouseGO;
    public InventorySlot beginItemSlot;
    public InventorySlot hoverItemSlot;
    public GameObject hoverMouseGO;
}
