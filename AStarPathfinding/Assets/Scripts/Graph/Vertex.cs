using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex
{
    private int _identifier;
    private Vector2 _position;
    private List<Edge> _edges;
    public Vector2 Position => _position;
    public int Identifier => _identifier;
    public Vertex(int identifier, Vector2 position)
    {
        _edges = new List<Edge>();
        _identifier = identifier;
        _position = position;
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
        List<Vertex> connectedVertices = new List<Vertex>();

        foreach (Edge edge in _edges)
        {
            connectedVertices.Add(edge.Target);
        }

        return connectedVertices;
    }

    public void SetPosition(Vector2 position)
    {
        _position = position;
    }
}
