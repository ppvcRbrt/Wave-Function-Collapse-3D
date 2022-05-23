using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class Core : MonoBehaviour
{
    public int size;
    public int height;
    public int patternSize;
    public int maxIterations;
    public int maxPropagationIterations;
    public GameObject rootObject;
    public GameObject outputObj;
    public GameObject rotatedOutput;
    bool[,,] state;
    int[,,][] compatible;
    float[,,] entropies;
    public bool allowReflections = false;
    public bool allowRotations = false;
    public bool highestFreqPattern = false;
    public bool allowEmptyPatterns = true;

    public int[] patterns;
    public string folderName;
    public bool drawPatterns = false;
    bool terminate = false;

    PatternStrat patternStrategy = new PatternStrat();
    
    public Dictionary<int, float> patternFrequency;
    public Dictionary<int, Dictionary<int, List<int>>> compatibilityIndex; 
    public Dictionary<int, Dictionary<int, List<int>>> patternsCompatibility; //this is extra
    private Queue<Vector3Int> propagationQueue = new Queue<Vector3Int>();

    public Transform patternsParent;

    //these values are to be used in testing performance of the algorithm
    public int algorithmRunCount;
    public string testName;
    int terminationCount = 0;
    int currentIteration = 0;
    int curretPropIteration = 0;
    float speed = 0.0f;
    Dictionary<int, Tuple<int, double, int, int>> performanceTests = new Dictionary<int, Tuple<int, double, int, int>>();
    
    public Core()
    {
        //wave = new bool[size, height, size];
        //compatibilityIndex = patternStrategy.extractPatternsCompat(rootObject);
    }

    //initiates the data array that will hold the current patterns available to each cell
    public void initCompatible()
    {
        compatible = new int[size, height, size][];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    compatible[x, y, z] = patterns;
                }
            }
        }
    }

    public void testAlgorithmPerformance()
    {
        performanceTests = new Dictionary<int, Tuple<int, double, int, int>>();
        for (int z = 0; z < algorithmRunCount; z++)
        {
            double startTimeTest = Time.realtimeSinceStartup;
            Init();
            run(maxIterations);
            double endTimeTest = Time.realtimeSinceStartup - startTimeTest;
            performanceTests.Add(z + 1, new Tuple<int, double, int, int>(terminationCount, endTimeTest, currentIteration, curretPropIteration));
            
            terminationCount = 0;
            currentIteration = 0;
            curretPropIteration = 0;
            patternStrategy = new PatternStrat();
            propagationQueue = new Queue<Vector3Int>();
            terminate = false;
        }

        //File.WriteAllLines("C:\\Users\\Robert\\Desktop\\Tests\\"+ testName +".csv", performanceTests.Select(s => s.ToString()));
        writeToFile();
    }

    public void writeToFile()
    {
        StringBuilder builder = new StringBuilder();
        foreach(Tuple<int,double,int,int> currentIterationInfo in performanceTests.Values)
        {
            builder.Append(currentIterationInfo.Item1).Append(",").Append(currentIterationInfo.Item2).Append(",").Append(currentIterationInfo.Item3).Append(",").Append(currentIterationInfo.Item4).Append("\n");
        }
        string info = builder.ToString();
        File.WriteAllText("C:\\Users\\Robert\\Desktop\\Tests\\" + testName + ".csv", info);
    }

    private void Start()
    {

        Init();
        run(maxIterations);
        Generator generator = new Generator(rootObject, allowEmptyPatterns, patternSize);
        generator.drawModel(compatible, patternStrategy.patterns, outputObj.transform, size, height, patternSize, folderName);
        outputObj.transform.position += new Vector3(15, 0, 0);

        if (drawPatterns)
        {
            generator.drawAllPatterns(patternStrategy.patterns, patternsParent, folderName, patternSize);
        }
        /*
        double startTime = Time.realtimeSinceStartup;
        
        
        
        patternStrategy.initPatternRecognition(rootObject, allowEmptyPatterns, patternSize);
        patternStrategy.patternRec.allowReflections = allowReflections;
        patternStrategy.patternRec.allowRotations = allowRotations;
        
        compatibilityIndex = patternStrategy.extractPatternsCompat(patternSize); //extracting the compatibility data structure
        Debug.Log("Compatible patterns extracted");

        patterns = patternStrategy.patterns.Select(x => x.Key).ToArray(); // for adding all the patterns in the available cells
         
        state = new bool[size, height, size];
        entropies = new float[size, height, size];
        initCompatible();
        Debug.Log("Initialized matrices");

        patternFrequency = patternStrategy.getRelativeFrequency();
        Debug.Log("Initialized frequency of patterns");

        patternsCompatibility = patternStrategy.compatiblePatterns;


        initializeCellsEntropy();
        Debug.Log("Initialized entropies");


        run(maxIterations);
        Debug.Log("Finished Running, drawing model");
        
        double endTime = Time.realtimeSinceStartup - startTime;
        Debug.Log("Algorithm finished building in : " + endTime);

        
        Generator generator = new Generator(rootObject, allowEmptyPatterns, patternSize);
        generator.drawModel(compatible, patternStrategy.patterns, outputObj.transform, size, height, patternSize, folderName);
        

        outputObj.transform.position += new Vector3(15, 0, 0);
        generator.drawAllPatterns(patternStrategy.patterns, patternsParent, folderName);
        patternsParent.transform.position -= new Vector3(50, 0, 0);
        */
        int i = 0;
    }

    //will check if any of the cells in the data array are empty
    public bool checkEmptyCompatible()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if(compatible[x,y,z].Length == 0)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }    

    //initiates all the needed arrays and data structures
    public void Init()
    {
        state = new bool[size, height, size];
        entropies = new float[size, height, size];

        patternStrategy.initPatternRecognition(rootObject, allowEmptyPatterns, patternSize);
        patternStrategy.patternRec.allowReflections = allowReflections;
        patternStrategy.patternRec.allowRotations = allowRotations;

        compatibilityIndex = patternStrategy.extractPatternsCompat(patternSize); //extracting the compatibility data structure
        Debug.Log("Compatible patterns extracted");

        patterns = patternStrategy.patterns.Select(x => x.Key).ToArray(); // for adding all the patterns in the available cells

        state = new bool[size, height, size];
        entropies = new float[size, height, size];
        initCompatible();
        Debug.Log("Initialized matrices");

        patternFrequency = patternStrategy.getRelativeFrequency();
        Debug.Log("Initialized frequency of patterns");

        patternsCompatibility = patternStrategy.compatiblePatterns;


        initializeCellsEntropy();
        Debug.Log("Initialized entropies");

    }

    //clears and reinitiates everything needed
    public void Clear()
    {
        state = new bool[size, height, size];
        entropies = new float[size, height, size];
        initCompatible();
    }

    //initializes the entropy for each cell
    public void initializeCellsEntropy()
    {
        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    //updateCellEntropy(new Vector3Int(x, y, z));
                    calculateEntropyV2(new Vector3Int(x, y, z));
                }
            }
        }
    }

    //checks how many patterns are in each cell and calculates their entropy [USE calculateEntropyV2 THIS IS DEPRECATED]  
    public void updateCellEntropy(Vector3Int cellPosition)
    {
        double entropy = 0;
        foreach(int currentPattern in compatible[cellPosition.x, cellPosition.y, cellPosition.z])
        {
            entropy -= patternFrequency[currentPattern] * Math.Log(patternFrequency[currentPattern], 2);
        }
        //entropy = Math.Round(entropy, 2);
        //entropy += UnityEngine.Random.Range(0.01f, 0.10f);
        entropies[cellPosition.x, cellPosition.y, cellPosition.z] = (float)entropy;
    }

    //removes from the data array a pattern, currently it creates a whole new array to replace the current one, this might be inefficient
    public void removeFromCompatible(Vector3Int cellPosition, int patternIndex)
    {
        List<int> patternsList = compatible[cellPosition.x, cellPosition.y, cellPosition.z].ToList();
        patternsList.Remove(patternIndex);
        compatible[cellPosition.x, cellPosition.y, cellPosition.z] = patternsList.ToArray();
    }


    //finds the position of the lowest entropy cell
    public Vector3Int lowestEntropyCellPos()
    {
        
        float min = 0;
        Vector3Int cellPosition = new Vector3Int(-1,-1,-1);

        for (int x = 0; x < size; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int z = 0; z < size; z++)
                {
                    float entropy = entropies[x, y, z];
                    if(min < entropies[x,y,z] && !state[x,y,z])
                    {
                        min = entropies[x, y, z];
                        cellPosition = new Vector3Int(x, y, z);
                    }
                }
            }
        }
        return cellPosition;
    }

    //checks the current state of the model and returns false if any of the cells have not been fully collapsed
    public bool checkWave()
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (!state[x, y, z])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    //runs the loop that our algorithm needs
    public void run(int maxIterations)
    {
        int i = 0;
        while(!checkWave() && i < maxIterations)
        {
            Vector3Int lowEntropyCellPos = lowestEntropyCellPos();
            if (lowEntropyCellPos.x == -1)
            {
                lowEntropyCellPos = new Vector3Int(0, 0, 0);
            }
            if(patternStrategy.patterns.Count == 0)
            {
                Debug.Log("You have no patterns! Try using a lower pattern size");
                break;
            }
            observeCell(lowEntropyCellPos);
            propagate(maxPropagationIterations);
            //propagateAllGrid();
            if (checkEmptyCompatible())
            {
                Clear();
            }
            i++;
            currentIteration++;
            Debug.Log("Running... Iteration : " + i);
        }
    }

    //will collapse a cell to one pattern given the cells position
    public void observeCell(Vector3Int cellPosition)
    {
        //Random choice of pattern might not be good, let's try choosing by frequency
        if (!highestFreqPattern)
        {
            int chosenPatternIndex = UnityEngine.Random.Range(0, compatible[cellPosition.x, cellPosition.y, cellPosition.z].Length - 1);
            List<int> patternsList = compatible[cellPosition.x, cellPosition.y, cellPosition.z].ToList();
            int[] chosenPattern = { patternsList[chosenPatternIndex] };
            compatible[cellPosition.x, cellPosition.y, cellPosition.z] = chosenPattern;
        }
        else
        {
            int[] chosenPattern = { choosePatternHighFreq(cellPosition) };
            compatible[cellPosition.x, cellPosition.y, cellPosition.z] = chosenPattern;
        }
        //Generator generator = new Generator(rootObject);
        //generator.visualizeCurrentTilePlacement(new Vector3Int(cellPosition.x, cellPosition.y, cellPosition.z), patternStrategy.patterns ,outputObj, patternSize, chosenPattern[0]);
        banCell(cellPosition);
    }

    //gets the pattern with the highest frequency out of a cell
    public int choosePatternHighFreq(Vector3Int cellPosition)
    {
        int[] patterns = compatible[cellPosition.x, cellPosition.y, cellPosition.z];
        float max = 0;
        int arrKey = 0;
        for(int i = 0; i < patterns.Length; i++)
        {
            if(max < patternFrequency[patterns[i]])
            {
                max = patternFrequency[patterns[i]];
                arrKey = i;
            }
        }
        int chosenPattern = patterns[arrKey];
        return chosenPattern;
    }

    //will set the state of the cell as true, meaning that it is fully collapsed and enqueues it into our propagation queue 
    public void banCell(Vector3Int cellPosition)
    {
        state[cellPosition.x, cellPosition.y, cellPosition.z] = true;
        if (cellExists(cellPosition) && !containsVector(cellPosition))
        {
            propagationQueue.Enqueue(cellPosition);
        }
    }

    //will propagate information throughout all of our data grid
    public void propagateAllGrid()
    {
        for(int x = 0; x < size; x++)
        {
            for(int y = 0; y < height; y++)
            {
                for(int z = 0; z < size; z++)
                {
                    removeFromNeighbour(new Vector3Int(x, y, z));
                }
            }
        }
    }

    //propagates information only to the cells that need propagating, i.e. any cells that have had their pattern list modified and their neighbours
    public void propagate(int maxIteration)
    {
        int i = 0;
        while(propagationQueue.Count > 0 && i < maxIteration && !terminate)
        {
            Vector3Int cellPosition = propagationQueue.Dequeue();
            
            removeFromNeighbourWPropagation(cellPosition);
            Debug.Log("Propagating... Iteration : " + i);
            curretPropIteration++;
            i++;
        }
    }

    //given the main cell position, will remove any illegal patterns from the cells neighbours
    public void removeFromNeighbour(Vector3Int cellPosition)
    {
        Vector3Int currentCellPosition = cellPosition;
        for (int i = 0; i < 6; i++)
        {
            Vector3Int neighbourCell = getNeighbouringCell(cellPosition, i);
            if (cellExists(neighbourCell) && !state[neighbourCell.x, neighbourCell.y, neighbourCell.z])
            {
                int currentDir = i;
                int[] currentPatterns = compatible[cellPosition.x, cellPosition.y, cellPosition.z];
                int[] neighbourPatterns = compatible[neighbourCell.x, neighbourCell.y, neighbourCell.z];
                List<int> allowedPatterns = new List<int>();

                foreach (int currentPattern in currentPatterns)
                {
                    foreach (int comparingPattern in neighbourPatterns)
                    {
                        if (patternsCompatibility[currentPattern][i].Contains(comparingPattern))
                        {
                            if (!allowedPatterns.Contains(comparingPattern))
                            {
                                allowedPatterns.Add(comparingPattern);
                            }
                        }
                    }
                }
                if (allowedPatterns.Count == 1)
                {
                    banCell(neighbourCell);
                }
                
                compatible[neighbourCell.x, neighbourCell.y, neighbourCell.z] = allowedPatterns.ToArray();
                calculateEntropyV2(neighbourCell);
                //updateCellEntropy(neighbourCell);
            }
        }
    }

    //given the main cell position, will remove any illegal patterns from the cells neighbours,
    //and enqueue them into our propagation queue given that the neighbours pattern list has been modified
    public void removeFromNeighbourWPropagation(Vector3Int cellPosition)
    {
        //Vector3Int currentCellPosition = cellPosition;
        int[] currentPatterns;
        int[] neighbourPatterns;
        for (int i = 0; i < 6; i++)
        {
            Vector3Int neighbourCell = getNeighbouringCell(cellPosition, i);
            if (cellExists(neighbourCell) && !state[neighbourCell.x, neighbourCell.y, neighbourCell.z])
            {
                //int currentDir = i;
                currentPatterns = compatible[cellPosition.x, cellPosition.y, cellPosition.z];
                neighbourPatterns = compatible[neighbourCell.x, neighbourCell.y, neighbourCell.z];
                
                List<int> allowedPatterns = new List<int>();
                foreach (int currentPattern in currentPatterns)
                {
                    foreach (int comparingPattern in neighbourPatterns)
                    {
                        if (patternsCompatibility[currentPattern][i].Contains(comparingPattern))
                        {
                            if (!allowedPatterns.Contains(comparingPattern))
                            {
                                allowedPatterns.Add(comparingPattern);
                            }
                        }
                    }
                }
                if (allowedPatterns.Count == 0)
                {
                    terminate = true;
                    terminationCount++;
                    break;
                }
                if (allowedPatterns.Count == 1)
                {
                    banCell(neighbourCell);
                }
                compatible[neighbourCell.x, neighbourCell.y, neighbourCell.z] = allowedPatterns.ToArray();
                if(allowedPatterns.Count < neighbourPatterns.Length)
                {
                    propagationQueue.Enqueue(new Vector3Int(neighbourCell.x, neighbourCell.y, neighbourCell.z));
                    calculateEntropyV2(neighbourCell);
                    //updateCellEntropy(neighbourCell);
                }
            }
        }
    }

    //will check if a cell exists within our model
    public bool cellExists(Vector3Int cellPosition)
    {
        if(cellPosition.x < 0 || cellPosition.x >= size || cellPosition.y < 0 || cellPosition.y >= height || cellPosition.z < 0 || cellPosition.z >= size)
        {
            return false;
        }
        return true;
    }

    //
    public bool containsVector(Vector3Int position)
    {
        foreach(Vector3Int currentVector in propagationQueue)
        {
            if(position == currentVector)
            {
                return true;
            }
        }    
        return false;
    }

    //gets neighbouring cells given a position and direction 
    //NOTE:: directions are as follows, 0 = forwards, 1 = backwards, 2 = right, 3 = left, 4 = below, 5 = above
    public Vector3Int getNeighbouringCell(Vector3Int cellPosition, int dir)
    {
        Vector3Int position = cellPosition;
        switch (dir)
        {
            case 0:
                position.z += 1;
                break;
            case 1:
                position.z -= 1;
                break;
            case 2:
                position.x += 1;
                break;
            case 3:
                position.x -= 1;
                break;
            case 4:
                position.y -= 1;
                break;
            case 5:
                position.y += 1;
                break;
        }

        return position;
    }

    public void calculateEntropyV2(Vector3Int cellPosition)
    {
        double entropy = 0;
        foreach (int currentPattern in compatible[cellPosition.x, cellPosition.y, cellPosition.z])
        {
            entropy += patternFrequency[currentPattern] * Math.Log(1/patternFrequency[currentPattern], 2);
            //entropy -= patternFrequency[currentPattern] * Math.Log(patternFrequency[currentPattern], 2);
        }
        entropies[cellPosition.x, cellPosition.y, cellPosition.z] = (float)entropy;
    }
}
