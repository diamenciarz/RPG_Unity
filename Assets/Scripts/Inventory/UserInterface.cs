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
        EventManager.StartListening("Update Inventory Display", UpdateWholeDisplay);
        EventManager.StartListening("Update Item Display", UpdateDisplayItem);
        EventManager.StartListening("Assign Display GO's to Slots", AssignDisplayGameObjectsToSlots);
    }
    private void OnDisable()
    {
        EventManager.StopListening("Update Inventory Display", UpdateWholeDisplay);
        EventManager.StopListening("Update Item Display", UpdateDisplayItem);
        EventManager.StopListening("Assign Display GO's to Slots", AssignDisplayGameObjectsToSlots);
    }
    private void Start()
    {
        CreateSlots();
        AssignSlotsToMyArray();
        AssignParentUserInterfaceToEachSlot();

        UpdateWholeDisplay();

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
    }
    private void UpdateWholeDisplay()
    {
        //For every slot, update the display game object (square with a sprite and number)
        foreach (KeyValuePair<GameObject, InventorySlot> slot in displayedItemsDictionary)
        {
            UpdateDisplayItem(slot.Key);
        }
    }
    private void UpdateDisplayItem(object itemGameObject)
    {
        GameObject displayGameObject = (GameObject)itemGameObject;
        if (!displayedItemsDictionary.ContainsKey(displayGameObject))
        {
            return;
        }
        InventorySlot inventorySlot = displayedItemsDictionary[displayGameObject];

        Image spriteToModify = displayGameObject.transform.GetChild(0).GetComponentInChildren<Image>();
        TMP_Text textToModify = displayGameObject.GetComponentInChildren<TextMeshProUGUI>();
        //If the slot is not empty
        if (inventorySlot.amount > 0)
        {
            //Display the item
            spriteToModify.sprite = inventoryToDisplay.itemDatabase.getItemObjectDictionary[inventorySlot.item.itemID].itemSprite;
            textToModify.text = inventorySlot.amount == 1 ? "" : inventorySlot.amount.ToString("n0");
        }
        else
        {
            //Display an empty sprite
            spriteToModify.sprite = nullImage;
            textToModify.text = "";
        }
    }
    private void UpdateDisplayGameObject(GameObject displayGameObject, InventorySlot inventorySlot)
    {
        
    }
    public abstract void CreateSlots();
    public abstract void AssignDisplayGameObjectsToSlots();
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
        MouseData.beginDragItemSlot = displayedItemsDictionary[obj];
        MouseData.beginDragItemGO = obj;
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
            //Debug.Log("Item object: " + inventoryToDisplay.itemDatabase.getItemObjectDictionary[displayedItemsDictionary[obj].item.itemID]);
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
            MouseData.beginDragItemSlot.RemoveItem();
            return;
        }
        if (MouseData.hoverMouseGO)
        {
            inventoryToDisplay.TryToSwapItemsInSlots(MouseData.beginDragItemSlot, MouseData.hoverItemSlot);
        }
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

    public static GameObject beginDragItemGO;
    public static InventorySlot beginDragItemSlot;
    public static InventorySlot hoverItemSlot;
    public static GameObject hoverMouseGO;
}
