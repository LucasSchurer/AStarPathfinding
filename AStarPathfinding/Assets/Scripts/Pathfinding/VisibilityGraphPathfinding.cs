using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class VisibilityGraphPathfinding : Pathfinding
{
    private VisibilityGraph _graph;

    public VisibilityGraphPathfinding(VisibilityGraph graph) : base(graph)
    {
        _graph = graph;
    }

    public override void FindPath(int startIdentifier, int goalIdentifier, ref PathfindingLog log, bool invokeOnPathProcessed = true)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        bool addedStart = false;
        bool addedGoal = false;

        Vertex start;
        Vertex goal;

        _graph.Vertices.TryGetValue(startIdentifier, out start);
        _graph.Vertices.TryGetValue(goalIdentifier, out goal);

        if (start == null)
        {
            start = _graph.CreateVertex(startIdentifier, false);
            
            if (start == null)
            {
                sw.Stop();
                UpdatePathfindingLog(false, sw.ElapsedMilliseconds, -1, ref log);
                if (invokeOnPathProcessed)
                {
                    onPathProcessed?.Invoke(log, null);
                }
                return;
            }

            addedStart = true;
        }

        if (goal == null)
        {
            goal = _graph.CreateVertex(goalIdentifier, false);

            if (goal == null)
            {
                sw.Stop();
                UpdatePathfindingLog(false, sw.ElapsedMilliseconds, -1, ref log);
                if (invokeOnPathProcessed)
                {
                    onPathProcessed?.Invoke(log, null);
                }
                return;
            }

            addedGoal = true;
        }

        if (addedStart)
        {
            _graph.CreateVertexEdges(start);
        }
        
        if (addedGoal)
        {
            _graph.CreateVertexEdges(goal);
        }

        base.FindPath(start.Identifier, goal.Identifier, ref log, invokeOnPathProcessed);

        if (addedStart)
        {
            _graph.RemoveVertex(start);
        }

        if (addedGoal)
        {
            _graph.RemoveVertex(goal);
        }

        sw.Stop();
        log.elapsedTime = sw.ElapsedMilliseconds;
    }
}
