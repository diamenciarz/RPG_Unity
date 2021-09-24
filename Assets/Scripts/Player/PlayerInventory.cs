using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    
    public InventoryObject playerInventoryObject;
    public InventoryObject playerEquipmentObject;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playerInventoryObject.SaveInventory();
            playerEquipmentObject.SaveInventory();
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            playerInventoryObject.LoadInventory();
            playerEquipmentObject.LoadInventory();
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Pickable Item")
        {
            var itemScript = collision.GetComponent<GroundItem>();

            if (itemScript)
            {
                playerInventoryObject.AddItemToInventory(new Item(itemScript.itemClass), 1);
                EventManager.TriggerEvent("Update Inventory Display");

                Destroy(collision.gameObject);
                
            }
        }
    }
    private void OnApplicationQuit()
    {
        playerInventoryObject.ClearInventory();
        playerEquipmentObject.ClearInventory();
    }
}
