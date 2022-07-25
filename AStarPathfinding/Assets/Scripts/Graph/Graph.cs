using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Graph
{
    public delegate void OnPathProcessed(Vertex[] steps);
    public OnPathProcessed onPathProcessed;

    /* https://gist.github.com/GibsS/fdba8e3cdbd307652fc3c01336b32534 */
    public static int CantorPairing(int i, int j) => (((i + j) * (i + j + 1)) / 2) + j;
    public static void ReverseCantorPairing(int m, out int i, out int j)
    {
        int t = (int)Math.Floor((-1 + Math.Sqrt(1 + 8 * m)) * 0.5f);
        i = t * (t + 3) / 2 - m;
        j = m - t * (t + 1) / 2;
    }

    public Texture2D _graphTexture;

    private int _rowCount;
    private int _columnCount;
    private float _vertexSize;

    private Vertex[,] _vertices;
    private Dictionary<int, Vertex> _visibilityGraph;
    public Vertex[,] Vertices => _vertices;

    public bool IsIndexValid(int rowIndex, int columnIndex) => rowIndex >= 0 && rowIndex < _rowCount && columnIndex >= 0 && columnIndex < _columnCount;

    public Graph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize)
    {
        _rowCount = rowCount;
        _columnCount = columnCount;
        _vertices = new Vertex[_rowCount, _columnCount];
        _vertexSize = vertexSize;
        _graphTexture = new Texture2D(_columnCount, _rowCount);
        _graphTexture.filterMode = FilterMode.Point;
        _graphTexture.Apply();

        GenerateGraph(grid);
    }

    private void GenerateGraph(Enums.TerrainType[,] grid)
    {
        CreateVertices(grid);
        CreateEdges();
        GenerateVisibilityGraph();
        /*GenerateSubgoalsEdges();*/
        _graphTexture.Apply();
    }

    private void CreateVertices(Enums.TerrainType[,] grid)
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                Vector2 vertexPosition;
                vertexPosition.x = j * _vertexSize + _vertexSize / 2;
                vertexPosition.y = i * _vertexSize + _vertexSize / 2;

                _vertices[i, j] = new Vertex(CantorPairing(i, j), i, j, vertexPosition, _vertexSize, grid[i, j]);
                UpdateOverlay(_vertices[i, j], false);
            }
        }

        _graphTexture.Apply();
    }

    private void GenerateVisibilityGraph()
    {
        _visibilityGraph = new Dictionary<int, Vertex>();

        foreach (Vertex vertex in _vertices)
        {
            if (vertex.TerrainType != Enums.TerrainType.Path)
            {
                continue;
            }
            

            for (int i = -1; i < 2; i+=2)
            {
                for (int j = -1; j < 2; j+=2)
                {
                    int neighbourRowIndex = vertex.RowIndex + i;
                    int neighbourColumnIndex = vertex.ColumnIndex + j;

                    if (IsIndexValid(neighbourRowIndex, neighbourColumnIndex))
                    {
                        if (_vertices[neighbourRowIndex, neighbourColumnIndex].TerrainType == Enums.TerrainType.Wall)
                        {
                            if (IsIndexValid(vertex.RowIndex + i, vertex.ColumnIndex))
                            {
                                if (_vertices[vertex.RowIndex + i, vertex.ColumnIndex].TerrainType == Enums.TerrainType.Path)
                                {
                                    if (IsIndexValid(vertex.RowIndex, vertex.ColumnIndex + j))
                                    {
                                        if (_vertices[vertex.RowIndex, vertex.ColumnIndex + j].TerrainType == Enums.TerrainType.Path)
                                        {
                                            Vertex subgoal = new Vertex(vertex.Identifier, vertex.RowIndex, vertex.ColumnIndex, vertex.Position, vertex.Size, Enums.TerrainType.Path);
                                            _visibilityGraph.Add(subgoal.Identifier, subgoal);
                                            UpdateOverlay(vertex, Color.blue, false);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        _graphTexture.Apply();
    }

    private void CreateEdges()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                if (_vertices[i, j] != null && _vertices[i, j].TerrainType != Enums.TerrainType.Wall)
                {
                    foreach (Vertex neighbour in GetVertexNeighbours(_vertices[i, j]))
                    {
                        _vertices[i, j].ConnectTo(neighbour, 1);
                    }
                }
            }
        }
    }

    private void GenerateSubgoalsEdges()
    {
        foreach (Vertex subgoal in _visibilityGraph.Values)
        {
            GenerateSubgoalEdges(subgoal);
        }
    }

    public void UpdateOverlay(Vertex vertex, bool applyChangesToTexture = true)
    {
        UpdateOverlay(vertex.RowIndex, vertex.ColumnIndex, vertex.TerrainType, applyChangesToTexture);
    }

    public void UpdateOverlay(int row, int column, Enums.TerrainType terrainType, bool applyChangesToTexture = true)
    {
        if (!IsIndexValid(row, column))
            return;

        _graphTexture.SetPixel(column, row, Vertex.GetColorBasedOnTerrainType(terrainType));

        if (applyChangesToTexture)
        {
            _graphTexture.Apply();
        }
    }

    public void UpdateOverlay(Vertex vertex, Color color, bool applyChangesToTexture = true)
    {
        _graphTexture.SetPixel(vertex.ColumnIndex, vertex.RowIndex, color);

        if (applyChangesToTexture)
        {
            _graphTexture.Apply();
        }
    }

    private List<Vertex> GetVertexNeighbours(Vertex vertex)
    {
        List<Vertex> neighbours = new List<Vertex>();

        if (!IsIndexValid(vertex.RowIndex, vertex.ColumnIndex))
        {
            return neighbours;
        }
        
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                Vertex neighbour;
                if (TryGetVertex(vertex.RowIndex + i, vertex.ColumnIndex + j, out neighbour))
                {
                    if (neighbour.TerrainType != Enums.TerrainType.Wall)
                    {
                        neighbours.Add(neighbour);
                    }
                }
            }
        }

        return neighbours;
    }

    public Vertex GetVertexOnPosition(Vector2 position)
    {
        int rowIndex = (int)(position.y / _vertexSize);
        int columnIndex = (int)(position.x / _vertexSize);

        if (IsIndexValid(rowIndex, columnIndex))
        {
            return _vertices[rowIndex, columnIndex];
        }

        return null;
    }

    public static void ResizeGrid(ref int[,] grid, int rowCount, int columnCount, int combinedRowsAmount, int combinedColumnsAmount)
    {
        int[,] newGrid = new int[rowCount / combinedRowsAmount, columnCount / combinedColumnsAmount];

        for (int i = 0; i < rowCount; i += combinedRowsAmount)
        {
            for (int j = 0; j < columnCount; j += combinedColumnsAmount)
            {
                int walkable = 0;

                for (int r = 0; r < combinedRowsAmount; r++)
                {
                    for (int c = 0; c < combinedColumnsAmount; c++)
                    {
                        int rowIndex = Mathf.Clamp(i + r, 0, rowCount - 1);
                        int columnIndex = Mathf.Clamp(j + c, 0, columnCount - 1);

                        walkable += grid[rowIndex, columnIndex];
                    }
                }

                newGrid[i / combinedRowsAmount, j / combinedColumnsAmount] = walkable;
            }
        }

        grid = newGrid;
    }

    public IEnumerator GetPathFromSourceToTargetWithOverlay(Vertex source, Vertex target)
    {
        foreach (Vertex vertex in _vertices)
        {
            UpdateOverlay(vertex, false);
        }

        UpdateOverlay(source, Color.yellow, false);
        UpdateOverlay(target, Color.yellow, false);
        _graphTexture.Apply();

        Stopwatch sw = new Stopwatch();
        sw.Start();

        List<Vertex> closedList = new List<Vertex>();
        List<Vertex> openList = new List<Vertex>();
        Vertex currentVertex;
        source.hCost = DistanceBetweenVertices(source, target);
        source.parent = null;
        target.parent = null;
        openList.Add(source);
        while (openList.Count > 0)
        {
            currentVertex = openList[0];
            closedList.Add(currentVertex);
            openList.Remove(currentVertex);

            UpdateOverlay(currentVertex, Color.magenta, false);

            if (currentVertex == target)
            {
                sw.Stop();
                UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");
                currentVertex = currentVertex.parent;

                while (currentVertex != null)
                {
                    UpdateOverlay(currentVertex, Color.green, false);
                    currentVertex = currentVertex.parent;
                }

                UpdateOverlay(source, Color.yellow, false);
                UpdateOverlay(target, Color.yellow, false);
                _graphTexture.Apply();

                break;
            }

            foreach (Vertex connectedVertex in currentVertex.GetConnectedVertices())
            {
                if (closedList.Contains(connectedVertex))
                {
                    continue;
                }

                int movementCostToConnectedVertex = currentVertex.gCost + DistanceBetweenVertices(currentVertex, connectedVertex);
                if (!openList.Contains(connectedVertex) || movementCostToConnectedVertex < connectedVertex.gCost)
                {
                    connectedVertex.gCost = movementCostToConnectedVertex;
                    connectedVertex.hCost = DistanceBetweenVertices(connectedVertex, target);
                    connectedVertex.parent = currentVertex;

                    if (!openList.Contains(connectedVertex))
                    {
                        openList.Add(connectedVertex);

                        UpdateOverlay(connectedVertex, Color.blue, false);
                    }
                }
            }

            _graphTexture.Apply();

            yield return new WaitForSeconds(0);

            UpdateOverlay(currentVertex, Color.red, true);
        }

        yield return null;
    }

    /// <summary>
    /// A* Algorithm Implementation to find a path between two vertices
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    public IEnumerator FindPath(Vertex source, Vertex target)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        HashSet<Vertex> closedSet = new HashSet<Vertex>();
        Heap<Vertex> openSet = new Heap<Vertex>(_vertices.Length);
        Vertex[] steps = new Vertex[] { source };
        Vertex currentVertex;

        source.hCost = DistanceBetweenVertices(source, target);
        source.parent = null;
        openSet.Add(source);

        while (openSet.Count > 0)
        {
            currentVertex = openSet.RemoveFirst();
            closedSet.Add(currentVertex);

            if (currentVertex == target)
            {
                sw.Stop();
                UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");

                steps = RetraceSteps(currentVertex);

                break;
            }

            foreach (Vertex connectedVertex in currentVertex.GetConnectedVertices())
            {
                if (closedSet.Contains(connectedVertex))
                {
                    continue;
                }

                int movementCostToConnectedVertex = currentVertex.gCost + DistanceBetweenVertices(currentVertex, connectedVertex);
                if (!openSet.Contains(connectedVertex) || movementCostToConnectedVertex < connectedVertex.gCost)
                {
                    connectedVertex.gCost = movementCostToConnectedVertex;
                    connectedVertex.hCost = DistanceBetweenVertices(connectedVertex, target);
                    connectedVertex.parent = currentVertex;

                    if (!openSet.Contains(connectedVertex))
                    {
                        openSet.Add(connectedVertex);
                    }
                }
            }

            if (sw.ElapsedMilliseconds > 10000)
            {
                break;
            }
        }

        onPathProcessed?.Invoke(steps);

        yield return null;
    }

    public IEnumerator FindPathStepByStep(Vertex source, Vertex target)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        HashSet<Vertex> closedSet = new HashSet<Vertex>();
        Heap<Vertex> openSet = new Heap<Vertex>(_vertices.Length);

        Vertex currentVertex;
        source.hCost = DistanceBetweenVertices(source, target);
        source.parent = null;
        target.parent = null;
        openSet.Add(source);
        while (openSet.Count > 0)
        {
            currentVertex = openSet.RemoveFirst();
            closedSet.Add(currentVertex);

            if (currentVertex == target)
            {
                sw.Stop();
                UnityEngine.Debug.Log(sw.ElapsedMilliseconds + "ms");
                currentVertex = currentVertex.parent;

                while (currentVertex != null)
                {
                    UpdateOverlay(currentVertex, Color.green, false);
                    currentVertex = currentVertex.parent;
                }

                UpdateOverlay(source, Color.yellow, false);
                UpdateOverlay(target, Color.yellow, false);
                _graphTexture.Apply();

                break;
            }

            foreach (Vertex connectedVertex in currentVertex.GetConnectedVertices())
            {
                if (closedSet.Contains(connectedVertex))
                {
                    continue;
                }

                int movementCostToConnectedVertex = currentVertex.gCost + DistanceBetweenVertices(currentVertex, connectedVertex);
                if (!openSet.Contains(connectedVertex) || movementCostToConnectedVertex < connectedVertex.gCost)
                {
                    connectedVertex.gCost = movementCostToConnectedVertex;
                    connectedVertex.hCost = DistanceBetweenVertices(connectedVertex, target);
                    connectedVertex.parent = currentVertex;

                    if (!openSet.Contains(connectedVertex))
                    {
                        openSet.Add(connectedVertex);
                    }
                }
            }

            if (sw.ElapsedMilliseconds > 10000)
            {
                break;
            }
        }

        yield return null;
    }

    /// <summary>
    /// Return a array with all the vertices used to reach a target vertex.
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    private Vertex[] RetraceSteps(Vertex target)
    {
        List<Vertex> steps = new List<Vertex>();

        while (target != null)
        {
            steps.Add(target);
            target = target.parent;
        }

        steps.Reverse();
        return steps.ToArray();
    }

    private int DistanceBetweenVertices(Vertex a, Vertex b)
    {
        int xDistance = Mathf.Abs(a.ColumnIndex - b.ColumnIndex);
        int yDistance = Mathf.Abs(a.RowIndex - b.RowIndex);

        return 14 * Mathf.Min(xDistance, yDistance) + 10 * Mathf.Abs(xDistance - yDistance);
    }
    private bool TryGetVertex(int rowIndex, int columnIndex, out Vertex vertex)
    {
        vertex = null;

        if (IsIndexValid(rowIndex, columnIndex))
        {
            vertex = _vertices[rowIndex, columnIndex];
            return true;
        }

        return false;
    }

    public bool TryGetSubgoal(int identifier, out Vertex subgoal)
    {
        return _visibilityGraph.TryGetValue(identifier, out subgoal);
    }

    private void GenerateSubgoalEdges(Vertex subgoal)
    {
    }

    public void PrintClearance(Vertex vertex)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                UnityEngine.Debug.Log($"Direction: {i} {j} Clearance: {Clearance(vertex, i, j)}");
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="cardinal"></param>
    /// <param name="diagonal"></param>
    /// <returns></returns>
    private int Clearance(Vertex vertex, int verticalMovement, int horizontalMovement)
    {
        int distance = 1;

        while (true)
        {
            int currentRow = vertex.RowIndex + (verticalMovement * distance);
            int currentColumn = vertex.ColumnIndex + (horizontalMovement * distance);

            if (_visibilityGraph.ContainsKey(CantorPairing(currentRow, currentColumn)))
            {
                return distance;
            }

            if (IsIndexValid(currentRow, currentColumn))
            {
                if (_vertices[currentRow, currentColumn].TerrainType == Enums.TerrainType.Wall)
                {
                    return distance - 1;
                }
            } else
            {
                return distance - 1;
            }

            distance++;
        }
    }
}
