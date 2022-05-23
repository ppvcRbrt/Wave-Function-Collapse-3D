using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public class PatternRecognitionTests
{
   
    public GameObject makeRootObject()
    {
        GameObject root = new GameObject();
        GameObject testTilemap1 = new GameObject();
        GameObject testTilemap2 = new GameObject();

        GameObject tile1 = new GameObject();
        GameObject tile2 = new GameObject();
        GameObject tile3 = new GameObject();
        GameObject tile4 = new GameObject();
        GameObject tile5 = new GameObject();
        GameObject tile6 = new GameObject();
        GameObject tile7 = new GameObject();
        
        tile1.name = "Tile1";
        tile2.name = "Tile2";
        tile3.name = "Tile3";
        tile4.name = "Tile4";
        tile5.name = "Tile5";
        tile6.name = "Tile6";
        tile7.name = "Tile7";

        tile1.transform.position = new Vector3(2, 0, 3);
        tile2.transform.position = new Vector3(2, 0, 1);
        tile3.transform.position = new Vector3(0, 0, 3);
        tile4.transform.position = new Vector3(0, 0, 0);
        tile5.transform.position = new Vector3(0, 0, 1);
        tile6.transform.position = new Vector3(1, 0, 0);
        tile7.transform.position = new Vector3(1, 0, 1);

        testTilemap1.transform.parent = root.transform;
        testTilemap2.transform.parent = root.transform;

        tile1.transform.parent = testTilemap1.transform;
        tile2.transform.parent = testTilemap1.transform;
        tile3.transform.parent = testTilemap2.transform;
        tile4.transform.parent = testTilemap1.transform;
        tile5.transform.parent = testTilemap1.transform;
        tile6.transform.parent = testTilemap1.transform;
        tile7.transform.parent = testTilemap1.transform;


        testTilemap1.AddComponent<Grid>();
        testTilemap1.GetComponentInChildren<Grid>().cellSize = new Vector3(0.5f, 0.5f, 0.5f);

        return root;
    }

    [Test]
    public void returns_all_tilemaps()
    {
        //checks that it returns all the tilemaps with the children inside

        //setup of root object
        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);
        //expected tile no's
        int tilemapCount = 2;

        Transform[] tilemapsReturned = patternTest.getTilemaps();

        Assert.That(tilemapsReturned.Length == tilemapCount, "Not all tilemaps are returned");
    }

    [Test]
    public void creates_correct_measurements()
    {
        // will create a tuple that measures the width, height and length of the model based
        //on the minimum and maximum positions of the children inside the tilemaps

        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);

        Vector4 expectedMeasurements = new Vector4(2.5f, 1, 3.5f, 0.5f);

        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();

        String measurementsError = "Measurements are incorrect, expected : " + expectedMeasurements + "Got instead : " + measurements.Item1;
        Assert.That(measurements.Item1 == expectedMeasurements, measurementsError);
    }

    [Test]
    public void check_grid_size_is_correct()
    {
        //needs to check if we have correctly translated from model size to our grid size as we
        //need only whole numbers for the x, y and z positions that we will be using for the grid

        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();

        Vector3Int expectedSize = new Vector3Int(5, 2, 7);

        patternTest.createGrid(measurements);
        Vector3Int gridSize = patternTest.gridInfo;

        String gridSizeError = "Size of the grid is incorrect, expected: " + expectedSize + " got instead : " + gridSize;
        Assert.That(gridSize == expectedSize, gridSizeError);
    }

    [Test]
    public void check_that_all_tiles_are_added_to_dictionary()
    {
        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();
        patternTest.createGrid(measurements);

        Dictionary<string, int> expectedTilesAndValues = new Dictionary<string, int>();
        expectedTilesAndValues.Add("Tile1", 1);
        expectedTilesAndValues.Add("Tile2", 2);
        expectedTilesAndValues.Add("Tile4", 3);
        expectedTilesAndValues.Add("Tile5", 4);
        expectedTilesAndValues.Add("Tile6", 5);
        expectedTilesAndValues.Add("Tile7", 6);
        expectedTilesAndValues.Add("Tile3", 7);

        Dictionary<string, int> tilesAndValues = patternTest.tileValues;
        String keysError = "The dictionaries do not match";
        Assert.AreEqual(expectedTilesAndValues, tilesAndValues, keysError);
    }

    [Test]
    public void grid_correctly_represents_input()
    {
        //needs to check that the x, y, z grid that is created is a correct representation of our
        //input data

        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();
        patternTest.createGrid(measurements);

        int[,,] expectedGrid = new int[5, 2, 7];
        expectedGrid[3, 0, 5] = 1;
        expectedGrid[3, 0, 1] = 2;
        expectedGrid[0, 0, 0] = 3;
        expectedGrid[0, 0, 1] = 4;
        expectedGrid[1, 0, 0] = 5;
        expectedGrid[1, 0, 1] = 6;
        expectedGrid[0, 1, 5] = 7;

        int[,,] grid = patternTest.inputGrid;

        Assert.AreEqual(expectedGrid, grid, "The grids do not match");
    }

    [Test]
    public void get_the_correct_pattern()
    {
        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2);
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();
        patternTest.createGrid(measurements);

        Vector3Int startPosition = new Vector3Int(3, 0, 1);
        int[,,] expectedPattern = new int[2, 2, 2];
        expectedPattern[0, 0, 0] = 2;

        int[,,] pattern = patternTest.getOnePattern(2, startPosition);

        Assert.AreEqual(expectedPattern, pattern, "Patterns do not match");
    }

    [Test]
    public void gets_all_patterns_without_empty_tiles_at_bottom()
    {
        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, false, 2); //to simplify the index a bit we do not allow empty spaces
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();
        patternTest.createGrid(measurements);

        //we expect only one pattern since the rest have empty spaces
        int[,,] expectedPattern = new int[2, 2, 2];
        expectedPattern[0, 0, 0] = 3;
        expectedPattern[0, 0, 1] = 4;
        expectedPattern[1, 0, 0] = 5;
        expectedPattern[1, 0, 1] = 6;
        expectedPattern[0, 1, 0] = 0;
        List<int[,,]> expectedList = new List<int[,,]>();
        expectedList.Add(expectedPattern);
        
        List<int[,,]> patternList = patternTest.getPatterns(2);

        Assert.AreEqual(expectedList, patternList, "Lists do not match");
    }


    [Test]
    public void patterns_are_all_indexed_with_duplicates_removed()
    {
        GameObject root = makeRootObject();
        PatternRecognition patternTest = new PatternRecognition(root, true, 2); //to simplify the index a bit we do not allow empty spaces
        PatternHelper helper = new PatternHelper();
        Tuple<Vector4, Transform[]> measurements = patternTest.findMeasurments();
        patternTest.createGrid(measurements);

        List<int[,,]> patternList = patternTest.getPatterns(2);
        
        List<int> expectedPatternsIndices = new List<int>();
       

        for(int x = 0; x < 15; x++) // after removing the duplicates we should only have 15 indices
        {
            expectedPatternsIndices.Add(x);
        }

        patternTest.createPatternIndex(patternList);
        List<int> patternIndices = patternTest.patterns.Keys.ToList();

        String indicesError = "The count of indices does not match, expected : " + expectedPatternsIndices.Count + "but instead got : " + patternIndices.Count;
        Assert.That(expectedPatternsIndices.Count == patternIndices.Count, indicesError);
    }
}