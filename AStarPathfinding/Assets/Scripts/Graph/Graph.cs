using System;
using System.Collections;
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

    public Texture2D _graphTexture;

    private int _rowCount;
    private int _columnCount;
    private float _vertexSize;

    private Vertex[,] _vertices;
    public Vertex[,] Vertices => _vertices;

    public bool IsIndexValid(int rowIndex, int columnIndex) => rowIndex >= 0 && rowIndex < _rowCount && columnIndex >= 0 && columnIndex < _columnCount;

    public Graph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize)
    {
        _rowCount = rowCount;
        _columnCount = columnCount;
        _vertices = new Vertex[_rowCount, _columnCount];
        _vertexSize = vertexSize;
        _graphTexture = new Texture2D(_columnCount, _rowCount);
        _graphTexture.filterMode = FilterMode.Point;
        _graphTexture.Apply();

        GenerateGraph(grid);
    }

    private void GenerateGraph(Enums.TerrainType[,] grid)
    {
        CreateVertices(grid);
        CreateEdges();
        _graphTexture.Apply();
    }

    private void CreateVertices(Enums.TerrainType [,] grid)
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                Vector2 vertexPosition;
                vertexPosition.x = j * _vertexSize + _vertexSize/2;
                vertexPosition.y = i * _vertexSize + _vertexSize/2;

                _vertices[i, j] = new Vertex(CantorPairing(i, j), i, j, vertexPosition, _vertexSize, grid[i, j]);
                UpdateOverlay(_vertices[i, j], false);
            }
        }

        _graphTexture.Apply();
    }

    private void CreateEdges()
    {
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
        }
    }
   
    public void UpdateOverlay(Vertex vertex, bool applyChangesToTexture = true)
    {
        UpdateOverlay(vertex.RowIndex, vertex.ColumnIndex, vertex.TerrainType, applyChangesToTexture);
    }

    private void UpdateOverlay(int row, int column, Enums.TerrainType terrainType, bool applyChangesToTexture = true)
    {
        _graphTexture.SetPixel(column, row, Vertex.GetColorBasedOnTerrainType(terrainType));

        if (applyChangesToTexture)
        {
            _graphTexture.Apply();
        }
    }

    private List<Vertex> GetVertexNeighbours(Vertex vertex)
    {
        List<Vertex> neighbours = new List<Vertex>();
        int rowIndex;
        int columnIndex;
        ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

        if (!IsIndexValid(rowIndex, columnIndex))
        {
            return neighbours;
        }

        // Checking the left neighbour index
        int neighbourIndex = columnIndex - 1;
        if (IsIndexValid(rowIndex, neighbourIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the right neighbour index
        neighbourIndex = columnIndex + 1;
        if (IsIndexValid(rowIndex, neighbourIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the down neighbour index
        neighbourIndex = rowIndex - 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[neighbourIndex, columnIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
            }
        }

        // Checking the up neighbour index
        neighbourIndex = rowIndex + 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[neighbourIndex, columnIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
            }            
        }

        return neighbours;
    }

    public Vertex GetVertexOnPosition(Vector2 position)
    {
        int rowIndex = (int)(position.y / _vertexSize);
        int columnIndex = (int)(position.x / _vertexSize);

        Debug.Log($"{rowIndex},{columnIndex} {position}");

        if (IsIndexValid(rowIndex, columnIndex))
        {
            return _vertices[rowIndex, columnIndex];
        }

        return null;
    }

    public static void ResizeGrid(ref int[,] grid, int rowCount, int columnCount, int combinedRowsAmount, int combinedColumnsAmount)
    {
        int[,] newGrid = new int[rowCount / combinedRowsAmount, columnCount / combinedColumnsAmount];

        for (int i = 0; i < rowCount; i += combinedRowsAmount)
        {
            for (int j = 0; j < columnCount; j += combinedColumnsAmount)
            {
                int walkable = 0;

                for (int r = 0; r < combinedRowsAmount; r++)
                {
                    for (int c = 0; c < combinedColumnsAmount; c++)
                    {
                        int rowIndex = Mathf.Clamp(i + r, 0, rowCount - 1);
                        int columnIndex = Mathf.Clamp(j + c, 0, columnCount - 1);

                        try
                        {
                            walkable += grid[rowIndex, columnIndex];
                        } catch (IndexOutOfRangeException) 
                        {
                            Debug.Log("erro");
                        }
                    }
                }

                newGrid[i / combinedRowsAmount, j / combinedColumnsAmount] = walkable;
            }
        }

        grid = newGrid;
    }
}
