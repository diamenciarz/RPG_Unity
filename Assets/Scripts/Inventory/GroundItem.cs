using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GroundItem : MonoBehaviour, ISerializationCallbackReceiver
{
    public ItemObject itemClass;
    SpriteRenderer mySpriteRenderer;

    public void OnAfterDeserialize()
    {

    }

    public void OnBeforeSerialize()
    {
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        mySpriteRenderer.sprite = itemClass.itemSprite;

        //EditorUtility.SetDirty(GetComponent<SpriteRenderer>());
    }
}
