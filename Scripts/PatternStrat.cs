using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PatternStrat 
{
    public PatternRecognition patternRec;
    public Dictionary<int, List<int[,,]>> edges = new Dictionary<int, List<int[,,]>>();
    public Dictionary<int, Dictionary<int, List<int>>> compatiblePatterns = new Dictionary<int, Dictionary<int, List<int>>>();
    public Dictionary<int, int[,,]> patterns;
    PatternHelper helper = new PatternHelper();
    public Transform patObj;

    
    //will take all the patterns and index their edges
    public void indexPatternsWithEdges(Dictionary<int, int[,,]> patterns)
    {
        foreach(KeyValuePair<int, int[,,]> currentPattern in patterns)
        {
            List<int[,,]> currentEdges = getEdges(currentPattern.Value);
            edges.Add(currentPattern.Key, currentEdges);
        }
    }
    public void initPatternRecognition(GameObject rootObject, bool allowEmpty, int patternSize)
    {
        patternRec = new PatternRecognition(rootObject, allowEmpty, patternSize);
    }

    //will extract which patterns are compatible with which and save them in our data structure
    public Dictionary<int, Dictionary<int, List<int>>> extractPatternsCompat(int patternSize)
    {
        Dictionary<int, int[,,]> patterns = patternRec.getPatternIndex(patternSize);
        this.patterns = patterns;
        indexPatternsWithEdges(patterns);
        List<int> compatiblePatternsList = new List<int>();
        Dictionary<int, List<int>> compatibilityDict = new Dictionary<int, List<int>>();
        foreach (KeyValuePair<int, List<int[,,]>> patternEdges1 in edges) //first pattern that we will compare
        {
            for (int i = 0; i < patternEdges1.Value.Count; i++) // each of the pattern's edges
            {
                foreach (KeyValuePair<int, List<int[,,]>> patternEdges2 in edges) // the pattern that we compare the first one to
                {
                    if (helper.comparePatterns(patternEdges1.Value[i], patternEdges2.Value[getOppositeEdge(i)], patternSize) && patternEdges1.Key != patternEdges2.Key)
                    {
                        compatiblePatternsList.Add(patternEdges2.Key);
                    }
                }
                compatibilityDict.Add(i, compatiblePatternsList);
                compatiblePatternsList = new List<int>();             
            }
            this.compatiblePatterns.Add(patternEdges1.Key, compatibilityDict);
            compatibilityDict = new Dictionary<int, List<int>>();
        }
        return this.compatiblePatterns;
    }

    //returns the opposite edge of a pattern as we need to compare them as such
    public int getOppositeEdge(int edgeIndex)
    {

        switch(edgeIndex)
        {
            case 0: //front (z+)
                return 1;
            case 1: //back  (z-)
                return 0;
            case 2: //left  (x+)
                return 3;
            case 3: //right (x-)
                return 2;
            case 4: //above (y+)
                return 5;
            case 5: //below (y-)
                return 4;
        }
        return -1;
    }
    

    //will get the edges of each pattern in the form of a 3D array and save it into a list where the position of each edge is equal to its
    //real-world position
    public List<int[,,]> getEdges(int[,,] pattern)
    {
        List<int[,,]> patternEdges = new List<int[,,]>();
        int length = (int)Math.Ceiling(Math.Pow(pattern.Length, (double)1 / 3));
        int edgeSize = length;
        for (int i = 0; i < 6; i++) //i = 6 because cubes have 6 faces, these faces are the edges of our cuboid shape
        {
            switch(i)
            {
                case 0:
                    //front edge
                    int[,,] frontEdge = new int[edgeSize, edgeSize, edgeSize];

                    for (int y = 0; y < length; y++)
                    {
                        for(int x = 0; x < length; x++)
                        {
                            frontEdge[x, y, 0] = pattern[x, y, 0];
                        }
                    }
                    patternEdges.Add(frontEdge);
                    break;
                
                case 1:
                    //back edge
                    int[,,] backEdge = new int[edgeSize, edgeSize, edgeSize];
                    for (int y = 0; y < length; y++)
                    {
                        for(int x = 0; x < length; x++)
                        {
                            //backEdge[x, y, length-1] = pattern[x, y, length-1];
                            backEdge[x, y, 0] = pattern[x, y, length-1];
                        }
                    }
                    patternEdges.Add(backEdge);
                    break;
                
                case 2:
                    //left edge
                    int[,,] leftEdge = new int[edgeSize, edgeSize, edgeSize];
                    for (int y = 0; y < length; y++)
                    {
                        for (int z = 0; z < length; z++)
                        {
                            leftEdge[0, y, z] = pattern[0, y, z];
                        }
                    }
                    patternEdges.Add(leftEdge);
                    break;

                case 3:
                    //right edge
                    int[,,] rightEdge = new int[edgeSize, edgeSize, edgeSize];
                    for (int y = 0; y < length; y++)
                    {
                        for (int z = 0; z < length; z++)
                        {
                            //rightEdge[length-1, y, z] = pattern[length-1, y, z];
                            rightEdge[0, y, z] = pattern[length-1, y, z];
                        }
                    }
                    patternEdges.Add(rightEdge);
                    break;
      
                case 4:
                    //above edge
                    int[,,] aboveEdge = new int[edgeSize, edgeSize, edgeSize];
                    for (int x = 0; x < length; x++)
                    {
                        for (int z = 0; z < length; z++)
                        {
                            //aboveEdge[x, length-1, z] = pattern[x, length-1, z];
                            aboveEdge[x, 0, z] = pattern[x, length-1, z];
                        }
                    }
                    patternEdges.Add(aboveEdge);
                    break;

                case 5:
                    //below edge
                    int[,,] belowEdge = new int[edgeSize, edgeSize, edgeSize];
                    for (int x = 0; x < length; x++)
                    {
                        for (int z = 0; z < length; z++)
                        {
                            belowEdge[x, 0, z] = pattern[x, 0, z];
                        }
                    }
                    patternEdges.Add(belowEdge);
                    break;
            }

        }
        return patternEdges;
    }  
    
    //gets the relative frequency of all the patterns and returns a dictionary with the index of the pattern and its frequency
    public Dictionary<int, float> getRelativeFrequency()
    {
        Dictionary<int, float> relativeFrequency = new Dictionary<int, float>();
        int sumOfPatterns = patternRec.patternFrequency.Values.Sum();
        foreach(KeyValuePair<int, int> currentPattern in patternRec.patternFrequency)
        {
            //double currentFreq = currentPattern.Value/sumOfPatterns;
            relativeFrequency.Add(currentPattern.Key, (float)currentPattern.Value / sumOfPatterns);
        }
        return relativeFrequency;
    }

    //clears everything
    public void ClearAll()
    {

    }
}
