using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject[] inventoryToSaveList;
    public InventoryObject playerInventoryObject;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (InventoryObject inventory in inventoryToSaveList)
            {
                inventory.SaveInventory();
            }
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (InventoryObject inventory in inventoryToSaveList)
            {
                inventory.LoadInventory();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Pickable Item")
        {
            var itemScript = collision.GetComponent<GroundItem>();

            if (itemScript)
            {
                playerInventoryObject.AddItemToInventory(new ItemDataForSlots(itemScript.itemClass), 1);
                //EventManager.TriggerEvent("Update Inventory Display");

                Destroy(collision.gameObject);
                
            }
        }
    }
    private void OnApplicationQuit()
    {
        foreach (InventoryObject inventory in inventoryToSaveList)
        {
            inventory.ClearInventory();
        }
    }
}
