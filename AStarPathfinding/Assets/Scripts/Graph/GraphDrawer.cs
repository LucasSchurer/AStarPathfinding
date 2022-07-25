using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphDrawer
{
    public Texture2D texture2D;
    private Graph _graph;

    public GraphDrawer(Graph graph)
    {
        _graph = graph;
        texture2D = new Texture2D(_graph.ColumnCount, _graph.RowCount);
        texture2D.filterMode = FilterMode.Point;
        Clear();
    }

    public void Draw()
    {
        foreach (Vertex vertex in _graph.Vertices.Values)
        {
            texture2D.SetPixel(vertex.ColumnIndex, vertex.RowIndex, GetVertexColor(vertex));
        }

        texture2D.Apply();
    }

    public void Clear()
    {
        Color32[] pixels = new Color32[_graph.ColumnCount * _graph.RowCount];
        for (int i = 0; i < _graph.ColumnCount * _graph.RowCount; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }
        texture2D.SetPixels32(pixels);
        Apply();
    }

    public void DrawVertex(Vertex vertex, bool shouldCallApply = true)
    {
        DrawVertex(vertex, GetVertexColor(vertex), shouldCallApply);
    }

    public void DrawVertex(Vertex vertex, Color32 color, bool shouldCallApply = true)
    {
        DrawPixel(vertex.RowIndex, vertex.ColumnIndex, color, shouldCallApply);
    }

    public void DrawPixel(int row, int column, Color32 color, bool shouldCallApply = true)
    {
        texture2D.SetPixel(column, row, color);

        if (shouldCallApply)
        {
            Apply();
        }
    }

    public void DrawVertices(Vertex[] vertices, bool shouldCallApply = true)
    {
        foreach (Vertex vertex in vertices)
        {
            DrawVertex(vertex, false);
        }

        if (shouldCallApply)
        {
            Apply();
        }
    }

    public void DrawVertices(Vertex[] vertices, Color32 color, bool shouldCallApply = true)
    {
        foreach (Vertex vertex in vertices)
        {
            DrawVertex(vertex, color, false);
        }

        if (shouldCallApply)
        {
            Apply();
        }
    }

    public void DrawPathBetweenVertices(Vertex a, Vertex b, Color color, bool shouldCallApply = true)
    {
        if (a == null || b == null)
        {
            return;
        }

        int xDistance = Mathf.Abs(a.ColumnIndex - b.ColumnIndex);
        int yDistance = Mathf.Abs(a.RowIndex - b.RowIndex);

        int verticalDirection = a.RowIndex > b.RowIndex ? -1 : a.RowIndex == b.RowIndex ? 0 : 1;
        int horizontalDirection = a.ColumnIndex > b.ColumnIndex ? -1 : a.ColumnIndex == b.ColumnIndex ? 0 : 1;

        int distanceDiagonal = 14 * Mathf.Min(xDistance, yDistance);
        int distanceCardinal = 10 * Mathf.Abs(xDistance - yDistance);

        for (int i = 0; i < distanceCardinal/10; i++)
        {
            DrawPixel(a.RowIndex + (i + 1) * verticalDirection, a.ColumnIndex + (i + 1) * horizontalDirection, Color.red);
        }

        Apply();
    }

    public void Apply()
    {
        texture2D.Apply();
    }

    public static Color32 GetVertexColor(Vertex vertex)
    {
        switch (vertex.TerrainType)
        {
            case Enums.TerrainType.None:
                return new Color32(0, 0, 0, 0);

            case Enums.TerrainType.Wall:
                return new Color32(0, 0, 0, 100);

            case Enums.TerrainType.Path:
                return new Color32(0, 255, 0, 100);

            default:
                return new Color32(0, 0, 0, 100);
        }
    }
}
