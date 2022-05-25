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
- **Propagation** : Communication of the removal of one of the patterns throughout the whole grid which can result in further removals. This process continues until there are no more reductions to be made or a location within the output grid contains no patterns

## Usage


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
