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
    public float executionTime = 1f;

    [SerializeField]
    private MapLoader _mapLoader;

    [SerializeField]
    private Graph _graph;
    private GraphDrawer _graphDrawer;

    private VisibilityGraph _ssg;
    private GraphDrawer _ssgDrawer;

    private Pathfinding _pathfinding;
    private SubgoalGraphPathfinding _ssgPathfinding;

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

    [SerializeField]
    private SpriteRenderer _ssgOverlayRenderer;

    private Vertex _sourceVertex;
    private Vertex _targetVertex;
    private Vertex _ssgSourceVertex;
    private Vertex _ssgTargetVertex;
    [SerializeField]
    private int _numberOfPoints = 2;

    private void Awake()
    {
        _mapLoader.onSpriteCreated += CreateGridBasedOnSprite;
    }

    private IEnumerator FindNPoints(int n)
    {
        UnityEngine.Debug.Log("Pathfinding Started");

        Stopwatch sw = new Stopwatch();
        float timeElapsed = 0f;
        float ssgTimeElapsed = 0f;
        float normalTimeElapsed = 0f;

        string path = "Assets/Output.txt";
        StreamWriter writer = new StreamWriter(path, false);

        for (int i = 0; i < n; i++)
        {
            PathfindingLog normalLog = new PathfindingLog();
            PathfindingLog ssgLog = new PathfindingLog();

            int aRow = UnityEngine.Random.Range(0, _gridRowCount);
            int aColumn = UnityEngine.Random.Range(0, _gridColumnCount);

            while (_grid[aRow, aColumn] == Enums.TerrainType.Wall)
            {
                aRow = UnityEngine.Random.Range(0, _gridRowCount);
                aColumn = UnityEngine.Random.Range(0, _gridColumnCount);
            }

            int bRow = UnityEngine.Random.Range(0, _gridRowCount);
            int bColumn = UnityEngine.Random.Range(0, _gridColumnCount);

            while (_grid[bRow, bColumn] == Enums.TerrainType.Wall)
            {
                bRow = UnityEngine.Random.Range(0, _gridRowCount);
                bColumn = UnityEngine.Random.Range(0, _gridColumnCount);
            }

            normalLog.startRow = ssgLog.startRow = aRow;
            normalLog.startColumn = ssgLog.startColumn = aColumn;
            normalLog.startIdentifier = ssgLog.startIdentifier = Graph.CantorPairing(aRow, aColumn);

            normalLog.goalRow = ssgLog.goalRow = bRow;
            normalLog.goalColumn = ssgLog.goalColumn = bColumn;
            normalLog.goalIdentifier = ssgLog.goalIdentifier = Graph.CantorPairing(bRow, bColumn);

            sw.Restart();
            sw.Start();

            _ssgPathfinding.FindPath(normalLog.startIdentifier, normalLog.goalIdentifier, ref ssgLog, false);
            _pathfinding.FindPath(normalLog.startIdentifier, normalLog.goalIdentifier, ref normalLog, false);

            sw.Stop();

            timeElapsed += sw.ElapsedMilliseconds;
            normalTimeElapsed += normalLog.elapsedTime;
            ssgTimeElapsed += ssgLog.elapsedTime;

            string equalDistance = normalLog.distance != ssgLog.distance ? "Different Distances" : "";

            string output = equalDistance + " " + normalLog.ToString() + " " + ssgLog.ToString();

            writer.WriteLine(output);

            UnityEngine.Debug.Log(i);

            yield return null;
        }

        writer.WriteLine("Elapsed Time: " + timeElapsed + "ms");
        writer.WriteLine("SSG elapsed Time: " + ssgTimeElapsed + "ms");
        writer.WriteLine("Normal elapsed Time: " + normalTimeElapsed + "ms");

        writer.Close();

        UnityEngine.Debug.Log("Pathfinding Ended");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (_targetVertex != null)
                {
                    _graphDrawer.DrawVertex(_targetVertex, false);
                }

                _targetVertex = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_sourceVertex != null)
                {
                    _graphDrawer.DrawVertex(_targetVertex, Color.yellow);
                }

            } else if (Input.GetKey(KeyCode.LeftAlt))
            {
                if (_ssgSourceVertex != null)
                {
                    _ssgDrawer.DrawVertex(_ssgSourceVertex, false);
                }

                _ssgSourceVertex = _ssg.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_ssgSourceVertex != null)
                {
                    _ssgDrawer.DrawVertex(_ssgSourceVertex, Color.yellow);
                }
            } else if (Input.GetKey(KeyCode.LeftControl))
            {
                if (_ssgTargetVertex != null)
                {
                    _ssgDrawer.DrawVertex(_ssgTargetVertex, false);
                }

                _ssgTargetVertex = _ssg.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_ssgSourceVertex != null)
                {
                    _ssgDrawer.DrawVertex(_ssgTargetVertex, Color.yellow);
                }
            } else
            {
                if (_sourceVertex != null)
                {
                    _graphDrawer.DrawVertex(_sourceVertex, false);
                }

                _sourceVertex = _graph.GetVertexOnPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                if (_sourceVertex != null)
                {
                    _graphDrawer.DrawVertex(_sourceVertex, Color.yellow);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            StopAllCoroutines();

            if (_sourceVertex != null && _targetVertex != null)
            {
                StartCoroutine(_pathfinding.FindPathCoroutine(_sourceVertex.Identifier, _targetVertex.Identifier));
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && Input.GetKey(KeyCode.LeftShift))
        {
            StopAllCoroutines();

            if (_sourceVertex != null && _targetVertex != null)
            {
                StartCoroutine(_ssgPathfinding.FindPathCoroutine(_sourceVertex.Identifier, _targetVertex.Identifier));
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            StartCoroutine(FindNPoints(_numberOfPoints));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _graphOverlayRenderer.gameObject.SetActive(!_graphOverlayRenderer.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _ssgOverlayRenderer.gameObject.SetActive(!_ssgOverlayRenderer.gameObject.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            if (_graphOverlayRenderer.gameObject.activeSelf)
            {
                _graphDrawer.Draw();
            }

            if (_ssgOverlayRenderer.gameObject.activeSelf)
            {
                _ssgDrawer.Clear();
                _ssgDrawer.Draw();
            }            
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            
        }
    }    

    private void OnPathProcessed(PathfindingLog log, Vertex[] steps)
    {
        _graphDrawer.Draw();

        if (log.reachedGoal)
        {
            _graphDrawer.DrawVertices(steps, Color.red, false);
            _graphDrawer.DrawVertex(steps[0], Color.yellow, false);
            _graphDrawer.DrawVertex(steps[steps.Length - 1], Color.yellow, true);
        }

        UnityEngine.Debug.Log(log.ToString());
    }

    private void OnSSGPathProcessed(PathfindingLog log, Vertex[] steps)
    {
        _ssgDrawer.Draw();

        if (log.reachedGoal)
        {
            _ssgDrawer.DrawVertices(steps, Color.blue, false);
            _ssgDrawer.DrawVertex(steps[0], Color.yellow, false);
            _ssgDrawer.DrawVertex(steps[steps.Length - 1], Color.yellow, true);
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
        
        
        _graph = new Graph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertex / pixelsPerUnit);
        _graphDrawer = new GraphDrawer(_graph);
        _vertexCount = _graph.Vertices.Count;
        _graphOverlayRenderer.sprite = Sprite.Create(_graphDrawer.texture2D, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertex);
        _graphDrawer.Draw();

        _ssg = new VisibilityGraph(_grid, _gridRowCount, _gridColumnCount, _unitsPerVertex / pixelsPerUnit);
        _ssgDrawer = new GraphDrawer(_ssg);
        _ssgOverlayRenderer.sprite = Sprite.Create(_ssgDrawer.texture2D, new Rect(0, 0, _gridColumnCount, _gridRowCount), transform.position, pixelsPerUnit / _unitsPerVertex);
        _ssgDrawer.Draw();

        _pathfinding = new Pathfinding(_graph);
        _pathfinding.onPathProcessed += OnPathProcessed;

        _ssgPathfinding = new SubgoalGraphPathfinding(_ssg);
        _ssgPathfinding.onPathProcessed += OnSSGPathProcessed;
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
        foreach (Vertex vertex in _graph.Vertices.Values)
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

            UnityEngine.Debug.Log(s);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (_grid != null)
            {
                if (_grid.Length < 10000)
                {
                    DrawGraph();
                }

                if (_sourceVertex != null)
                {
                    DrawVertex(_sourceVertex, Color.magenta);
                    DrawVertexConnections(_sourceVertex);

                    if (_ssg.Vertices.ContainsKey(_sourceVertex.Identifier))
                    {
                        Vertex vertex = null;
                        _ssg.Vertices.TryGetValue(_sourceVertex.Identifier, out vertex);
                        DrawVertexConnectionsAux(vertex);
                    }
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

        if (_ssgOverlayRenderer.gameObject.activeSelf)
        {
            foreach (Vertex vertex in _ssg.Vertices.Values)
            {
                if (vertex == null)
                {
                    continue;
                }

                DrawVertex(vertex);
            }
        }

        if (_graphOverlayRenderer.gameObject.activeSelf)
        {
            foreach (Vertex vertex in _graph.Vertices.Values)
            {
                if (vertex == null)
                {
                    continue;
                }

                DrawVertex(vertex);
            }
        }
    }

    private void DrawVertex(Vertex vertex, Color? c = null)
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
    
    private void DrawVertexConnections(Vertex vertex)
    {
        foreach (Vertex connection in vertex.GetConnectedVertices())
        {
            DrawVertex(connection, Color.red);
        }
    }

    private void DrawVertexConnectionsAux(Vertex vertex)
    {
        foreach (Vertex connection in vertex.GetConnectedVertices())
        {
            DrawVertex(connection, Color.yellow);
        }
    }

    private void OnDisable()
    {
        _mapLoader.onSpriteCreated -= CreateGridBasedOnSprite;
        _pathfinding.onPathProcessed -= OnPathProcessed;
        _ssgPathfinding.onPathProcessed -= OnSSGPathProcessed;
    }
}
