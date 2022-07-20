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

    public void CreateVertex(Vector2 position)
    {
        _vertices.Add(new Vertex(position, _vertices.Count));
    }
}
