using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Create Food Item", menuName = "Inventory System/Items/Food")]
public class FoodItemObject : ItemObject
{
    public int duration;
    public float speedPercentIncrease;

    private void Awake()
    {
        itemType = ItemType.Food;
    }
}
