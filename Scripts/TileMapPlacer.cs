using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapPlacer : MonoBehaviour
{
    
    public int tilemapLevels;
    //public GameObject[] prefabs;
    [System.NonSerialized]
    public int currentLevel = 0;
    public Transform root;

    public void AddLevel()
    {
        GameObject tileLevel = new GameObject("TilemapLv" + currentLevel);
        tileLevel.AddComponent<Tilemap>();

        tileLevel.transform.parent = gameObject.transform;
        tileLevel.transform.position = new Vector3(0, currentLevel, 0);
    }

    public int GetChildrenCount()
    {
        currentLevel = gameObject.transform.childCount;
        return currentLevel;
    }
}
