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

        /*Graph.ResizeGrid(ref grid, tex.height, tex.width, 2, 2);*/

        _graph = new Graph(_grid, textureHeight, textureWidth, pixelsPerUnit);
        _graphOverlayRenderer.sprite = Sprite.Create(_graph._graphTexture, new Rect(0, 0, textureWidth, textureHeight), transform.position, pixelsPerUnit);
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            /*DrawGraph();*/
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

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(vertex.Position, new Vector2(1, 1));

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
