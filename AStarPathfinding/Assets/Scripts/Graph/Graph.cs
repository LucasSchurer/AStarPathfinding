using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    private List<Vertex> _vertices;
    public List<Vertex> Vertices => _vertices;

    public Graph()
    {
        _vertices = new List<Vertex>();
    }

    public void RandomMoveVertices()
    {
        foreach (Vertex vertex in _vertices)
        {
            Vector2 newPosition = vertex.Position;
            newPosition.x += Random.Range(-0.05f, 0.05f);
            newPosition.y += Random.Range(-0.05f, 0.05f);
            newPosition.x = Mathf.Clamp(newPosition.x, -10, 10);
            newPosition.y = Mathf.Clamp(newPosition.y, -10, 10);
            vertex.SetPosition(newPosition);
        }
    }

    public void GenerateGraph()
    {
        for (int i = 0; i < 10; i++)
        {
            CreateVertex(new Vector2(Random.Range(-5, 5), Random.Range(-5, 5)));
        }

        for (int i = 0; i < Random.Range(5, 15); i++)
        {
            Vertex source = _vertices[Random.Range(0, _vertices.Count - 1)];
            Vertex target = _vertices[Random.Range(0, _vertices.Count - 1)];
            source.AddRelationship(target, 1);
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
                    CreateVertex(new Vector2(2 * i, 2 * j), (i * 10 + j));
                }
            }
        }
    }

    public void CreateVertex(Vector2 position, int number = 0)
    {
        _vertices.Add(new Vertex(position, number));
    }
}
