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
    public InventorySlot[] slotsOnInterface;

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
        AssignSlotsToMyArray();
        AssignParentUserInterfaceToEachSlot();

        UpdateDisplay();

        AddEvent(gameObject, EventTriggerType.PointerEnter, delegate { OnEnterInterface(gameObject); });
        AddEvent(gameObject, EventTriggerType.PointerExit, delegate { OnExitInterface(gameObject); });
    }
    private void AssignSlotsToMyArray()
    {
        slotsOnInterface = new InventorySlot[inventoryToDisplay.inventory.inventorySlotArray.Length];
        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            slotsOnInterface[i] = inventoryToDisplay.inventory.inventorySlotArray[i];
        }
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
    //Slot events
    protected void OnEnter(GameObject obj)
    {
        MouseData.hoverMouseGO = obj;
        if (displayedItemsDictionary.ContainsKey(obj))
        {
            MouseData.hoverItemSlot = displayedItemsDictionary[obj];
        }
    }
    protected void OnExit(GameObject obj)
    {
        MouseData.hoverMouseGO = null;
        MouseData.hoverItemSlot = null;
    }
    protected void OnBeginDrag(GameObject obj)
    {
        GameObject temporaryMouseObject = CreateTemporaryItemObject(obj);

        MouseData.temporaryMouseGO = temporaryMouseObject;
        MouseData.beginItemSlot = displayedItemsDictionary[obj];
    }
    private GameObject CreateTemporaryItemObject(GameObject obj)
    {
        GameObject temporaryMouseObject = null;

        if (displayedItemsDictionary[obj].item.itemID >= 0)
        {
            temporaryMouseObject = new GameObject();
            RectTransform rectTransform = temporaryMouseObject.AddComponent<RectTransform>();

            //Make sure the size of the created clone item is the same as the size of the original itemSlot
            Vector2 itemSize = obj.GetComponent<RectTransform>().sizeDelta;
            rectTransform.sizeDelta = itemSize;
            temporaryMouseObject.transform.SetParent(transform.parent);

            Image image = temporaryMouseObject.AddComponent<Image>();
            image.sprite = inventoryToDisplay.itemDatabase.getItemObjectDictionary[displayedItemsDictionary[obj].item.itemID].itemSprite;
            image.raycastTarget = false;
        }
        return temporaryMouseObject;
    }
    protected void OnEndDrag(GameObject obj)
    {
        //Cleanup
        Destroy(MouseData.temporaryMouseGO);

        if (MouseData.hoverUI == null)
        {
            MouseData.beginItemSlot.RemoveItemFromSlot();
            EventManager.TriggerEvent("Update Inventory Display");

            return;
        }
        if (MouseData.hoverMouseGO)
        {
            inventoryToDisplay.TryToSwapItemsInSlots(MouseData.beginItemSlot, MouseData.hoverItemSlot);
            EventManager.TriggerEvent("Update Inventory Display");
        }
        /*
        //Can not swap an empty slot onto an item
        if (MouseData.beginItemSlot != null)
        {
            GameObject mouseHoverObj = MouseData.hoverMouseGO;
            InventorySlot mouseHoverSlot = MouseData.hoverItemSlot;

            Dictionary<int, ItemObject> getItemObjectFromDictionary = inventoryToDisplay.itemDatabase.getItemObjectDictionary;
            //If dropped item on any item slot Game Object or UI
            if (MouseData.hoverUI != null)
            {
                //If cursor is hovering over another slot
                //If the item in hand can be moved onto the slot that the cursor is hovering over
                if (mouseHoverSlot != null && mouseHoverSlot.CanPlaceItemInSlot(getItemObjectFromDictionary[displayedItemsDictionary[obj].item.itemID]))
                {
                    //Then if the item, which the cursor is hovering over, can be moved onto the slot, which the cursor started dragging from
                    //Or if the slot, which we are moving the item into, has no item
                    if (mouseHoverSlot.item.itemID == -1 || MouseData.beginItemSlot.CanPlaceItemInSlot(getItemObjectFromDictionary[mouseHoverSlot.item.itemID]))
                    {
                        inventoryToDisplay.TrySwapItemsInSlots(MouseData.beginItemSlot, MouseData.hoverItemSlot);
                    }
                    //Else can not swap the item
                }
                //Else, can not swap the item, so don't
            }
            else
            {
                //If not, then delete the item
                inventoryToDisplay.DeleteItemFromSlot(MouseData.beginItemSlot);
            }
            MouseData.beginItemSlot = null;
        }*/
    }
    protected void OnDrag(GameObject obj)
    {
        if (MouseData.temporaryMouseGO != null)
        {
            MouseData.temporaryMouseGO.GetComponent<RectTransform>().position = Input.mousePosition;
        }
    }
    //Interface events
    protected void OnEnterInterface(GameObject obj)
    {
        MouseData.hoverUI = obj.GetComponent<UserInterface>();
    }
    protected void OnExitInterface(GameObject obj)
    {
        MouseData.hoverUI = null;
        
    }
    //Custom event method. The events are triggered by unity UI elements
    protected void AddEvent(GameObject obj, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        var eventTrigger = new EventTrigger.Entry();
        eventTrigger.eventID = type;
        eventTrigger.callback.AddListener(action);
        trigger.triggers.Add(eventTrigger);
    }
}
public static class MouseData
{
    public static UserInterface hoverUI;

    public static GameObject temporaryMouseGO;
    public static InventorySlot beginItemSlot;
    public static InventorySlot hoverItemSlot;
    public static GameObject hoverMouseGO;
}
