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
        texture2D.Apply();
    }

    public void Draw()
    {
        foreach (Vertex vertex in _graph.Vertices.Values)
        {
            texture2D.SetPixel(vertex.ColumnIndex, vertex.RowIndex, GetVertexColor(vertex));
        }

        texture2D.Apply();
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
