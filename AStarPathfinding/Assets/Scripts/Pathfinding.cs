using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding
{
    public delegate void OnPathProcessed(Vertex[] steps);
    public OnPathProcessed onPathProcessed;

    private Graph _graph;

    public Pathfinding(Graph graph)
    {
        _graph = graph;
    }

    /// <summary>
    /// A* Algorithm Implementation to find a path between two vertices
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public IEnumerator FindPath(Vertex source, Vertex target)
    {
        bool reachedGoal = false;
        Stopwatch sw = new Stopwatch();
        sw.Start();

        HashSet<Vertex> closedSet = new HashSet<Vertex>();
        Heap<Vertex> openSet = new Heap<Vertex>(_graph.VerticesCount);
        Vertex[] steps = new Vertex[] { source };
        Vertex currentVertex;

        source.hCost = DistanceBetweenVertices(source, target);
        source.gCost = 0;
        source.parent = null;
        openSet.Add(source);

        while (openSet.Count > 0)
        {
            currentVertex = openSet.RemoveFirst();
            closedSet.Add(currentVertex);

            if (currentVertex == target)
            {
                sw.Stop();
                steps = RetraceSteps(currentVertex);

                reachedGoal = true;
            }

            foreach (Vertex connectedVertex in currentVertex.GetConnectedVertices())
            {
                if (closedSet.Contains(connectedVertex))
                {
                    continue;
                }

                int movementCostToConnectedVertex = currentVertex.gCost + DistanceBetweenVertices(currentVertex, connectedVertex);
                if (!openSet.Contains(connectedVertex) || movementCostToConnectedVertex < connectedVertex.gCost)
                {
                    connectedVertex.gCost = movementCostToConnectedVertex;
                    connectedVertex.hCost = DistanceBetweenVertices(connectedVertex, target);
                    connectedVertex.parent = currentVertex;

                    if (!openSet.Contains(connectedVertex))
                    {
                        openSet.Add(connectedVertex);
                    }
                }
            }
        }

        sw.Stop();

        if (reachedGoal)
        {
            onPathProcessed?.Invoke(steps);
        } else
        {
            UnityEngine.Debug.Log("Unreachable Goal");
        }

        UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");

        yield return null;
    }

    /// <summary>
    /// Return a array with all the vertices used to reach a target vertex.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private Vertex[] RetraceSteps(Vertex target)
    {
        List<Vertex> steps = new List<Vertex>();

        UnityEngine.Debug.Log("Distance: " + target.gCost);

        while (target != null)
        {
            steps.Add(target);
            target = target.parent;
        }

        steps.Reverse();
        return steps.ToArray();
    }

    public static int DistanceBetweenVertices(Vertex a, Vertex b)
    {
        int xDistance = Mathf.Abs(a.ColumnIndex - b.ColumnIndex);
        int yDistance = Mathf.Abs(a.RowIndex - b.RowIndex);

        return 14 * Mathf.Min(xDistance, yDistance) + 10 * Mathf.Abs(xDistance - yDistance);
    }
}
