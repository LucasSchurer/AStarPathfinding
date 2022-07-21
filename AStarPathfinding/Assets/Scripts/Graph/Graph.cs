using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Graph
{
    public delegate void OnPathFound(long timeInMilliseconds);
    public OnPathFound onPathFound;

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

    public void UpdateOverlay(Vertex vertex, bool applyChangesToTexture = true)
    {
        UpdateOverlay(vertex.RowIndex, vertex.ColumnIndex, vertex.TerrainType, applyChangesToTexture);
    }

    private void UpdateOverlay(int row, int column, Enums.TerrainType terrainType, bool applyChangesToTexture = true)
    {
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
        int rowIndex;
        int columnIndex;
        ReverseCantorPairing(vertex.Identifier, out rowIndex, out columnIndex);

        if (!IsIndexValid(rowIndex, columnIndex))
        {
            return neighbours;
        }

        // Checking the left neighbour index
        int neighbourIndex = columnIndex - 1;
        if (IsIndexValid(rowIndex, neighbourIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the right neighbour index
        neighbourIndex = columnIndex + 1;
        if (IsIndexValid(rowIndex, neighbourIndex))
        {
            if (_vertices[rowIndex, neighbourIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[rowIndex, neighbourIndex]);
            }
        }

        // Checking the down neighbour index
        neighbourIndex = rowIndex - 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[neighbourIndex, columnIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
            }
        }

        // Checking the up neighbour index
        neighbourIndex = rowIndex + 1;
        if (IsIndexValid(neighbourIndex, columnIndex))
        {
            if (_vertices[neighbourIndex, columnIndex].TerrainType == Enums.TerrainType.Path)
            {
                neighbours.Add(_vertices[neighbourIndex, columnIndex]);
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


    /// <summary>
    /// A* Algorithm Implementation
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
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
            currentVertex = GetLowestCostVertexInList(openList);
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

    public IEnumerator GetPathFromSourceToTarget(Vertex source, Vertex target)
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

        Dictionary<int, Vertex> closedList = new Dictionary<int, Vertex>();
        List<Vertex> openList = new List<Vertex>();
        Vertex currentVertex;
        source.hCost = DistanceBetweenVertices(source, target);
        source.parent = null;
        target.parent = null;
        openList.Add(source);
        while (openList.Count > 0)
        {
            currentVertex = GetLowestCostVertexInList(openList);
            closedList.Add(currentVertex.Identifier, currentVertex);
            openList.Remove(currentVertex);

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
                if (closedList.ContainsKey(connectedVertex.Identifier))
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

    private Vertex GetLowestCostVertexInList(List<Vertex> list)
    {
        if (list.Count == 0)
        {
            return null;
        }

        Vertex lowestCostVertex = list[0];

        foreach (Vertex vertex in list)
        {
            if (lowestCostVertex.fCost == vertex.fCost)
            {
                if (lowestCostVertex.hCost > vertex.hCost)
                {
                    lowestCostVertex = vertex;
                }
            } else if (lowestCostVertex.fCost > vertex.fCost)
            {
                lowestCostVertex = vertex;
            }
        }

        return lowestCostVertex;
    }

    private int DistanceBetweenVertices(Vertex a, Vertex b)
    {
        int xDistance = Mathf.Abs(a.ColumnIndex - b.ColumnIndex);
        int yDistance = Mathf.Abs(a.RowIndex - b.RowIndex);

        return xDistance + yDistance;
    }
}
