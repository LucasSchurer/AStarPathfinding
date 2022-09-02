# A* Pathfinding
An A* search algorithm implemented in Unity, using a graph and a visibility graph created from an image.

# Getting Started
In PathfindingScene, the Map GameObject controls the image used to generate the graph and the graph's size. All images used to the tests are from [movingai](https://movingai.com/benchmarks/street/index.html).
  
After setting an image, you can run the scene. You can manually find a path between two points using LMB for the start point and SHIFT+LMB for the goal point. When using the "full" graph overlay (which you can enable/disable by pressing 1), you can see the selected points highlighted in yellow.

![image](https://user-images.githubusercontent.com/39245000/188198717-cb039ee5-d19d-4ac7-8cbc-8bce707a06b6.png)

With two points selected, you can press F for the A* pathfinding using the full graph or SHIFT+F for the pathfinding on the visibility graph.

Full graph pathfinding:  
![image](https://user-images.githubusercontent.com/39245000/188198919-00ef34b7-5f5f-4acf-8e5a-fc66a7f3c1c2.png)

Visibility graph pathfinding:  
![image](https://user-images.githubusercontent.com/39245000/188199278-ad996366-1f18-48c1-9e97-60880113e822.png)

Output for full graph:
```
{
	Start: [29, 7] (673)
	Goal: [31, 32] (2048)  
	Reached Goal: True  
	Distance: 258  
	Open Set Size: 128  
	Closed Set Size A: 72  
	Elapsed Time A: 0ms  
}
```

Output for visibility graph:
```
{
	Start: [29, 7] (673)
	Goal: [31, 32] (2048)
	Reached Goal: True
	Distance: 258
	Open Set Size: 34
	Closed Set Size A: 4
	Elapsed Time A: 0ms
}
```

Alternatively, you can generate N numbers of points and perform a pathfinding on both graphs by pressing I. The number of points is also controlled on the Map GameObject (through the MapController component). The output will be placed by default on the Assets folder.

# References
The visibility graph implemented in this project is heavily based on Subgoal Graphs, presented in [this paper](http://www.gameaipro.com/GameAIPro2/GameAIPro2_Chapter15_Subgoal_Graphs_for_Fast_Optimal_Pathfinding.pdf).
