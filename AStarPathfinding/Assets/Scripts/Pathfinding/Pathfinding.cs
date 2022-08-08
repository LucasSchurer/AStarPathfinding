using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Pathfinding
{
    public delegate void OnPathProcessed(PathfindingLog log, Vertex[] steps);
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
    public virtual IEnumerator FindPathCoroutine(int startIdentifier, int goalIdentifier)
    {
        PathfindingLog log = new PathfindingLog();
        FindPath(startIdentifier, goalIdentifier, ref log);
        yield return null;
    }

    public virtual void FindPath(int startIdentifier, int goalIdentifier, ref PathfindingLog log, bool invokeOnPathProcessed = true)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        bool reachedGoal = false;

        if (startIdentifier == goalIdentifier)
        {
            sw.Stop();
            reachedGoal = true;
            UpdatePathfindingLog(reachedGoal, sw.ElapsedMilliseconds, 0, ref log);
            if (invokeOnPathProcessed)
            {
                onPathProcessed?.Invoke(log, null);
            }
            return;
        }

        Vertex start;
        Vertex goal;

        if (!_graph.Vertices.TryGetValue(startIdentifier, out start) || !_graph.Vertices.TryGetValue(goalIdentifier, out goal))
        {
            sw.Stop();
            reachedGoal = false;
            UpdatePathfindingLog(reachedGoal, sw.ElapsedMilliseconds, -1, ref log);
            if (invokeOnPathProcessed)
            {
                onPathProcessed?.Invoke(log, null);
            }
            return;
        }

        HashSet<Vertex> closedSet = new HashSet<Vertex>();
        Heap<Vertex> openSet = new Heap<Vertex>(_graph.VerticesCount);
        Vertex[] steps = null;
        Vertex currentVertex;

        start.hCost = DistanceBetweenVertices(start, goal);
        start.gCost = 0;
        start.parent = null;
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            currentVertex = openSet.RemoveFirst();
            closedSet.Add(currentVertex);
            log.closedSetSize++;

            if (currentVertex == goal)
            {
                sw.Stop();
                steps = RetraceSteps(goal);
                reachedGoal = true;
                break;
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
                    connectedVertex.hCost = DistanceBetweenVertices(connectedVertex, goal);
                    connectedVertex.parent = currentVertex;

                    if (!openSet.Contains(connectedVertex))
                    {
                        openSet.Add(connectedVertex);
                        log.openSetSize++;
                    }
                    else
                    {
                        openSet.UpdateNode(connectedVertex);
                    }
                }
            }
        }

        sw.Stop();
        UpdatePathfindingLog(start, goal, reachedGoal, sw.ElapsedMilliseconds, ref log);
        
        if (invokeOnPathProcessed)
        {
            onPathProcessed?.Invoke(log, steps);
        }
    }

    /// <summary>
    /// Return a array with all the vertices used to reach a target vertex.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    protected Vertex[] RetraceSteps(Vertex target)
    {
        List<Vertex> steps = new List<Vertex>();

        while (target != null)
        {
            steps.Add(target);
            target = target.parent;
        }

        steps.Reverse();
        return steps.ToArray();
    }

    protected void UpdatePathfindingLog(Vertex start, Vertex goal, bool reachedGoal, float elapsedTime, ref PathfindingLog log)
    {
        log.SetStartInfo(start.Identifier);
        log.SetGoalInfo(goal.Identifier);
        UpdatePathfindingLog(reachedGoal, elapsedTime, reachedGoal ? goal.gCost : - 1, ref log);
    }

    protected void UpdatePathfindingLog(bool reachedGoal, float elapsedTime, int distance, ref PathfindingLog log)
    {
        log.reachedGoal = reachedGoal;
        log.elapsedTime = elapsedTime;
        log.distance = distance;
    }

    public static int DistanceBetweenVertices(Vertex a, Vertex b)
    {
        int xDistance = Mathf.Abs(a.ColumnIndex - b.ColumnIndex);
        int yDistance = Mathf.Abs(a.RowIndex - b.RowIndex);

        return 14 * Mathf.Min(xDistance, yDistance) + 10 * Mathf.Abs(xDistance - yDistance);
    }
}
