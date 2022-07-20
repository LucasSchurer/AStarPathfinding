using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphController : MonoBehaviour
{
    [SerializeField]
    private Graph _graph;

    private void Awake()
    {
        _graph = new Graph();
        _graph.GenerateGraph();
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
            
            foreach(Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(vertex.Position, connectedVertex.Position);
            }
        }

        _graph.RandomMoveVertices();
    }
}
