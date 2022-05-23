using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;


public class TileMapWindow : EditorWindow
{
    public int tilemapLevels;
    public int currentLevel = 0;
    public Transform root;
    public float cellSize;

    [MenuItem("Window/TilePlacer")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(TileMapWindow));
    }

    void OnGUI()
    {
        root = GameObject.Find("root").transform;
        tilemapLevels = EditorGUILayout.IntField("Tile Levels : ", tilemapLevels);
        cellSize = EditorGUILayout.FloatField("Cell Size : ", cellSize);
        if (GUILayout.Button("Add Single Level"))
        {
            AddLevel(root);
        }
        
        if (GUILayout.Button("Create Tile Levels"))
        {
            for (int i = 0; i < tilemapLevels; i++)
            {
                GetChildrenCount();
                AddLevel(root);
            }
        }
    }

    private void OnSelectionChange()
    {
        //Debug.Log(Selection.activeTransform.name);
        /**
        if(Selection.activeTransform)
        {
            string levelName = Selection.activeTransform.name;
            int height = int.Parse(new String(levelName.Where(Char.IsDigit).ToArray()));
            GameObjectBrushEditor brush = (GameObjectBrushEditor)GridPaintingState.activeBrushEditor;
            brush.brush.SetOffset(new Vector3Int(0, 0, 0), new Vector3(0, (float)(0.5 + height), 0));
        }
        */
    }
    public void OnSceneGUI()
    {
        Event e = Event.current;
        if (e != null && e.type == EventType.MouseDown && e.button == 0)
        {
            Debug.Log(GridPaintingState.activeBrushEditor);
            Debug.Log("Mouse down!");
        }

            //Debug.Log(tileMapPlacer.GetChildrenCount());
    }

    public bool CheckIfBrushActive()
    {
        
        GameObjectBrushEditor brush = (GameObjectBrushEditor)GridPaintingState.activeBrushEditor;
        
        if (brush.brush.GetType() == typeof(GameObjectBrush))
        {
            return true;
        }
        return false;
    }
    
    public void AddLevel(Transform root)
    {
        GameObject tileLevel = new GameObject("TilemapLv" + GetChildrenCount());
        tileLevel.AddComponent<Tilemap>();

        tileLevel.transform.parent = root;
        
        tileLevel.AddComponent<Grid>();
        tileLevel.GetComponent<Grid>().cellSwizzle = GridLayout.CellSwizzle.XZY;
        tileLevel.GetComponent<Grid>().cellSize = new Vector3(cellSize,cellSize,cellSize);
        tileLevel.transform.position = new Vector3(0, cellSize*currentLevel, 0);
    }

    public int GetChildrenCount()
    {
        currentLevel = root.childCount;
        return currentLevel;
    }

}
