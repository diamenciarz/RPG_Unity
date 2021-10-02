using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAnimator : MonoBehaviour
{
    public Animator inventoryPanelAnimator;
    public Animator equipmentPanelAnimator;
    public bool isDisplayingInventory = false;

    //Private variables
    private Coroutine inventoryAnimationCoroutine = null;

    private void Start()
    {
        isDisplayingInventory = false;
        inventoryPanelAnimator.SetBool("isClosed", !isDisplayingInventory);
        equipmentPanelAnimator.SetBool("isClosed", !isDisplayingInventory);
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.I))
        {
            //Flip the boolean
            UpdateInventoryState(!isDisplayingInventory);
        }
    }

    public void UpdateInventoryState(bool isOpen)
    {
        isDisplayingInventory = isOpen;

        if (inventoryAnimationCoroutine == null)
        {
            inventoryAnimationCoroutine = StartCoroutine(PlayInventoryAnimation());
        }
    }

    public IEnumerator PlayInventoryAnimation()
    {
        inventoryPanelAnimator.SetBool("isClosed", !isDisplayingInventory);
        equipmentPanelAnimator.SetBool("isClosed", !isDisplayingInventory);
        yield return new WaitForSeconds(0.6f);
        
        bool shouldPlayNewAnimation = equipmentPanelAnimator.GetBool("isClosed") == isDisplayingInventory;
        if (shouldPlayNewAnimation)
        {
            inventoryAnimationCoroutine = StartCoroutine(PlayInventoryAnimation());
        }
        else
        {
            Debug.Log("Ready for a new animation");
            ClearInventoryAnimationCoroutine();
        }
    }
    private void ClearInventoryAnimationCoroutine()
    {
        inventoryAnimationCoroutine = null;
    }

}
