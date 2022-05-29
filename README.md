# Wave-Function-Collapse-3D
An implementation of [Maxim Gumin's WFC algorithm](https://github.com/mxgmn/WaveFunctionCollapse), this was developed as part of my final year project and is accompanied by a [dissertation](https://github.com/ppvcRbrt/Wave-Function-Collapse-3D/blob/main/Dissertation.pdf) giving some more in-depth information on the development process as a whole.

This implementation was created as an extension of Unity to aid with
development of procedurally generated worlds as a general solution. The
solution manages to create varied and expansive 3D models given a much smaller
input training data by extracting constraints from the input and using a solver to
create the model given the constraints.

The current implementation does come
with some caveats. As it currently stands, the height of output model cannot be
changed, it is very dependent on the input data given, and it
cannot create structures that are meant to be logically connected (eg. a road
network) as the solution implements the overlapping model for finding constraints only (this can be extended to different constraint-finding models).

## Results
Below you can see some of the outputs of the algorithm given an input model.

##### Input Model
<img src="https://user-images.githubusercontent.com/73713049/170087778-ccbe7411-eed4-4b9f-a401-743b20c19cd0.png" width="600" height = "400">

##### Output Model
<img src="https://user-images.githubusercontent.com/73713049/170088266-1ddbbaf1-5b5e-4612-b17c-6bc53cbd03b9.png">

</br>
</br>

##### Input Model
<img src="https://user-images.githubusercontent.com/73713049/170088677-78e12e28-22c0-465c-8be4-c28ba1d0d83a.png" width="600" height = "400">

##### Output Model
<img src="https://user-images.githubusercontent.com/73713049/170089071-059da888-378b-475a-8c57-8ee426125b06.png">

## Terminology in this context

- **Pattern** &emsp; &emsp;: A 3D "slice" of the input in the form of an array containing discrete indices representing some of the tiles.  
- **Output Grid** : 3D grid of arrays. When initialized, it starts with all the patterns available included in each array at each location in the grid.
- **Observation** : The act of removing all patterns bar one from the array contained at a specific location in the output grid.
- **Propagation** : Communication of the removal of one of the patterns throughout the whole grid which can result in further removals. This process continues until there are no more reductions to be made or a location within the output grid contains no patterns.

## Setup
When bringing the project into Unity, there are some prerequisites that are needed before any procedural generation can be done;
- A folder named "Resources" needs to exists in the Unity's assets folder, this is where the models used for procedural generation are stored.
- In the scene, there needs to exist an empty gameobject specifically named "root". This will be where the input model is stored.
- The root object must have the "Core.cs" script as a component. The script contains all the options for the procedural generation.
- The root object can have the "DrawingArea.cs" script. This script helps the user draw only on the positive x and z axes.
- In the scene, there needs to exist an empty gameobject that will contain the drawn output.
- In the scene, you can include an empty gameobject for the drawing of the patterns, this is an optional step.
- Unity's 2D Tilemap Editor[1.0.0]
- Unity's 2D Tilemap Extras[2.0.0]

## Usage
### Drawing Input
To draw an input, Unity's Tilemap Editor was used in conjuction with the GameObject Brush.
After all the prerequisites have been met, navigate to Window > TilePlacer. From there There are 2 main options;
- **Tile Levels** : Refers to the height of the model.
- **Cell Size** : Refers to the width of a single cell in the Tilemap.
<p>
  After filling in the two options, you can either create all the levels specified by pressing "Create Tile Levels" or you can choose to add a single level with the    cell size specified. This will populate the "root" object created with empty Tilemaps.
</p>
<img src="https://user-images.githubusercontent.com/73713049/170870639-123509c9-3e7d-41ee-8796-6ba3e0cdf267.png">
<p>
  The next step is to setup our brush, in order to do that, navigate to Window > 2D > Tile Palette. Either from the Tile Palette window or from the Hierarchy view choose the Tilemap you would like to draw on[1]. In the middle left corner of the window choose the GameObject Brush[2]. Finally, drag and drop or choose the model you would like to use as your current tile in the GameObject field of the GameObject Brush[3].
</p>
<img src="https://user-images.githubusercontent.com/73713049/170871907-dc9cc938-f85b-48a1-81e5-40f99b40499c.png">
<p>
  The "Drawing Area" script we added in the setup step will serve as a guideline for where to draw, the main function of this is to show the position of the cell located at (0,0,0). The main reason for this is because the application does not support drawing on -ve axes. Otherwise you can start drawing on the tilemap using the GameObject brush.
</p>
<img src="https://user-images.githubusercontent.com/73713049/170872205-093264a5-b9bf-446f-a3a1-0ab77f65cc98.png">

### Procedural Generation
The "Core.cs" script that was attached to the root object contains a few fields that can control different aspects of the procedural generation, they are as follows;
- **Size** : The size of the output model in patterns.(Eg. If you set the size to 10 then the output model will be 10 patterns wide).
- **Height** : The height of the model in patterns. NOTE: This currently does not behave as expected on heights bigger than 1.
- **Pattern Size** : The width, length and height of a single pattern that is extracted. (eg. a pattern of size 2 would yield a cube that is 2x2x2).
- **Max Iterations** : The maximum ammount of iterations for the main algorithm loop.
- **Max Propagation Iterations** : The maximum ammount of times the algorithm can propagate information during a single loop.
- **Root Object** : The GameObject containing the input Tilemaps.
- **Output Object** : The empty GameObject that will contain the output.
- **Allow Reflections** : This will add reflected patterns to the input dataset.
- **Allow Rotations** : This will add rotated patterns to the input dataset. NOTE: This option increases the algorithm runtime by nearly 300%.
- **Allow Empty Patterns** : This will constrain the input patterns to patterns that do not contain any empty tiles at the bottom Y position.
- **Folder Name** : In the "Resources" folder you can have multiple folders containing different models, adding a folder name will target only the models inside that specific one. NOTE: The folder name must be followed by a "/".
- **Patterns Parent** : The empty GameObject that will contain all of our patterns if you decide to draw them. 
- **Draw Patterns** : Allows the algorithm to also draw all the patterns found.

## Solution Flowchart
![image](https://user-images.githubusercontent.com/73713049/170090681-ff7e8935-510f-4f5a-99e6-fad19120c1fc.png)

## Solution Breakdown
The solution is split into three distinct phases;

##### Pattern Recognition
During the first phase, the input model is translated into a 3D grid which is then "sliced" at each unique position to a user defined n x n x n grid which is then saved for further manipulation. During this phase, we can apply different constraints to our patterns(eg. only allow patterns that contain no empty tiles at the bottom, reflect or rotate the patterns etc...).

##### Constraint Extraction
Each pattern's edge is compared to another pattern's opposite edge. If they overlap, then we can say that the second pattern may reside next to the first pattern in that specific position. This gives us the constraints the solver will be working under. Below is an example of two patterns whose edges overlap.

<img src="https://user-images.githubusercontent.com/73713049/170134092-d4a1ef40-3fea-4269-9951-ec013fa7571e.png" width = "500">

##### Core Algorithm
The final phase of the solution is taking all the patterns and assigning them to every position in the output grid, the entropy of each cell is then calculated and stored. A cell is then observed which causes the solver to start propagating. This removes patterns that violate the constraints found in the last phase. Once all the patterns conform to the constraints, the entropy of the cells is recalculated and the lowest entropy cell is chosen for observation. This cycle continues until all cells contain only one pattern or until a cell contains 0 patterns in which case the process restarts with a fresh output grid.
