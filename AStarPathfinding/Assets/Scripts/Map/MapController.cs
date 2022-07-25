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
    [Range(1, 50)]
    private int _unitsPerVertex = 1;

    [SerializeField]
    private SpriteRenderer _graphOverlayRenderer;

    private Vertex _selectedVertex;
    private Vertex _targetVertex;
    private Vertex _visibilityGraphSelectedVertex;

    private void Awake()
    {
        _mapLoader.onSpriteCreated += CreateGridBasedOnSprite;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (_targetVertex != null)
                {
                    _graph.UpdateOverlay(_targetVertex);
                }

                _targetVertex = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (_targetVertex != null)
                {
                    _graph.UpdateOverlay(_targetVertex, Color.yellow);
                }
            } else if (Input.GetKey(KeyCode.LeftAlt))
            {

                Vertex temp = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (temp != null)
                {
                    _graph.UpdateOverlay(temp, Color.blue);
                }


                /*                if (_visibilityGraphSelectedVertex != null)
                                {
                                    _graph.UpdateOverlay(_visibilityGraphSelectedVertex, Color.blue, true);
                                }

                                _graph.TryGetSubgoal(_graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition)).Identifier, out _visibilityGraphSelectedVertex);

                                if (_visibilityGraphSelectedVertex != null)
                                {
                                    _graph.UpdateOverlay(_visibilityGraphSelectedVertex, Color.cyan);
                                }*/
            } else if (Input.GetKey(KeyCode.LeftControl))
            {
                Vertex temp = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (temp != null)
                {
                    _graph.UpdateOverlay(temp, Color.red);
                }

            } else
            { 
                if (_selectedVertex != null)
                {
                    _graph.UpdateOverlay(_selectedVertex);
                }

                _selectedVertex = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (_selectedVertex != null)
                {
                    _graph.UpdateOverlay(_selectedVertex, Color.yellow);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && _selectedVertex != null && _targetVertex != null)
        {
            StopAllCoroutines();
            StartCoroutine(_graph.FindPath(_selectedVertex, _targetVertex));
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            foreach (Vertex vertex in _graph.Vertices)
            {
                _graph.UpdateOverlay(vertex, false);
            }

            _graph._graphTexture.Apply();
        }

        if (Input.GetKeyDown(KeyCode.H) && _selectedVertex != null)
        {
            _graph.PrintClearance(_selectedVertex);
        }
    }

    private void OnPathProcessed(Vertex[] steps)
    {
        ClearGraph();
        DrawSteps(steps);
    }

    private void DrawSteps(Vertex[] steps)
    {
        foreach (Vertex vertex in steps)
        {
            _graph.UpdateOverlay(vertex, Color.red, false);
        }

        _graph.UpdateOverlay(steps[0], Color.yellow, false);
        _graph.UpdateOverlay(steps[steps.Length - 1], Color.yellow, true);
    }

    private void ClearGraph()
    {
        foreach (Vertex vertex in _graph.Vertices)
        {
            _graph.UpdateOverlay(vertex, false);
        }

        _graph._graphTexture.Apply();
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
        ResizeGrid(_unitsPerVertex);
        _graph = new Graph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertex / pixelsPerUnit);
        _vertexCount = _graph.Vertices.Length;
        _graphOverlayRenderer.sprite = Sprite.Create(_graph._graphTexture, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertex);

        _graph.onPathProcessed += OnPathProcessed;
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

                if (_visibilityGraphSelectedVertex != null)
                {
                    DrawVertex(_visibilityGraphSelectedVertex, Color.magenta);
                    DrawVertexConnections(_visibilityGraphSelectedVertex);
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

            DrawVertex(vertex);
        }
    }

    private void DrawVertex(Vertex vertex, Color? c = null)
    {
        Gizmos.color = c ?? Vertex.GetColorBasedOnTerrainType(vertex.TerrainType);

        Gizmos.DrawWireCube(vertex.Position, new Vector2(vertex.Size - vertex.Size * 0.05f, vertex.Size - vertex.Size * 0.05f));

        int rowIndex;
        int columnIndex;
        Graph.ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

        Handles.color = Color.black;
        Handles.Label(vertex.Position, vertex.Identifier.ToString());
        Vector2 columnRowHandlePosition = vertex.Position;
        columnRowHandlePosition.y -= vertex.Size / 6;
        Handles.Label(columnRowHandlePosition, $"{rowIndex},{columnIndex}");
    }
    
    private void DrawVertexConnections(Vertex vertex)
    {
        foreach (Vertex connection in vertex.GetConnectedVertices())
        {
            DrawVertex(connection, Color.red);
        }
    }

    private void OnDisable()
    {
        _mapLoader.onSpriteCreated -= CreateGridBasedOnSprite;
        _graph.onPathProcessed -= OnPathProcessed;
    }
}
