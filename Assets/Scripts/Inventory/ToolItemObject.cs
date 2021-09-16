using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Tool Item", menuName = "Inventory System/Items/Tool")]
public class ToolItemObject : ItemObject
{
    public int usesLeft;

    public void Awake()
    {
        itemType = ItemType.Tool;
        
    }
}
