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
        AssignParentUserInterfaceToEachSlot();


        CreateSlots();
        UpdateDisplay();
    }
    public void AssignParentUserInterfaceToEachSlot()
    {
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            inventoryToDisplay.inventory.inventorySlotArray[i].parent = this;
        }
        /*
        foreach (var inventorySlot in inventoryToDisplay.inventory.inventorySlotArray)
        {
            inventorySlot.parent = this;
            //Is not setting the parent correctly
        }*/
    }
    private void UpdateDisplay()
    {
        //Debug.Log("Displayed items dictionary length: " + displayedItemsDictionary.Count);
        //For every slot, update the display game object (square with a sprite and number)
        foreach (KeyValuePair<GameObject, InventorySlot> slot in displayedItemsDictionary)
        {
            //If the slot is not empty
            if (slot.Value.amount > 0)
            {
                //Display the item
                slot.Key.transform.GetChild(0).GetComponentInChildren<Image>().sprite = inventoryToDisplay.itemDatabase.getItemObjectDictionary[slot.Value.item.itemID].itemSprite;
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
        if (displayedItemsDictionary[obj].amount > 0)
        {
            GameObject temporaryMouseObject = new GameObject();
            //MouseItem mouseItem = new MouseItem(mouseObject); //?
            RectTransform rectTransform = temporaryMouseObject.AddComponent<RectTransform>();

            //Make sure the size of the created clone item is the same as the size of the original itemSlot
            Vector2 itemSize = obj.GetComponent<RectTransform>().sizeDelta;
            rectTransform.sizeDelta = itemSize;
            temporaryMouseObject.transform.SetParent(transform.parent);

            //Debug.Log("Item id: " + displayedItemsDictionary[obj].item.itemID);
            if (displayedItemsDictionary[obj].item.itemID >= 0)
            {
                Image image = temporaryMouseObject.AddComponent<Image>();
                image.sprite = inventoryToDisplay.itemDatabase.getItemObjectDictionary[displayedItemsDictionary[obj].item.itemID].itemSprite;
                image.raycastTarget = false;
            }
            DataHolder.mouseItem.temporaryMouseGO = temporaryMouseObject;
            DataHolder.mouseItem.beginItemSlot = displayedItemsDictionary[obj];
        }
        else
        {
            DataHolder.mouseItem.temporaryMouseGO = null;
            DataHolder.mouseItem.beginItemSlot = null;
        }
    }
    protected void OnEndDrag(GameObject obj)
    {
        //Can not swap an empty slot onto an item
        if (DataHolder.mouseItem.beginItemSlot != null)
        {
            MouseItem itemOnMouse = DataHolder.mouseItem;
            GameObject mouseHoverObj = itemOnMouse.hoverMouseGO;
            InventorySlot mouseHoverSlot = itemOnMouse.hoverItemSlot;

            Dictionary<int, ItemObject> getItemObjectFromDictionary = inventoryToDisplay.itemDatabase.getItemObjectDictionary;
            //If dropped item on any item slot Game Object
            if (mouseHoverObj != null)
            {
                //If the item in hand can be moved onto the slot that the cursor is hovering over
                if (mouseHoverSlot.CanPlaceItemInSlot(getItemObjectFromDictionary[displayedItemsDictionary[obj].item.itemID]))
                {
                    //If the slot, which we are moving the item into has an item
                    if (mouseHoverSlot.item.itemID != -1)
                    {
                        // Then if the item, which the cursor is hovering over, can be moved onto the slot, which the cursor started dragging from
                        //Debug.Log("Item that we are hovering over: " + mouseHoverSlot.item.itemID);
                        if (itemOnMouse.beginItemSlot.CanPlaceItemInSlot(getItemObjectFromDictionary[mouseHoverSlot.item.itemID]))
                        {
                            inventoryToDisplay.SwapItemsInSlots(itemOnMouse.beginItemSlot, mouseHoverSlot); //itemOnMouse.hoverItemSlot.parent.displayedItemsDictionary[itemOnMouse.hoverMouseGO]
                        }
                        //Else can not swap the item
                    }
                    else
                    {
                        //Otherwise no need to check the other slot
                        inventoryToDisplay.SwapItemsInSlots(itemOnMouse.beginItemSlot, mouseHoverSlot); //itemOnMouse.hoverItemSlot.parent.displayedItemsDictionary[itemOnMouse.hoverMouseGO]
                    }
                }
                //Else, can not swap the item, so don't
            }
            else
            {
                //If not, then delete the item
                inventoryToDisplay.DeleteItemFromSlot(itemOnMouse.beginItemSlot);
            }
            //Cleanup
            Destroy(DataHolder.mouseItem.temporaryMouseGO);
            DataHolder.mouseItem.beginItemSlot = null;
        }
    }
    protected void OnDrag(GameObject obj)
    {
        if (DataHolder.mouseItem.temporaryMouseGO != null)
        {
            DataHolder.mouseItem.temporaryMouseGO.GetComponent<RectTransform>().position = Input.mousePosition;
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
    public GameObject temporaryMouseGO;
    public InventorySlot beginItemSlot;
    public InventorySlot hoverItemSlot;
    public GameObject hoverMouseGO;
}
