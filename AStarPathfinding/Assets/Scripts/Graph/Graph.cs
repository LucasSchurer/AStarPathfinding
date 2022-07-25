using System;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    /* https://gist.github.com/GibsS/fdba8e3cdbd307652fc3c01336b32534 */
    public static int CantorPairing(int i, int j) => (((i + j) * (i + j + 1)) / 2) + j;
    public static void ReverseCantorPairing(int m, out int i, out int j)
    {
        int t = (int)Math.Floor((-1 + Math.Sqrt(1 + 8 * m)) * 0.5f);
        i = t * (t + 3) / 2 - m;
        j = m - t * (t + 1) / 2;
    }

    protected Enums.TerrainType[,] _grid;
    protected int _rowCount;
    protected int _columnCount;
    protected float _vertexSize;

    protected Dictionary<int, Vertex> _vertices;
    public int VerticesCount => _vertices.Count;
    public int RowCount => _rowCount;
    public int ColumnCount => _columnCount;
    public Dictionary<int, Vertex> Vertices => _vertices;

    public bool isSSG = false;
    public GraphDrawer graphDrawer;
    public bool IsIndexValid(int rowIndex, int columnIndex) => rowIndex >= 0 && rowIndex < _rowCount && columnIndex >= 0 && columnIndex < _columnCount;

    public Graph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize)
    {
        _rowCount = rowCount;
        _columnCount = columnCount;
        _grid = grid;
        _vertices = new Dictionary<int, Vertex>(grid.Length);
        _vertexSize = vertexSize;
        GenerateGraph();
    }

    protected virtual void GenerateGraph()
    {
        CreateVertices();
        CreateEdges();
    }

    protected virtual void CreateVertices()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                TryCreateVertex(i, j, out _);
            }
        }
    }

    protected virtual void CreateEdges()
    {
        foreach (Vertex vertex in _vertices.Values)
        {
            if (vertex.TerrainType != Enums.TerrainType.Wall)
            {
                foreach (Vertex neighbour in GetVertexNeighbours(vertex))
                {
                    vertex.ConnectTo(neighbour, 1);
                }
            }
        }
/*


        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                if (_vertices[i, j] != null && _vertices[i, j].TerrainType != Enums.TerrainType.Wall)
                {
                    foreach (Vertex neighbour in GetVertexNeighbours(_vertices[i, j]))
                    {
                        _vertices[i, j].ConnectTo(neighbour, 1);
                    }
                }
            }
        }*/
    }

    protected bool TryCreateVertex(int row, int column, out Vertex createdVertex)
    {
        if (IsIndexValid(row, column))
        {
            int identifier = CantorPairing(row, column);

            if (!_vertices.ContainsKey(identifier))
            {
                Vector2 vertexPosition;
                vertexPosition.x = column * _vertexSize + _vertexSize / 2;
                vertexPosition.y = row * _vertexSize + _vertexSize / 2;

                createdVertex = new Vertex(identifier, row, column, vertexPosition, _vertexSize, _grid[row, column]);

                _vertices.Add(createdVertex.Identifier, createdVertex);

                return true;
            }
        }

        createdVertex = null;
        return false;
    }

    private List<Vertex> GetVertexNeighbours(Vertex vertex)
    {
        List<Vertex> neighbours = new List<Vertex>();

        if (!IsIndexValid(vertex.RowIndex, vertex.ColumnIndex))
        {
            return neighbours;
        }
        
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                Vertex neighbour;
                if (TryToGetVertex(vertex.RowIndex + i, vertex.ColumnIndex + j, out neighbour))
                {
                    if (neighbour.TerrainType != Enums.TerrainType.Wall)
                    {
                        if (i != 0 && j != 0)
                        {
                            Vertex cardinalNeighbour;
                            if (TryToGetVertex(vertex.RowIndex + i, vertex.ColumnIndex, out cardinalNeighbour))
                            {
                                if (cardinalNeighbour.TerrainType != Enums.TerrainType.Wall)
                                {
                                    if (TryToGetVertex(vertex.RowIndex, vertex.ColumnIndex + j, out cardinalNeighbour))
                                    {
                                        if (cardinalNeighbour.TerrainType != Enums.TerrainType.Wall)
                                        {
                                            neighbours.Add(neighbour);
                                        }
                                    }
                                }
                            }
                        } else
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }
        }

        return neighbours;
    }

    public Vertex GetVertexOnPosition(Vector2 position)
    {
        int rowIndex = (int)(position.y / _vertexSize);
        int columnIndex = (int)(position.x / _vertexSize);

        Vertex vertex;

        if (_vertices.TryGetValue(CantorPairing(rowIndex, columnIndex), out vertex))
        {
            return vertex;
        }

        return null;
    }
    
    public bool TryToGetIndexesOnPosition(Vector2 position, out int rowIndex, out int columnIndex)
    {
        rowIndex = (int)(position.y / _vertexSize);
        columnIndex = (int)(position.x / _vertexSize);

        return rowIndex < 0 || columnIndex < 0;
    }

    protected bool TryToGetVertex(int rowIndex, int columnIndex, out Vertex vertex)
    {
        return _vertices.TryGetValue(CantorPairing(rowIndex, columnIndex), out vertex);
    }
}
