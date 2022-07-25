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

    private Enums.TerrainType[,] _grid;
    private int _rowCount;
    private int _columnCount;
    private float _vertexSize;

    private Dictionary<int, Vertex> _vertices;
    public int VerticesCount => _vertices.Count;
    public int RowCount => _rowCount;
    public int ColumnCount => _columnCount;
    public Dictionary<int, Vertex> Vertices => _vertices;

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

    private void GenerateGraph()
    {
        CreateVertices();
        CreateEdges();
    }

    private void CreateVertices()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                Vector2 vertexPosition;
                vertexPosition.x = j * _vertexSize + _vertexSize / 2;
                vertexPosition.y = i * _vertexSize + _vertexSize / 2;

                Vertex newVertex = new Vertex(CantorPairing(i, j), i, j, vertexPosition, _vertexSize, _grid[i, j]);

                _vertices.Add(newVertex.Identifier, newVertex); 
            }
        }
    }

   /* private void GenerateVisibilityGraph()
    {
        _visibilityGraph = new Dictionary<int, Vertex>();

        foreach (Vertex vertex in _vertices.Values)
        {
            if (vertex.TerrainType != Enums.TerrainType.Path)
            {
                continue;
            }
            

            for (int i = -1; i < 2; i+=2)
            {
                for (int j = -1; j < 2; j+=2)
                {
                    int neighbourRowIndex = vertex.RowIndex + i;
                    int neighbourColumnIndex = vertex.ColumnIndex + j;

                    if (IsIndexValid(neighbourRowIndex, neighbourColumnIndex))
                    {
                        if (_vertices[neighbourRowIndex, neighbourColumnIndex].TerrainType == Enums.TerrainType.Wall)
                        {
                            if (IsIndexValid(vertex.RowIndex + i, vertex.ColumnIndex))
                            {
                                if (_vertices[vertex.RowIndex + i, vertex.ColumnIndex].TerrainType == Enums.TerrainType.Path)
                                {
                                    if (IsIndexValid(vertex.RowIndex, vertex.ColumnIndex + j))
                                    {
                                        if (_vertices[vertex.RowIndex, vertex.ColumnIndex + j].TerrainType == Enums.TerrainType.Path)
                                        {
                                            Vertex subgoal = new Vertex(vertex.Identifier, vertex.RowIndex, vertex.ColumnIndex, vertex.Position, vertex.Size, Enums.TerrainType.Path);
                                            _visibilityGraph.Add(subgoal.Identifier, subgoal);
                                            UpdateOverlay(vertex, Color.blue, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        _graphTexture.Apply();
    }*/

    private void CreateEdges()
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
                if (TryGetVertex(vertex.RowIndex + i, vertex.ColumnIndex + j, out neighbour))
                {
                    if (neighbour.TerrainType != Enums.TerrainType.Wall)
                    {
                        neighbours.Add(neighbour);
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
    private bool TryGetVertex(int rowIndex, int columnIndex, out Vertex vertex)
    {
        return _vertices.TryGetValue(CantorPairing(rowIndex, columnIndex), out vertex);
    }
    private int Clearance(Vertex vertex, int verticalMovement, int horizontalMovement)
    {
        /*        int distance = 1;

                while (true)
                {
                    int currentRow = vertex.RowIndex + (verticalMovement * distance);
                    int currentColumn = vertex.ColumnIndex + (horizontalMovement * distance);

                    if (_visibilityGraph.ContainsKey(CantorPairing(currentRow, currentColumn)))
                    {
                        return distance;
                    }

                    if (IsIndexValid(currentRow, currentColumn))
                    {
                        if (_vertices[currentRow, currentColumn].TerrainType == Enums.TerrainType.Wall)
                        {
                            return distance - 1;
                        }
                    } else
                    {
                        return distance - 1;
                    }

                    distance++;
                }*/

        return 0;
    }
}
