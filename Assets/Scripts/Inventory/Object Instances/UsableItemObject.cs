using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Usable Item", menuName = "Inventory System/Items/Usable Item")]
public class UsableItemObject : ItemObject
{
    public bool canUse;

    private void Awake()
    {
        itemType = ItemType.UsableItem;
    }
}
