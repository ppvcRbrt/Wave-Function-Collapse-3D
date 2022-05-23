using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternHelper 
{
    //returns true if two patterns are exactly the same
    public bool comparePatterns(int[,,] pattern1, int[,,] pattern2, int patternSize)
    {
        //int length = (int)Math.Ceiling(Math.Pow(pattern1.Length, (double)1 / 3));
        for (int x = 0; x < patternSize; x++)
        {
            for (int z = 0; z < patternSize; z++)
            {
                for (int y = 0; y < patternSize; y++)
                {
                    if (pattern1[x, y, z] != pattern2[x, y, z])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    //translates world position to grid position where each cell is 1 unit away from the other
    public Vector3Int getGridPositionV2(Vector3 worldPos, float step, int gridY)
    {
        float x = (worldPos.x - (step / 2)) / step;
        float z = (worldPos.z - (step / 2)) / step;

        return new Vector3Int(Convert.ToInt32(x), gridY, Convert.ToInt32(z));
    }


    //translates world position to grid position where each cell is 1 unit away from the other
    public Vector3Int getGridPosition(Vector3 worldPos, float step, int gridY)
    {
        float x = (worldPos.x + (step / 2)) / step - 1;
        float z = (worldPos.z + (step / 2)) / step - 1;

        return new Vector3Int((int)x, gridY, (int)z);
    }

    //checks if a pattern contains empty space 
    public bool containsEmpty(int[,,] pattern, int yPos)
    {
        int length = (int)Math.Ceiling(Math.Pow(pattern.Length, (double)1 / 3));
        for (int x = 0; x < length; x++)
        {
            for (int z = 0; z < length; z++)
            {
                for (int y = 0; y <= yPos; y++)
                {
                    int currentTile = pattern[x, y, z];
                    if (currentTile == 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    //will reflect a pattern along the x axis, will need to also rotate by 180 degrees to get more varied output
    public int[,,] reflectPattern(int[,,] pattern)
    {
        int patternSize = (int)Math.Ceiling(Math.Pow(pattern.Length, (double)1 / 3));
        int[,,] reflectedPattern = new int[patternSize, patternSize, patternSize];
        Vector3Int maxPos = new Vector3Int(patternSize - 1, patternSize - 1, patternSize - 1);

        for (int x = 0; x < patternSize; x++)
        {
            for (int y = 0; y < patternSize; y++)
            {
                for (int z = 0; z < patternSize; z++)
                {
                    Vector3Int currentPos = new Vector3Int(x, y, z);
                    Vector3Int reflectedPos = maxPos - currentPos;
                    reflectedPattern[reflectedPos.x, currentPos.y, currentPos.z] = pattern[x, y, z];
                }
            }
        }
        return reflectedPattern;
    }

    // god bless https://stackoverflow.com/questions/63876819/rotate-a-3d-array
    public int[,,] rotate90DegreesAroundY(int[,,] pattern)
    {
        var inputWidth = pattern.GetLength(0);
        var inputHeight = pattern.GetLength(1);
        var inputDepth = pattern.GetLength(1);

        // We swap the sizes because rotating a 3x4x5 yields a 4x3x5.
        var output = new int[inputHeight, inputWidth, inputDepth];

        var maxHeight = inputHeight - 1;
        var maxDepth = inputDepth - 1;
        var maxWidth = inputWidth - 1;

        for (int k = 0; k < inputDepth; k++)
        {
            for (int j = 0; j < output.GetLength(1); j++)
            {
                for (int i = 0; i < output.GetLength(0); i++)
                {
                    output[i, j, k] = pattern[maxWidth - k, j, i];
                }
            }
        }

        return output;
    }


}
