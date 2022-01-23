using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapColliderManager : MonoBehaviour
{
    public GameObject collisionMap;

    private void Start()
    {
        CreateCopy();
        gameObject.layer = 8;
    }

    private void CreateCopy()
    {
        collisionMap = Instantiate(this.gameObject, transform);
        TilemapCollider2D tilemap = collisionMap.GetComponent<TilemapCollider2D>();
        tilemap.usedByComposite = true;
        tilemap.gameObject.name = "Tilemap_Collision_Composite";
        tilemap.GetComponent<MapColliderManager>().enabled = false;
    }

}
