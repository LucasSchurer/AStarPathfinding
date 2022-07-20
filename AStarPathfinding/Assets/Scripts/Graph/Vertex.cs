using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex
{
    private int _number;
    private Vector2 _position;
    private List<Edge> _edges;

    public Vector2 Position => _position;
    public int Number => _number;
    public Vertex(Vector2 position, int number = 0)
    {
        _edges = new List<Edge>();
        _position = position;
        _number = number;
    }

    public void AddRelationship(Vertex target, float cost)
    {
        if (target != null && target != this && !DoesRelationshipExists(target))
        {
            _edges.Add(new Edge(this, target, cost));
        }
    }

    public bool DoesRelationshipExists(Vertex target)
    {
        foreach (Edge edge in _edges)
        {
            if (edge.Target == target)
            {
                return true;
            }
        }

        return false;
    }

    public static void CreateRelationship(Vertex source, Vertex target, float cost)
    {
        source.AddRelationship(target, cost);
    }

    public List<Vertex> GetConnectedVertices()
    {
        List<Vertex> connectedVertices = _edges.Select(e => e.Target).ToList();

        return connectedVertices;
    }

    public void SetPosition(Vector2 position)
    {
        _position = position;
    }
}
