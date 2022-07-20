using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GraphController : MonoBehaviour
{
    [SerializeField]
    private Graph _graph;

    private void Awake()
    {
        _graph = new Graph();

        int[,] grid = new int[3, 3] { 
                                    { 0, 0, 0 }, 
                                    { 0, 0, 0 }, 
                                    { 0, 0, 0 } };

        _graph.GenerateGraph(grid, 3, 3);
        PrintGraph();
    }

    private void PrintGraph()
    {
        foreach (Vertex vertex in _graph.Vertices)
        {
            string s = $"Vertex: {vertex.Number}\nConnected Vertices: ";

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                s += $"{connectedVertex.Number} ";
            }

            Debug.Log(s);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            DrawGraph();
        }
    }

    private void DrawGraph()
    {
        foreach (Vertex vertex in _graph.Vertices)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(vertex.Position, 0.3f);

            Handles.color = Color.yellow;
            Handles.Label(vertex.Position, vertex.Number.ToString());

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(vertex.Position, connectedVertex.Position);
            }
        }
    }
}
