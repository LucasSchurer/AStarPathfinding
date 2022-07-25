using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgoalGraph : Graph
{
    public SubgoalGraph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize) : base(grid, rowCount, columnCount, vertexSize)
    {
        isSSG = true;
    }

    protected override void CreateVertices()
    {
        for (int i = 0; i < _rowCount; i++)
        {
            for (int j = 0; j < _columnCount; j++)
            {
                if (_grid[i, j] == Enums.TerrainType.Wall)
                {
                    continue;
                }

                AddCorners(i, j);
            }
        }
    }

    protected override void CreateEdges()
    {
        foreach (Vertex vertex in _vertices.Values)
        {
            AddDirectHReachableSubgoals(vertex);
        }
    }

    private void AddDirectHReachableSubgoals(Vertex vertex)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (!IsIndexValid(vertex.RowIndex + i, vertex.ColumnIndex + j))
                {
                    continue;
                }

                // Search for direct-h-reachable subgoals that can be reached in only one movement
                int clearance = Clearance(vertex.RowIndex, vertex.ColumnIndex, i, j);
                TryToCreateConnection(vertex, vertex.RowIndex + (clearance * i), vertex.ColumnIndex + (clearance * j));

                // If is a diagonal, search for subgoals that can be reached in two movements
                if (i != 0 && j != 0)
                {
                    int maxDiagonalDistance = clearance;
                    int maxVerticalDistance = Clearance(vertex.RowIndex, vertex.ColumnIndex, i, 0);
                    int maxHorizontalDistance = Clearance(vertex.RowIndex, vertex.ColumnIndex, 0, j);

                    for (int k = 1; k < maxDiagonalDistance; k++)
                    {
                        int currentRowIndex = vertex.RowIndex + (i * k);
                        int currentColumnIndex = vertex.ColumnIndex + (j * k);

                        if (!IsIndexValid(currentRowIndex, currentColumnIndex))
                        {
                            continue;
                        }

                        int verticalClearance = Clearance(currentRowIndex, currentColumnIndex, i, 0);
                        int horizontalClearance = Clearance(currentRowIndex, currentColumnIndex, 0, j);

                        if (verticalClearance < maxVerticalDistance)
                        {
                            maxVerticalDistance = verticalClearance;
                            TryToCreateConnection(vertex, currentRowIndex + (verticalClearance * i), currentColumnIndex);
                        }

                        if (horizontalClearance < maxHorizontalDistance)
                        {
                            maxHorizontalDistance = horizontalClearance;
                            TryToCreateConnection(vertex, currentRowIndex, currentColumnIndex + (horizontalClearance * j));
                        }
                    }
                }
            }
        }
    }

    private void TryToCreateConnection(Vertex vertex, int connectionRow, int connectionColumn)
    {
        int possibleIdentifier = CantorPairing(connectionRow, connectionColumn);

        Vertex possibleVertex;
        if (_vertices.TryGetValue(possibleIdentifier, out possibleVertex))
        {
            vertex.ConnectTo(possibleVertex, Pathfinding.DistanceBetweenVertices(vertex, possibleVertex));
        }
    }

    private void AddCorners(int rowIndex, int columnIndex)
    {
        for (int i = -1; i < 2; i += 2)
        {
            for (int j = -1; j < 2; j += 2)
            {
                int cornerRowIndex = rowIndex + i;
                int cornerColumnIndex = columnIndex + j;

                if (IsIndexValid(cornerRowIndex, cornerColumnIndex) && _grid[cornerRowIndex, cornerColumnIndex] == Enums.TerrainType.Wall)
                {
                    if (IsIndexValid(rowIndex + i, columnIndex) && IsIndexValid(rowIndex, columnIndex + j) &&
                        _grid[rowIndex + i, columnIndex] == Enums.TerrainType.Path && _grid[rowIndex, columnIndex + j] == Enums.TerrainType.Path)
                    {
                        TryCreateVertex(rowIndex, columnIndex, out _);
                    }
                }
            }
        }
    }

    private int Clearance(int rowIndex, int columnIndex, int verticalMovement, int horizontalMovement)
    {
        int distance = 1;

        while (true)
        {
            int currentRow = rowIndex + (verticalMovement * distance);
            int currentColumn = columnIndex + (horizontalMovement * distance);

            if (_vertices.ContainsKey(CantorPairing(currentRow, currentColumn)))
            {
                return distance;
            }

            if (IsIndexValid(currentRow, currentColumn))
            {
                if (_grid[currentRow, currentColumn] == Enums.TerrainType.Wall)
                {
                    return distance - 1;
                }
            }
            else
            {
                return distance - 1;
            }

            distance++;
        }
    }

    public void AddVertex(int rowIndex, int columnIndex)
    {
        Vertex createdVertex;
        if (TryCreateVertex(rowIndex, columnIndex, out createdVertex))
        {
            AddDirectHReachableSubgoals(createdVertex);
        }
    }

    public bool TryToInsertSubgoalOnPosition(Vector2 position)
    {
        int rowIndex;
        int columnIndex;

        if (TryToGetIndexesOnPosition(position, out rowIndex, out columnIndex))
        {
            AddVertex(rowIndex, columnIndex);
            return true;
        }

        return false;
    }

    public void DrawHReachableSubgoals(Vertex vertex)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                if (!IsIndexValid(vertex.RowIndex + i, vertex.ColumnIndex + j))
                {
                    continue;
                }

                // Search for direct-h-reachable subgoals that can be reached in only one movement
                int clearance = Clearance(vertex.RowIndex, vertex.ColumnIndex, i, j);
                graphDrawer.DrawPixel(vertex.RowIndex + (clearance * i), vertex.ColumnIndex + (clearance * j), Color.magenta, false);

                // If is a diagonal, search for subgoals that can be reached in two movements
                if (i != 0 && j != 0)
                {
                    int maxDiagonalDistance = clearance;
                    int maxVerticalDistance = Clearance(vertex.RowIndex, vertex.ColumnIndex, i, 0);
                    int maxHorizontalDistance = Clearance(vertex.RowIndex, vertex.ColumnIndex, 0, j);

                    for (int k = 1; k < maxDiagonalDistance; k++)
                    {
                        int currentRowIndex = vertex.RowIndex + (i * k);
                        int currentColumnIndex = vertex.ColumnIndex + (j * k);

                        if (!IsIndexValid(currentRowIndex, currentColumnIndex))
                        {
                            continue;
                        }

                        int verticalClearance = Clearance(currentRowIndex, currentColumnIndex, i, 0);
                        int horizontalClearance = Clearance(currentRowIndex, currentColumnIndex, 0, j);

                        if (verticalClearance < maxVerticalDistance)
                        {
                            maxVerticalDistance = verticalClearance;
                            graphDrawer.DrawPixel(currentRowIndex + (horizontalClearance * j), currentColumnIndex, Color.magenta, false);
                        }

                        if (horizontalClearance < maxHorizontalDistance)
                        {
                            maxHorizontalDistance = horizontalClearance;
                            graphDrawer.DrawPixel(currentRowIndex, currentColumnIndex + (horizontalClearance * j),  Color.magenta, false);
                        }
                    }
                }
            }
        }

        graphDrawer.Apply();
    }

    private int DrawClearance(int rowIndex, int columnIndex, int verticalMovement, int horizontalMovement)
    {
        int distance = 1;

        while (true)
        {
            int currentRow = rowIndex + (verticalMovement * distance);
            int currentColumn = columnIndex + (horizontalMovement * distance);

            graphDrawer.DrawPixel(currentRow, currentColumn, Color.red, false);

            if (_vertices.ContainsKey(CantorPairing(currentRow, currentColumn)))
            {
                return distance;
            }

            if (IsIndexValid(currentRow, currentColumn))
            {
                if (_grid[currentRow, currentColumn] == Enums.TerrainType.Wall)
                {
                    return distance - 1;
                }
            }
            else
            {
                return distance - 1;
            }

            distance++;
        }
    }

    public void PrintClearance(Vertex vertex)
    {
        string clearance = "";

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                string verticalDirection = i == -1 ? "Down" : i == 1 ? "Up" : "";
                string horizontalDirection = j == -1 ? "Left" : j == 1 ? "Right" : "";

                clearance += $"Direction: {verticalDirection} {horizontalDirection} Clearance: {Clearance(vertex.RowIndex, vertex.ColumnIndex, i, j)}\n";
            }
        }

        Debug.Log(clearance);
    }
}
