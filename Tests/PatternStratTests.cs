using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PatternStratTests 
{
    [Test]
    public void gets_correct_edges_from_pattern()
    {
        PatternStrat stratTest = new PatternStrat();
        int[,,] testPattern = new int[2,2,2];
        //front, left and bottom edge should read 1 the rest should be 0 
        testPattern[0, 0, 0] = 1;
        int[,,] emptyEdge = new int[2, 2, 2];

        List<int[,,]> expectedEdges = new List<int[,,]>();
        expectedEdges.Add(testPattern); //front edge
        expectedEdges.Add(emptyEdge);
        expectedEdges.Add(testPattern); //left edge
        expectedEdges.Add(emptyEdge);
        expectedEdges.Add(emptyEdge);
        expectedEdges.Add(testPattern); //bottom edge

        List<int[,,]> edges = stratTest.getEdges(testPattern);

        Assert.AreEqual(expectedEdges, edges, "Edges do not match!");
    }

    [Test]
    public void indexes_the_edges_correctly_to_a_pattern()
    {
        PatternStrat stratTest = new PatternStrat();
        int[,,] testPattern1 = new int[2, 2, 2];
        //front, left and bottom edge should read 1 the rest should be 0 
        testPattern1[0, 0, 0] = 1;
        int[,,] testPattern2 = new int[2, 2, 2];

        List<int[,,]> expectedEdgesPat1 = new List<int[,,]>();
        expectedEdgesPat1.Add(testPattern1); //front edge
        expectedEdgesPat1.Add(testPattern2); //back edge
        expectedEdgesPat1.Add(testPattern1); //left edge
        expectedEdgesPat1.Add(testPattern2); //right edge
        expectedEdgesPat1.Add(testPattern2); //top edge
        expectedEdgesPat1.Add(testPattern1); //bottom edge

        List<int[,,]> expectedEdgesPat2 = new List<int[,,]>();
        expectedEdgesPat2.Add(testPattern2); 
        expectedEdgesPat2.Add(testPattern2); 
        expectedEdgesPat2.Add(testPattern2); 
        expectedEdgesPat2.Add(testPattern2); 
        expectedEdgesPat2.Add(testPattern2); 
        expectedEdgesPat2.Add(testPattern2);

        Dictionary<int, int[,,]> patternsDictionary = new Dictionary<int, int[,,]>();
        patternsDictionary.Add(0, testPattern1);
        patternsDictionary.Add(1, testPattern2);
        
        Dictionary<int, List<int[,,]>> expectedEdgesDictionary = new Dictionary<int, List<int[,,]>>();
        expectedEdgesDictionary.Add(0, expectedEdgesPat1);
        expectedEdgesDictionary.Add(1, expectedEdgesPat2);

        stratTest.indexPatternsWithEdges(patternsDictionary);
        Dictionary<int, List<int[,,]>> edgesDictionary = stratTest.edges;

        Assert.AreEqual(expectedEdgesDictionary, edgesDictionary, "The dictionaries are not equal");
    }

    public GameObject makeRootObject()
    {
        GameObject root = new GameObject();
        GameObject testTilemap1 = new GameObject();
        GameObject testTilemap2 = new GameObject();
        GameObject tile1 = new GameObject();
        GameObject tile2 = new GameObject();
        GameObject tile3 = new GameObject();
        GameObject tile4 = new GameObject();
        //GameObject tile5 = new GameObject();
        //GameObject tile6 = new GameObject();

        tile1.name = "Tile1";
        tile2.name = "Tile2";
        tile3.name = "Tile1";
        tile4.name = "Tile2";
        //tile5.name = "Tile5";
        //tile6.name = "Tile6";

        tile1.transform.position = new Vector3(0,0,0);
        tile2.transform.position = new Vector3(0,0,2);
        tile3.transform.position = new Vector3(2,0,0);
        tile4.transform.position = new Vector3(2,0,2);
        //tile5.transform.position = new Vector3(2,0,1);
        //tile6.transform.position = new Vector3(2,0,2);

        testTilemap1.transform.parent = root.transform;
        testTilemap2.transform.parent = root.transform;
        tile1.transform.parent = testTilemap1.transform;
        tile2.transform.parent = testTilemap1.transform;
        tile3.transform.parent = testTilemap1.transform;
        tile4.transform.parent = testTilemap1.transform;
        //tile5.transform.parent = testTilemap1.transform;
        //tile6.transform.parent = testTilemap1.transform;

        testTilemap1.AddComponent<Grid>();
        testTilemap1.GetComponentInChildren<Grid>().cellSize = new Vector3(1, 1, 1);

        return root;
    }

   

}
