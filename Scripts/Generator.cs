using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Generator : MonoBehaviour
{
    PatternRecognition patternRec;
    Dictionary<int, int[,,]> patterns;
    Dictionary<String, int> tileValues;
    float cubeSize;

    //we will need the root object that contains our physical tiles in order to extract the pattern indices and the values that they represent
    public Generator(GameObject rootObject, bool allowEmpty, int patternSize)
    {
        patternRec = new PatternRecognition(rootObject, allowEmpty, patternSize);
    }

    //method that will load a prefab from our "Resources" folder, in order to redraw any prefabs that we have used they must be in there
    private UnityEngine.Object LoadPrefabFromFile(string filename)
    {
        //Debug.Log("Trying to load prefab from file (" + filename + ")...");
        var loadedObject = Resources.Load(filename);
        if (loadedObject == null)
        {
            throw new FileNotFoundException("...no file found by the name " + filename + "- please check the configuration");
        }
        return loadedObject;
    }

    //will take all the patterns from our dictionary and draw them, this is a testing method
    public void drawAllPatterns(Dictionary<int, int[,,]> patterns, Transform parentTrans, string folderName, int patternSize)
    {

        int offsetZ = 0;
        int offsetX = -30;
        int i = 0;
        foreach (KeyValuePair<int, int[,,]> pattern in patterns)
        {
            GameObject patternObj = new GameObject("Pattern : " + i);
            patternObj.transform.parent = parentTrans;
            patternObj.transform.position = new Vector3(offsetX, 0, offsetZ);
            drawTilemap(pattern.Value, patternObj, folderName);
            if(offsetZ > 40)
            {
                offsetX += 1 + patternSize;
                offsetZ = 0;
            }
            offsetZ += 2 * patternSize;
            i++;
        }

    }


    //will draw our final model given the data array that contains only one pattern, if it contains more than one it will throw out an error
    public void drawModel(int[,,][] compatible, Dictionary<int, int[,,]> patterns, Transform parentTrans, int width, int height, int patternSize, string folderName)
    {
        Vector3 position = new Vector3(0, 0, 0);
        patternRec.getPatternIndex(patternSize);
        cubeSize = patternRec.modelInfo.Item1.w;
        tileValues = patternRec.tileValues;

        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int z = 0; z < width; z++)
                {
                    if(compatible[x,y,z].Length > 0 && compatible[x,y,z].Length < 2)
                    {
                        int patternToDraw = compatible[x, y, z][0];
                        GameObject patternObj = new GameObject("Pattern : " + patternToDraw);
                        patternObj.transform.parent = parentTrans;
                        position = new Vector3(cubeSize * x * patternSize - cubeSize, cubeSize * y * patternSize - cubeSize, cubeSize * z * patternSize - cubeSize);
                        drawTilemap(patterns[patternToDraw], patternObj, position, cubeSize, folderName);
                    }
                    else
                    {
                        Debug.Log("You have too many patterns in one cell to draw, check that the data array only contains 1 pattern");
                    }
                }
            }
        }
    }

    public void drawModelTest(int[,,][]compatbile, Dictionary<int, int[,,]> patterns, GameObject parent, int patternSize, string folderName)
    {
        patternRec.getPatternIndex(patternSize);
        cubeSize = patternRec.modelInfo.Item1.w;
        tileValues = patternRec.tileValues;
        int patternToDraw = compatbile[0, 0, 0][0];
        drawTilemap(patterns[patternToDraw], parent, folderName);
    }

    //will draw the left, right, above, below, forwards and backwards edges for a given pattern, this is a testing method
    public void drawEdges(List<int[,,]> edges, Transform parentTrans, int patternSize, string folderName)
    {
        patternRec.getPatternIndex(patternSize);
        cubeSize = patternRec.modelInfo.Item1.w;
        tileValues = patternRec.tileValues;
        
        int offset = 0;
        foreach (int[,,] pattern in edges)
        {
            GameObject patternObj = new GameObject("Pattern : " + offset);
            patternObj.transform.parent = parentTrans;
            patternObj.transform.position = new Vector3(-1, 0, offset);
            drawTilemap(pattern, patternObj, folderName);
            offset++;
        }


    }

    //will draw a singluar pattern, this is a testing method
    public void drawTilemap(int[,,] inputGrid, GameObject patternObj, string folderName)
    {
        Vector3 currentWorldPosition;
        patternRec.getPatternIndex(3);
        cubeSize = patternRec.modelInfo.Item1.w;
        tileValues = patternRec.tileValues;

        int patternSize = (int)Math.Ceiling(Math.Pow(inputGrid.Length, (double)1 / 3));
        for (int x = 0; x < patternSize; x++)
        {
            for(int y = 0; y < patternSize; y++)
            {
                for(int z = 0; z < patternSize; z++)
                {
                    currentWorldPosition = new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);
                    int prefabIndex = inputGrid[x, y, z];
                    if (prefabIndex != 0) 
                    {
                        string prefabName = tileValues.FirstOrDefault(pName => pName.Value == prefabIndex).Key;
                        var currentPrefab = LoadPrefabFromFile(folderName + prefabName);

                        GameObject currentCube = (GameObject)Instantiate(currentPrefab);

                        currentCube.transform.parent = patternObj.transform;
                        currentCube.transform.localPosition = currentWorldPosition;
                    }

                }
            }
        }
    }

    //will draw a singular pattern with an offset inside a transform for ease of debugging
    public void drawTilemap(int[,,] inputGrid, GameObject patternObj, Vector3 offset, float cubeSize, string folderName)
    {
        //hacky way of adding the empty to tilevalues
        if(tileValues.ContainsKey("Empty"))
        {
            tileValues.Add("Empty", 0);
        }

        Vector3 currentWorldPosition;
        int patternSize = (int)Math.Ceiling(Math.Pow(inputGrid.Length, (double)1 / 3));
        for (int x = 0; x < patternSize; x++)
        {
            for (int y = 0; y < patternSize; y++)
            {
                for (int z = 0; z < patternSize; z++)
                {
                    currentWorldPosition = new Vector3(x * cubeSize, y * cubeSize, z * cubeSize);
                    
                    int prefabIndex = inputGrid[x, y, z];
                    if (prefabIndex != 0) 
                    {
                        string prefabName = tileValues.FirstOrDefault(pName => pName.Value == prefabIndex).Key;
                        var currentPrefab = LoadPrefabFromFile(folderName + prefabName);

                        GameObject currentCube = (GameObject)Instantiate(currentPrefab);
                        currentCube.transform.localPosition = currentWorldPosition;

                        currentCube.transform.parent = patternObj.transform;
                    }
                    /* will create a cube in place of an empty for an easier time when debugging
                    else
                    {
                        GameObject currentCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        currentCube.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                        currentCube.transform.localPosition = currentWorldPosition;

                        currentCube.transform.parent = patternObj.transform;

                    }
                    */
                }
            }
        }
        patternObj.transform.localPosition += offset;
    }

    public void visualizeCurrentTilePlacement(Vector3Int currentPosition, Dictionary<int, int[,,]> patterns, GameObject parentTransform, int patternSize, int patternToDraw)
    {
        patternRec.getPatternIndex(patternSize);
        cubeSize = patternRec.modelInfo.Item1.w;
        tileValues = patternRec.tileValues;
        GameObject patternObj = new GameObject("Pattern : " + patternToDraw);
        patternObj.transform.parent = parentTransform.transform;
        //drawTilemap(patterns[patternToDraw], patternObj, currentPosition, cubeSize);

    }

    public void clear(GameObject parentTransform)
    {
        foreach (Transform child in parentTransform.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }
}
