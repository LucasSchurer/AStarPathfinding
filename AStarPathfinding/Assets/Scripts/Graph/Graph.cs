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

    public Graph(int[,] grid, int rowCount, int columnCount, float vertexSize)
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

    private void GenerateGraph(int[,] grid)
    {
        CreateVertices(grid);
        /*CreateEdges();*/
        _graphTexture.Apply();
    }

    private void CreateVertices(int [,] grid)
    {
        Vector2 initialPosition = new Vector2(_vertexSize * _rowCount, _vertexSize * _columnCount);

        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                bool isWalkable = grid[i, j] == 0;
                Vector2 vertexPosition;
                /*vertexPosition.x = initialPosition.x + j * _vertexSize;*/
                vertexPosition.x = -initialPosition.x + j * _vertexSize + _vertexSize * 1.5f;
                vertexPosition.y = initialPosition.y + i * _vertexSize - _vertexSize/2;

                _vertices[i, j] = new Vertex(CantorPairing(i, j), i, j, vertexPosition, _vertexSize, isWalkable);
                UpdateOverlay(i, j, isWalkable);
            }
        }
    }

    private void CreateEdges()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                if (_vertices[i, j] != null)
                {
                    foreach (Vertex neighbour in GetVertexNeighbours(_vertices[i, j]))
                    {
                        _vertices[i, j].AddRelationship(neighbour, 1);
                    }
                }
            }
        }
    }

    private void UpdateOverlay(int row, int column, bool isWalkable)
    {
        Color pixel = _graphTexture.GetPixel(column, row);
        
        if (isWalkable)
        {
            pixel.g = 1f;
        } else
        {
            pixel.r = 1f;
        }

        _graphTexture.SetPixel(column, row, pixel);
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
            if (_vertices[rowIndex, neighbourIndex].IsWalkable)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the right neighbour index
        neighbourIndex = columnIndex + 1;
        if (IsIndexValid(rowIndex, neighbourIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].IsWalkable)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the down neighbour index
        neighbourIndex = rowIndex - 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].IsWalkable)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
            }
        }

        // Checking the right neighbour index
        neighbourIndex = rowIndex + 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].IsWalkable)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
            }            
        }

        return neighbours;
    }
}
