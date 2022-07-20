using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class MapController : MonoBehaviour
{
    [SerializeField]
    private MapLoader _mapLoader;

    [SerializeField]
    private Graph _graph;

    [SerializeField]
    private Enums.TerrainType[,] _grid;
    private int _gridRowCount;
    private int _gridColumnCount;

    [SerializeField]
    private int _vertexCount = 0;

    [SerializeField]
    [Range(1, 20)]
    private int _unitsPerVertice = 1;

    [SerializeField]
    private SpriteRenderer _graphOverlayRenderer;

    private void Awake()
    {
        _mapLoader.onSpriteCreated += CreateGridBasedOnSprite;
    }

    private void CreateGridBasedOnSprite(Sprite sprite)
    {
        Texture2D spriteTexture = sprite.texture;
        int textureHeight = spriteTexture.height;
        int textureWidth = spriteTexture.width;
        float pixelsPerUnit = sprite.pixelsPerUnit;

        _grid = new Enums.TerrainType[spriteTexture.height, spriteTexture.width];

        Color[] pixels = spriteTexture.GetPixels();

        for (int i = 0; i < textureHeight; i++)
        {
            for (int j = 0; j < textureWidth; j++)
            {
                if (pixels[i * spriteTexture.height + j].grayscale < 0.2f && pixels[i * spriteTexture.height + j].a != 0f)
                {
                    _grid[i, j] = Enums.TerrainType.Wall;
                }
                else
                {
                    _grid[i, j] = Enums.TerrainType.Path;
                }
            }
        }

        _gridRowCount = textureHeight;
        _gridColumnCount = textureWidth;
        ResizeGrid(_unitsPerVertice);
        _graph = new Graph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertice);
        _vertexCount = _graph.Vertices.Length;
        _graphOverlayRenderer.sprite = Sprite.Create(_graph._graphTexture, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertice);
    }

    private void ResizeGrid(int verticesByUnit)
    {
        Enums.TerrainType[,] resizedGrid = new Enums.TerrainType[_gridRowCount/verticesByUnit, _gridColumnCount/verticesByUnit];

        int newGridRowCount = _gridRowCount / verticesByUnit;
        int newGridColumnCount = _gridColumnCount / verticesByUnit;

        for (int i = 0; i < newGridRowCount; i++)
        {
            for (int j = 0; j < newGridColumnCount; j++)
            {
                int wallCount = 0;
                int pathCount = 0;

                for (int r = 0; r < verticesByUnit; r++)
                {
                    for (int c = 0; c < verticesByUnit; c++)
                    {
                        int vertexRowIndex = i * verticesByUnit + r;
                        int vertexColumnIndex = j * verticesByUnit + c;

                        if (_grid[vertexRowIndex, vertexColumnIndex] == Enums.TerrainType.Wall)
                        {
                            wallCount++;
                        } else
                        {
                            pathCount++;
                        }
                    }
                }

                resizedGrid[i, j] = pathCount > wallCount ? Enums.TerrainType.Path : Enums.TerrainType.Wall;
            }
        }

        _gridRowCount = newGridRowCount;
        _gridColumnCount = newGridColumnCount;

        _grid = resizedGrid;
    }

    private void PrintGraph()
    {
        foreach (Vertex vertex in _graph.Vertices)
        {
            if (vertex == null)
            {
                continue;
            }

            string s = $"Vertex: {vertex.Identifier}\nConnected Vertices: ";

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                s += $"{connectedVertex.Identifier} ";
            }

            Debug.Log(s);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (_grid != null)
            {
                if (_grid.Length < 2000)
                {
                    DrawGraph();
                }
            }
        }
    }

    private void DrawGraph()
    {
        if (_graph == null)
        {
            return;
        }

        foreach (Vertex vertex in _graph.Vertices)
        {
            if (vertex == null)
            {
                continue;
            }

            if (vertex.TerrainType == Enums.TerrainType.Path)
            {
                Gizmos.color = Color.green;
            } else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawWireCube(vertex.Position, new Vector2(vertex.Size - vertex.Size * 0.05f, vertex.Size - vertex.Size * 0.05f));

            int rowIndex;
            int columnIndex;
            Graph.ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

            Handles.color = Color.yellow;
            Handles.Label(vertex.Position, vertex.Identifier.ToString());
            Vector2 columnRowHandlePosition = vertex.Position;
            columnRowHandlePosition.y -= vertex.Size / 6;
            Handles.Label(columnRowHandlePosition, $"{rowIndex},{columnIndex}");

            foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(vertex.Position, connectedVertex.Position);
            }
        }
    }

    private void OnDisable()
    {
        _mapLoader.onSpriteCreated -= CreateGridBasedOnSprite;
    }
}
