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

        }
    }
}
