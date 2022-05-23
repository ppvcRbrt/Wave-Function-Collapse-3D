using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingArea : MonoBehaviour
{
    public float drawingAreaSize;
    
    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawCube(new Vector3(drawingAreaSize/2,0,drawingAreaSize/2), new Vector3(drawingAreaSize, 0.3f, drawingAreaSize));

        if (transform.GetComponentInChildren<Grid>() != null)
        {
            float gridSize = transform.GetComponentInChildren<Grid>().cellSize.x;
            Gizmos.color = new Color(0, 1, 1, 0.9f);
            Gizmos.DrawCube(new Vector3(gridSize / 2, gridSize / 2, gridSize / 2), new Vector3(gridSize, gridSize, gridSize));
        }
        else
        {
            Debug.Log("Make sure you have a tile level in your root");
        }
    }

}
