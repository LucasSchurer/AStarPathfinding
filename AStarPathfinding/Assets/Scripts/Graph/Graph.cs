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

    private Dictionary<int, Vertex> _vertices;
    public Dictionary<int, Vertex> Vertices => _vertices;

    public Graph()
    {
        _vertices = new Dictionary<int, Vertex>();
    }

    public void RandomMoveVertices()
    {
        foreach (Vertex vertex in _vertices.Values)
        {
            Vector2 newPosition = vertex.Position;
            newPosition.x += UnityEngine.Random.Range(-0.05f, 0.05f);
            newPosition.y += UnityEngine.Random.Range(-0.05f, 0.05f);
            newPosition.x = Mathf.Clamp(newPosition.x, -10, 10);
            newPosition.y = Mathf.Clamp(newPosition.y, -10, 10);
            vertex.SetPosition(newPosition);
        }
    }

    public void GenerateGraph(int[,] grid, int rowCount, int columnCount)
    {
        CreateVertices(grid, rowCount, columnCount);
    }

    private void CreateVertices(int [,] grid, int rowCount, int columnCount)
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                if (grid[i, j] == 0)
                {
                    CreateVertex(grid, rowCount, columnCount, i, j);
                }
            }
        }
    }

    private void CreateVertex(int [,] grid, int rowCount, int columnCount, int rowIndex, int columnIndex)
    {
        if (grid[rowIndex, columnIndex] == 1)
        {
            return;
        }

        int vertexIdentifier = CantorPairing(rowIndex, columnIndex);
        Vertex vertex;
        CreateVertex(vertexIdentifier, new Vector2(1 * columnIndex, 1 * rowIndex), out vertex);
        
        if (vertex.GetConnectedVertices().Count > 0)
        {
            return;
        }

        foreach (Tuple<int, int> neighbourIndex in MatrixUtils<int>.GetNeighboursIndexes(grid, rowCount, columnCount, rowIndex, columnIndex))
        {
            if (grid[neighbourIndex.Item1, neighbourIndex.Item2] == 1)
            {
                continue;
            }

            int neighbourIdentifier = CantorPairing(neighbourIndex.Item1, neighbourIndex.Item2);

            if (!_vertices.ContainsKey(neighbourIdentifier))
            {
                CreateVertex(grid, rowCount, columnCount, neighbourIndex.Item1, neighbourIndex.Item2);
            }

            Vertex neighbour;
            _vertices.TryGetValue(neighbourIdentifier, out neighbour);
            vertex.AddRelationship(neighbour, 1);
        }
    }

    public bool CreateVertex(Vector2 position)
    {
        return CreateVertex(_vertices.Count, position);
    }

    public bool CreateVertex(int identifier, Vector2 position)
    {
        return CreateVertex(identifier, position, out _);
    }

    public bool CreateVertex(int identifier, Vector2 position, out Vertex addedVertex)
    {
        try
        {
            Vertex vertex = new Vertex(identifier, position);
            _vertices.Add(identifier, vertex);
            addedVertex = vertex;
            return true;
        } catch (ArgumentException)
        {
            Vertex existingVertex;
            if (_vertices.TryGetValue(identifier, out existingVertex))
            {
                addedVertex = existingVertex;
            } else
            {
                addedVertex = null;
            }

            return false;
        }
    }
}
