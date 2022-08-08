using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class SubgoalGraphPathfinding : Pathfinding
{
    private SubgoalGraph _graph;

    public SubgoalGraphPathfinding(SubgoalGraph graph) : base(graph)
    {
        _graph = graph;
    }

    public override IEnumerator FindPath(int sourceIdentifier, int targetIdentifier)
    {
        bool addedSource = false;
        bool addedTarget = false;
        Vertex sourceVertex;
        Vertex targetVertex;

        _graph.Vertices.TryGetValue(sourceIdentifier, out sourceVertex);
        _graph.Vertices.TryGetValue(targetIdentifier, out targetVertex);

        if (sourceVertex == null)
        {
            int sourceRow;
            int sourceColumn;
            Graph.ReverseCantorPairing(sourceIdentifier, out sourceRow, out sourceColumn);
            _graph.AddVertex(sourceRow, sourceColumn, out sourceVertex);
            addedSource = true;
        }

        if (targetVertex == null)
        {
            int targetRow;
            int targetColumn;
            Graph.ReverseCantorPairing(targetIdentifier, out targetRow, out targetColumn);
            _graph.AddVertex(targetRow, targetColumn, out targetVertex);
            addedTarget = true;
        }

        if (addedSource)
        {
            _graph.CreateVertexEdges(sourceVertex);
        }

        if (addedTarget)
        {
            _graph.CreateVertexEdges(targetVertex);
        }

        yield return base.FindPath(sourceVertex.Identifier, targetVertex.Identifier);
/*
        if (addedSource)
        {
            _graph.RemoveVertex(sourceVertex);
        }

        if (addedTarget)
        {
            _graph.RemoveVertex(targetVertex);
        }*/

        yield return null;
    }

    public override void FindPathUsingLog(int sourceIdentifier, int targetIdentifier, ref PathfindingLog log)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        bool addedSource = false;
        bool addedTarget = false;
        Vertex sourceVertex;
        Vertex targetVertex;

        _graph.Vertices.TryGetValue(sourceIdentifier, out sourceVertex);
        _graph.Vertices.TryGetValue(targetIdentifier, out targetVertex);

        if (sourceVertex == null)
        {
            int sourceRow;
            int sourceColumn;
            Graph.ReverseCantorPairing(sourceIdentifier, out sourceRow, out sourceColumn);
            _graph.AddVertex(sourceRow, sourceColumn, out sourceVertex);
            addedSource = true;
        }

        if (targetVertex == null)
        {
            int targetRow;
            int targetColumn;
            Graph.ReverseCantorPairing(targetIdentifier, out targetRow, out targetColumn);
            _graph.AddVertex(targetRow, targetColumn, out targetVertex);
            addedTarget = true;
        }

        if (addedSource)
        {
            _graph.CreateVertexEdges(sourceVertex);
        }
        
        if (addedTarget)
        {
            _graph.CreateVertexEdges(targetVertex);
        }

        base.FindPathUsingLog(sourceVertex.Identifier, targetVertex.Identifier, ref log);

        if (addedSource)
        {
            _graph.RemoveVertex(sourceVertex);
        }

        if (addedTarget)
        {
            _graph.RemoveVertex(targetVertex);
        }

        sw.Stop();
        log.timeSpent = sw.ElapsedMilliseconds;
    }
}
