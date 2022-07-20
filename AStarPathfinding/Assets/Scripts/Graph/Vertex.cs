using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Vertex
{
    private int _identifier;
    private int _rowIndex;
    private int _columnIndex;
    private Vector2 _position;
    private float _size;
    private List<Edge> _edges;
    private Enums.TerrainType _terrainType;
    public Vector2 Position => _position;
    public float Size => _size;
    public int Identifier => _identifier;
    public int RowIndex => _rowIndex;
    public int ColumnIndex => _columnIndex;
    public Enums.TerrainType TerrainType => _terrainType;
    public Vertex(int identifier, int rowIndex, int columnIndex, Vector2 position, float size, Enums.TerrainType terrainType)
    {
        _edges = new List<Edge>();
        _identifier = identifier;
        _rowIndex = rowIndex;
        _columnIndex = columnIndex;
        _position = position;
        _size = size;
        _terrainType = terrainType;
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

    public static Color GetColorBasedOnTerrainType(Enums.TerrainType terrainType)
    {
        switch (terrainType)
        {
            case Enums.TerrainType.None:
                return new Color(0, 0, 0, 0);

            case Enums.TerrainType.Wall:
                return new Color(1, 0, 0, 1);

            case Enums.TerrainType.Path:
                return new Color(0, 1, 0, 1);

            default:
                return new Color(0, 0, 0, 0);
        }
    }
}
