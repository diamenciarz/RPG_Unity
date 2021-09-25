using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class StaticInterface : UserInterface
{
    public GameObject[] slots;
    public override void CreateSlots()
    {
        displayedItemsDictionary = new Dictionary<GameObject, InventorySlot>();

        for (int i = 0; i < inventoryToDisplay.inventory.inventorySlotArray.Length; i++)
        {
            GameObject itemGameObject = slots[i];


            //Add event triggers to each slot
            AddEvent(itemGameObject, EventTriggerType.PointerEnter, delegate { OnEnter(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.PointerExit, delegate { OnExit(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.BeginDrag, delegate { OnBeginDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.EndDrag, delegate { OnEndDrag(itemGameObject); });
            AddEvent(itemGameObject, EventTriggerType.Drag, delegate { OnDrag(itemGameObject); });

            //Add each slot to the dictionary containing slots as values
            displayedItemsDictionary.Add(itemGameObject,inventoryToDisplay.inventory.inventorySlotArray[i]);
        }
        EventManager.TriggerEvent("Update Inventory Display");
    }
}
