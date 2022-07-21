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
    private Dictionary<int, Edge> _edges;
    private Enums.TerrainType _terrainType;
    public Vector2 Position => _position;
    public float Size => _size;
    public int Identifier => _identifier;
    public int RowIndex => _rowIndex;
    public int ColumnIndex => _columnIndex;
    public Enums.TerrainType TerrainType => _terrainType;
    public Vertex(int identifier, int rowIndex, int columnIndex, Vector2 position, float size, Enums.TerrainType terrainType)
    {
        _edges = new Dictionary<int, Edge>();
        _identifier = identifier;
        _rowIndex = rowIndex;
        _columnIndex = columnIndex;
        _position = position;
        _size = size;
        _terrainType = terrainType;
    }

    public void ConnectTo(Vertex target, float cost)
    {
        if (!_edges.ContainsKey(target.Identifier))
        {
            _edges.Add(target.Identifier, new Edge(this, target, cost));
        }
    }

    public bool IsConnectedTo(Vertex target)
    {
        return _edges.ContainsKey(target.Identifier);
    }

    public Vertex[] GetConnectedVertices()
    {
        Vertex[] connectedVertices = new Vertex[_edges.Count()];

        for (int i = 0; i < _edges.Count; i++)
        {
            connectedVertices[i] = _edges.ElementAt(i).Value.Target;
        }

        return connectedVertices;
    }

    public void ChangeTerrainType(Enums.TerrainType newTerrainType)
    {
        _terrainType = newTerrainType;
    }

    public static Color GetColorBasedOnTerrainType(Enums.TerrainType terrainType)
    {
        switch (terrainType)
        {
            case Enums.TerrainType.None:
                return new Color(0, 0, 0, 0);

            case Enums.TerrainType.Wall:
                return new Color(1, 0, 0, 0.4f);

            case Enums.TerrainType.Path:
                return new Color(0, 1, 0, 0.4f);

            default:
                return new Color(0, 0, 0, 0.4f);
        }
    }
}
