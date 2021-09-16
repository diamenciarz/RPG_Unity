using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public InventoryObject playerInventoryObject;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Pickable Item")
        {
            var itemScript = collision.GetComponent<Item>();

            if (itemScript)
            {
                playerInventoryObject.AddItemToInventory(itemScript.itemClass, 1);
                Destroy(collision.gameObject);
        }
    }
}
    private void OnApplicationQuit()
    {
        playerInventoryObject.ClearInventory();
    }
}
