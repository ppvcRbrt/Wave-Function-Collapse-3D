using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum Direction
{
    FORWARDS,
    BEHIND,
    ABOVE,
    BELOW,
    RIGHT,
    LEFT
}
public class PatternRecognition 
{
    private int tileSize;
    public Transform root;
    public int[,,] inputGrid;
    public Dictionary<String, int> tileValues = new Dictionary<String, int>();
    public Dictionary<int, int[,,]> patterns = new Dictionary<int, int[,,]>();
    public Tuple<Vector4, Transform[]> modelInfo;
    public List<int[,,]> pList;
    public Dictionary<int, int> patternFrequency = new Dictionary<int, int>();
    public Vector3Int gridInfo;
    PatternHelper helper = new PatternHelper();

    public bool allowRotations = false;
    public bool allowReflections = false;
    bool allowEmptyPatterns;
    int patternSize;
    bool patternIsEmpty = false;

    public PatternRecognition(GameObject rootObject, bool allowEmptyPatterns, int patternSize)
    {
        root = rootObject.transform;
        this.allowEmptyPatterns = allowEmptyPatterns;
        this.patternSize = patternSize;
    }

    //runs all the code needed to get all the patterns indexed and then returns them
    public Dictionary<int, int[,,]> getPatternIndex(int patternSize)
    {
        modelInfo = findMeasurments();
        createGrid(modelInfo);

        pList = getPatterns(patternSize);
        createPatternIndex(pList);
        return patterns;
    }

    //gets the physical cubes that are in the scene in the form of a transform that holds each level
    public Transform[] getTilemaps()
    {
        int mapCount = root.childCount;
        Transform[] tilemaps = new Transform[mapCount];
        for(int i = 0; i < mapCount; i++)
        {
            tilemaps[i] = root.GetChild(i);
        }
        return tilemaps;
    }

    //creates an x,y,z grid that represents the user input
    public void createGrid(Tuple<Vector4, Transform[]> modelInfo)
    {
        // (x + 1) * step - (step/2) -> (x+1) the +1 is to account that we start at 0 so in total we have 3 cells,
                                      //*step - (step/2) to get the local position while accounting for the width of the element
        Vector4 measurements = modelInfo.Item1;
        float x = (measurements.x / measurements.w);
        float y = (measurements.y / measurements.w);
        float z = (measurements.z / measurements.w);
        int currentValue = 1;
        gridInfo = new Vector3Int(Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z));
        
        //Debug.Log(measurements.x + " " + measurements.y + " " + measurements.z);
        //Debug.Log(measurements.w);
        //Debug.Log(x + " " + y + " " + z);
        inputGrid = new int[Convert.ToInt32(x), Convert.ToInt32(y), Convert.ToInt32(z)];
        int gridY = 0;
        foreach (Transform tilemap in modelInfo.Item2)
        {
            //List<Transform> tiles = tilemap.GetComponentsInChildren<Transform>().ToList();
            //tiles.RemoveAt(0);
            
            int tileChildCount = tilemap.childCount;
            List<Transform> tiles = new List<Transform>();
            for (int ID = 0; ID < tileChildCount; ID++)
            {
                tiles.Add(tilemap.GetChild(ID));
            }
            
            foreach (Transform tile in tiles)
            {
                Vector3 tilePosition = tile.position;
                if(!tileValues.ContainsKey(tile.name))
                {
                    tileValues.Add(tile.name, currentValue);
                    currentValue++;
                }
                int valueToAdd = tileValues[tile.name];
                Vector3Int gridPosition = helper.getGridPositionV2(tilePosition, measurements.w, gridY);
                inputGrid[gridPosition.x, gridPosition.y, gridPosition.z] = valueToAdd;
            }
            gridY++;
            
        }
    }

    
    //gets the size of the model and the world size of each cube
    public Tuple<Vector4, Transform[]> findMeasurments()
    {
        Transform[] tilemaps = getTilemaps();

        float[] minXA = new float[tilemaps.Length];
        float[] maxXA = new float[tilemaps.Length];
        float[] minZA = new float[tilemaps.Length];
        float[] maxZA = new float[tilemaps.Length];
        float step = tilemaps[0].GetComponent<Grid>().cellSize.x; //we extract the size of the sprites from the grid that is drawn on
        float y = 0;
        for(int i = 0; i < tilemaps.Length; i++)
        {
            int tileChildCount = tilemaps[i].childCount;
            Transform[] tiles = new Transform[tileChildCount];
            for(int ID = 0; ID < tileChildCount; ID++)
            {
                tiles[ID] = tilemaps[i].GetChild(ID);
            }
            
            //Transform[] transforms = tilemaps[i].GetComponentsInChildren<Transform>();
            //NOTE : need to copy transforms array from index 1 to another array as the first transform is always the root object,
            //       which we do not want
            //Transform[] tiles = new Transform[transforms.Length - 1];
            //System.Array.Copy(transforms, 1, tiles, 0, tiles.Length);

            if(tiles.Select(tile => tile).ToList().Count > 0)
            {
                minXA[i] = tiles.Select(tile => tile.localPosition.x).Min();
                maxXA[i] = tiles.Select(tile => tile.localPosition.x).Max();
                minZA[i] = tiles.Select(tile => tile.localPosition.z).Min();
                maxZA[i] = tiles.Select(tile => tile.localPosition.z).Max();
                y += step;
            }
        }
        float minX = Mathf.Abs(minXA.Min()) - (step/2);
        float maxX = Mathf.Abs(maxXA.Max()) + (step/2);
        float minZ = Mathf.Abs(minZA.Min()) - (step/2);
        float maxZ = Mathf.Abs(maxZA.Max()) + (step/2);

        float[] x = { minX, maxX };
        float[] z = { minZ, maxZ };

        x = x.OrderByDescending(x => x).ToArray();
        z = z.OrderByDescending(y => y).ToArray();
        
        //Debug.Log("Max X: " + x[0] + " " + "Min X: " + x[1]);
        //Debug.Log("Max Z: " + z[0] + " " + "Min Z: " + z[1]);

        //Debug.Log("MinX = " + minX + " " + "MaxX = " + maxX + " " + "minZ = " + minZ + " " + "MaxZ = " + maxZ);

        float width = x[0] - x[1]; 
        float length = z[0] - z[1];
        float height = y;
        //Debug.Log(width + " " + length + " " + height);
        return Tuple.Create(new Vector4(width, height, length, step), tilemaps);
    }

    //iterates over the grid and saves an n x n x n snapshot of that grid
    public List<int[,,]> getPatterns(int patternSize)
    {
        //Debug.Log(patternSize);
        List<int[,,]> patternList = new List<int[,,]>();
        
        for(int y = 0; y < gridInfo.y; y++)
        {
            if(y + patternSize <= gridInfo.y)
            {
                for (int x = 0; x < gridInfo.x; x++)
                {
                    for (int z = 0; z < gridInfo.z; z++)
                    {
                        int[,,] currentPattern = getOnePattern(patternSize, new Vector3Int(x, y, z));
                        
                        if(!allowEmptyPatterns)
                        {
                            if(!helper.containsEmpty(currentPattern, 0))
                            {
                                patternList.Add(currentPattern);

                                if (allowReflections)
                                {
                                    int[,,] reflectedPattern = helper.reflectPattern(currentPattern);
                                    reflectedPattern = helper.rotate90DegreesAroundY(reflectedPattern);
                                    reflectedPattern = helper.rotate90DegreesAroundY(reflectedPattern); //rotate twice for 180 degrees
                                    patternList.Add(reflectedPattern);
                                }
                                if (allowRotations)
                                {
                                    //get the rotated patterns as well
                                    for (int i = 0; i < 3; i++) //i < 3 because we can have 3 90 degree rotations until we reach the original position
                                    {
                                        currentPattern = helper.rotate90DegreesAroundY(currentPattern);
                                        patternList.Add(currentPattern);
                                    }
                                }

                            }
                            else
                            {
                                patternIsEmpty = false;
                            }
                        }
                        else
                        {
                            patternList.Add(currentPattern);

                            if (allowReflections)
                            {
                                int[,,] reflectedPattern = helper.reflectPattern(currentPattern);
                                reflectedPattern = helper.rotate90DegreesAroundY(reflectedPattern);
                                reflectedPattern = helper.rotate90DegreesAroundY(reflectedPattern); //rotate twice for 180 degrees
                                patternList.Add(reflectedPattern);
                            }
                            if (allowRotations)
                            {
                                //get the rotated patterns as well
                                for (int i = 0; i < 3; i++) //i < 3 because we can have 3 90 degree rotations until we reach the original position
                                {
                                    currentPattern = helper.rotate90DegreesAroundY(currentPattern);
                                    patternList.Add(currentPattern);
                                }
                            }
                        }
                        
                    }
                }
            }
        }
        return patternList;
    }

    //goes over the grid and saves the positions of the current indices to an n x n x n array that will serve as our "snapshot" of the
    // grid
    public int[,,] getOnePattern(int patternSize, Vector3Int startPos)
    {
        int[,,] pattern = new int[patternSize,patternSize,patternSize];
        int patternX = 0;
        int patternY = 0;
        int patternZ = 0;

        for (int x = startPos.x; x < startPos.x + patternSize; x++)
        {
            for(int z = startPos.z; z < startPos.z + patternSize; z++)
            {
                for(int y = startPos.y; y < startPos.y + patternSize; y++)
                {
                    if (doesExist(new Vector3Int(x, y, z)))
                    {
                        pattern[patternX, patternY, patternZ] = inputGrid[x, y, z];
                        patternY++;
                    }
                }
                patternZ++;
                patternY = 0;
            }
            patternX++;
            patternZ = 0;
        }
        return pattern;
    }

    
    //checks if a cell exists within our grid
    public bool doesExist(Vector3Int position)
    {
        Vector3Int maxPosition = new Vector3Int(gridInfo.x - 1, gridInfo.y - 1, gridInfo.z - 1);
        if(position.x > maxPosition.x || position.y > maxPosition.y || position.z > maxPosition.z)
        {
            return false;
        }
        return true;
    }

    
    //creates an index that holds all our paterns with a unique int pointing to each pattern, creates the frequency
    //list for all our patterns too
    public void createPatternIndex(List<int[,,]> patternList)
    {
        for (int i = 0; i < patternList.Count; i++)
        {
            if(patterns.Count == 0)
            {
                patterns.Add(i, patternList[i]);
                patternFrequency.Add(i, 1);
            }

            bool patternIndexed = false;

            foreach (KeyValuePair<int, int[,,]> currentPattern in patterns)
            {
                patternIndexed = helper.comparePatterns(patternList[i], currentPattern.Value, patternSize);
                if (patternIndexed)
                {
                    //add 1 to frequency list every time we see the same pattern
                    patternFrequency[currentPattern.Key]++;
                    break;
                }
            }
            if(!patternIndexed)
            {
                patterns.Add(i,patternList[i]);
                //if pattern is not indexed add it to the frequency list with a value of 1
                patternFrequency.Add(i, 1);
            }
        }
    }
}
