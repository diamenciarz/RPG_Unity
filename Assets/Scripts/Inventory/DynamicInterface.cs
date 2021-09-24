using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DynamicInterface : UserInterface
{
    public GameObject inventoryPrefab;

    public int xOffsetOfItemSlots;
    public int yOffsetOfItemSlots;
    public int amountOfColumns;

    public int xDisplayStart;
    public int yDisplayStart;

    public override void CreateSlots()
    {
        
        displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            var itemGameObject = Instantiate(inventoryPrefab, Vector3.zero, Quaternion.identity, transform);
            itemGameObject.GetComponent<RectTransform>().localPosition = GetItemSlotPosition(i);
            Debug.Log("Created slot");
            displayedItemsDictionary.Add(itemGameObject, inventoryToDisplay.inventory.inventorySlotArray[i]);

            //Hook up each slot to unity events
            AddEvent(itemGameObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.PointerExit, delegate { OnExit(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.BeginDrag, delegate { OnBeginDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.EndDrag, delegate { OnEndDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.Drag, delegate { OnDrag(itemGameObject); });
        }
    }
    private Vector3 GetItemSlotPosition(int positionNumber)
    {
        return new Vector3(xDisplayStart + xOffsetOfItemSlots * (positionNumber % amountOfColumns), yDisplayStart + (-yOffsetOfItemSlots * (positionNumber / amountOfColumns)), 0f);
    }
}
