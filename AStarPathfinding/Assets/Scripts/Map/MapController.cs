using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class MapController : MonoBehaviour
{
    [SerializeField]
    private MapLoader _mapLoader;

    private Graph _fullGraph;
    private GraphDrawer _fullGraphDrawer;

    private VisibilityGraph _visibilityGraph;
    private GraphDrawer _visibilityGraphDrawer;

    private Pathfinding _fullGraphPathfinding;
    private VisibilityGraphPathfinding _visibilityGraphPathfinding;

    [SerializeField]
    private Enums.TerrainType[,] _grid;
    private int _gridRowCount;
    private int _gridColumnCount;

    [SerializeField]
    [Range(1, 50)]
    private int _unitsPerVertex = 1;

    [SerializeField]
    private SpriteRenderer _fullGraphOverlay;

    [SerializeField]
    private SpriteRenderer _visibilityGraphOverlay;

    private Vertex _startVertex;
    private Vertex _goalVertex;
    [SerializeField]
    private int _numberOfPoints = 50;

    private void Awake()
    {
        _mapLoader.onMapSpriteCreated += CreateGridBasedOnSprite;
    }

    private int GetNonWallIdentifier()
    {
        int aRow = UnityEngine.Random.Range(0, _gridRowCount);
        int aColumn = UnityEngine.Random.Range(0, _gridColumnCount);

        while (_grid[aRow, aColumn] == Enums.TerrainType.Wall)
        {
            aRow = UnityEngine.Random.Range(0, _gridRowCount);
            aColumn = UnityEngine.Random.Range(0, _gridColumnCount);
        }

        return Graph.CantorPairing(aRow, aColumn);
    }

    private IEnumerator FindNPoints(int n)
    {
        UnityEngine.Debug.Log("Pathfinding Started");

        Stopwatch sw = new Stopwatch();
        float elapsedTime = 0f;
        float visibilityGraphElapsedTime = 0f;
        float fullGraphElapsedTime = 0f;

        string path = "Assets/Output.txt";
        StreamWriter writer = new StreamWriter(path, false);

        for (int i = 0; i < n; i++)
        {
            PathfindingLog fullGraphLog = new PathfindingLog();
            PathfindingLog visibilityGraphLog = new PathfindingLog();

            int startIdentifier = GetNonWallIdentifier();
            int goalIdentifier = GetNonWallIdentifier();

            sw.Restart();
            sw.Start();

            _visibilityGraphPathfinding.FindPath(startIdentifier, goalIdentifier, ref visibilityGraphLog, false);
            _fullGraphPathfinding.FindPath(startIdentifier, goalIdentifier, ref fullGraphLog, false);

            sw.Stop();

            elapsedTime += sw.ElapsedMilliseconds;
            fullGraphElapsedTime += fullGraphLog.elapsedTime;
            visibilityGraphElapsedTime += visibilityGraphLog.elapsedTime;

            writer.WriteLine(PathfindingLog.JoinLogs(fullGraphLog, visibilityGraphLog) + ",");

            UnityEngine.Debug.Log(i);

            yield return null;
        }

        writer.WriteLine("-----------------------------------------------------------------");
        writer.WriteLine($"Map Used: {_mapLoader.ImageURL}");
        writer.WriteLine($"Elapsed Time: {elapsedTime}ms");
        writer.WriteLine($"Full Graph (A) Elapsed Time : {fullGraphElapsedTime}ms");
        writer.WriteLine($"Visibility Graph (B) Elapsed Time : {visibilityGraphElapsedTime}ms");
        writer.WriteLine($"Average Full Graph (B) Processing Time : {fullGraphElapsedTime/n}ms");
        writer.WriteLine($"Average Visibility Graph (B) Processing Time : {visibilityGraphElapsedTime/n}ms");
        writer.WriteLine("-----------------------------------------------------------------");

        writer.Close();

        UnityEngine.Debug.Log("Pathfinding Ended");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (_goalVertex != null)
                {
                    _fullGraphDrawer.DrawVertex(_goalVertex, false);
                }

                _goalVertex = _fullGraph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_startVertex != null)
                {
                    _fullGraphDrawer.DrawVertex(_goalVertex, Color.yellow);
                }
            } else
            {
                if (_startVertex != null)
                {
                    _fullGraphDrawer.DrawVertex(_startVertex, false);
                }

                _startVertex = _fullGraph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_startVertex != null)
                {
                    _fullGraphDrawer.DrawVertex(_startVertex, Color.yellow);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftShift))
        {
            StopAllCoroutines();

            if (_startVertex != null && _goalVertex != null)
            {
                StartCoroutine(_visibilityGraphPathfinding.FindPathCoroutine(_startVertex.Identifier, _goalVertex.Identifier));
            }
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            StopAllCoroutines();

            if (_startVertex != null && _goalVertex != null)
            {
                StartCoroutine(_fullGraphPathfinding.FindPathCoroutine(_startVertex.Identifier, _goalVertex.Identifier));
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(FindNPoints(_numberOfPoints));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _fullGraphOverlay.gameObject.SetActive(!_fullGraphOverlay.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _visibilityGraphOverlay.gameObject.SetActive(!_visibilityGraphOverlay.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (_fullGraphOverlay.gameObject.activeSelf)
            {
                _fullGraphDrawer.Clear();
                _fullGraphDrawer.Draw();
            }

            if (_visibilityGraphOverlay.gameObject.activeSelf)
            {
                _visibilityGraphDrawer.Clear();
                _visibilityGraphDrawer.Draw();
            }            
        }
    }    

    private void OnFullGraphPathProcessed(PathfindingLog log, Vertex[] steps)
    {
        _fullGraphDrawer.Draw();

        if (log.reachedGoal)
        {
            _fullGraphDrawer.DrawVertices(steps, Color.red, false);
            _fullGraphDrawer.DrawVertex(steps[0], Color.yellow, false);
            _fullGraphDrawer.DrawVertex(steps[steps.Length - 1], Color.yellow, true);
        }

        UnityEngine.Debug.Log(log.ToString());
    }

    private void OnVisibilityGraphPathProcessed(PathfindingLog log, Vertex[] steps)
    {
        _visibilityGraphDrawer.Draw();

        if (log.reachedGoal)
        {
            _visibilityGraphDrawer.DrawVertices(steps, Color.blue, false);
            _visibilityGraphDrawer.DrawVertex(steps[0], Color.yellow, false);
            _visibilityGraphDrawer.DrawVertex(steps[steps.Length - 1], Color.yellow, true);
        }

        UnityEngine.Debug.Log(log.ToString());
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

        InitializeFullGraph(pixelsPerUnit);
        InitializeVisibilityGraph(pixelsPerUnit);
    }

    public void InitializeFullGraph(float pixelsPerUnit)
    {
        _fullGraph = new Graph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertex / pixelsPerUnit);
        _fullGraphDrawer = new GraphDrawer(_fullGraph);
        _fullGraphOverlay.sprite = Sprite.Create(_fullGraphDrawer.texture2D, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertex);
        _fullGraphDrawer.Draw();

        _fullGraphPathfinding = new Pathfinding(_fullGraph);
        _fullGraphPathfinding.onPathProcessed += OnFullGraphPathProcessed;
    }

    public void InitializeVisibilityGraph(float pixelsPerUnit)
    {
        _visibilityGraph = new VisibilityGraph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertex / pixelsPerUnit);
        _visibilityGraphDrawer = new GraphDrawer(_visibilityGraph);
        _visibilityGraphOverlay.sprite = Sprite.Create(_visibilityGraphDrawer.texture2D, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertex);
        _visibilityGraphDrawer.Draw();

        _visibilityGraphPathfinding = new VisibilityGraphPathfinding(_visibilityGraph);
        _visibilityGraphPathfinding.onPathProcessed += OnVisibilityGraphPathProcessed;
    }

    /// <summary>
    /// Resize grid to fit more cells per map sprite pixel.
    /// </summary>
    /// <param name="verticesByUnit"></param>
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

    private void OnDisable()
    {
        _mapLoader.onMapSpriteCreated -= CreateGridBasedOnSprite;
        _fullGraphPathfinding.onPathProcessed -= OnFullGraphPathProcessed;
        _visibilityGraphPathfinding.onPathProcessed -= OnVisibilityGraphPathProcessed;
    }

    #region DEBUG

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (_grid != null)
            {
                if (_grid.Length < 10000)
                {
                    DrawGizmosGraph();
                }

                if (_startVertex != null)
                {
                    DrawGizmosVertex(_startVertex, Color.magenta);
                    DrawGizmosVertexConnections(_startVertex, Color.red);

                    if (_visibilityGraph.Vertices.ContainsKey(_startVertex.Identifier))
                    {
                        Vertex vertex = null;
                        _visibilityGraph.Vertices.TryGetValue(_startVertex.Identifier, out vertex);
                        DrawGizmosVertexConnections(vertex, Color.yellow);
                    }
                }
            }
        }
    }

    private void DrawGizmosGraph()
    {
        if (_fullGraph == null)
        {
            return;
        }

        if (_visibilityGraphOverlay.gameObject.activeSelf)
        {
            foreach (Vertex vertex in _visibilityGraph.Vertices.Values)
            {
                if (vertex == null)
                {
                    continue;
                }

                DrawGizmosVertex(vertex);
            }
        }

        if (_fullGraphOverlay.gameObject.activeSelf)
        {
            foreach (Vertex vertex in _fullGraph.Vertices.Values)
            {
                if (vertex == null)
                {
                    continue;
                }

                DrawGizmosVertex(vertex);
            }
        }
    }

    private void DrawGizmosVertex(Vertex vertex, Color? c = null)
    {
        Gizmos.color = c ?? Vertex.GetColorBasedOnTerrainType(vertex.TerrainType);

        Gizmos.DrawWireCube(vertex.Position, new Vector2(vertex.Size - vertex.Size * 0.05f, vertex.Size - vertex.Size * 0.05f));

        int rowIndex;
        int columnIndex;
        Graph.ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.black;
        Handles.Label(vertex.Position, vertex.Identifier.ToString(), style);
        Vector2 columnRowHandlePosition = vertex.Position;
        columnRowHandlePosition.y -= vertex.Size / 6;
        Handles.Label(columnRowHandlePosition, $"{rowIndex},{columnIndex}", style);

        columnRowHandlePosition.y += vertex.Size / 3f;
        Handles.Label(columnRowHandlePosition, $"{vertex.gCost},{vertex.hCost}, {vertex.fCost}", style);
    }
    
    private void DrawGizmosVertexConnections(Vertex vertex, Color color)
    {
        foreach (Vertex connection in vertex.GetConnectedVertices())
        {
            DrawGizmosVertex(connection, color);
        }
    }

    #endregion
}
