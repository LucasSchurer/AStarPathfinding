using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityGraph : Graph
{
    public VisibilityGraph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize) : base(grid, rowCount, columnCount, vertexSize)
    {
    }

    protected override void CreateVertices()
    {
        for (int i = 0; i < _gridRowCount; i++)
        {
            for (int j = 0; j < _gridColumnCount; j++)
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
            CreateVertexEdges(vertex);
        }
    }

    /// <summary>
    /// Create vertex edges based on 
    /// SSG rules.
    /// </summary>
    /// <param name="vertex"></param>
    public void CreateVertexEdges(Vertex vertex)
    {
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue;
                }

                int clearance = Clearance(vertex.RowIndex, vertex.ColumnIndex, i, j);

                TryToCreateConnection(vertex, vertex.RowIndex + clearance * i, vertex.ColumnIndex + clearance * j);

                if (i != 0 && j != 0)
                {
                    CreateTwoMovementEdges(vertex, i, j, clearance);
                }
            }
        }
    }

    /// <summary>
    /// Create edges that can be reached in two movements.
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="verticalMovement"></param>
    /// <param name="horizontalMovement"></param>
    /// <param name="diagonalClearance"></param>
    private void CreateTwoMovementEdges(Vertex vertex, int verticalMovement, int horizontalMovement, int diagonalClearance)
    {
        int verticalClearance = Clearance(vertex.RowIndex, vertex.ColumnIndex, verticalMovement, 0);
        int horizontalClearance = Clearance(vertex.RowIndex, vertex.ColumnIndex, 0, horizontalMovement);

        for (int i = 1; i <= diagonalClearance; i++)
        {
            int currentRow = vertex.RowIndex + (i * verticalMovement);
            int currentColumn = vertex.ColumnIndex + (i * horizontalMovement);

            int tempVerticalClearance = Clearance(currentRow, currentColumn, verticalMovement, 0);
            int tempHorizontalClearance = Clearance(currentRow, currentColumn, 0, horizontalMovement);

            if (tempVerticalClearance < verticalClearance)
            {
                verticalClearance = tempVerticalClearance;
            }

            if (tempHorizontalClearance < horizontalClearance)
            {
                horizontalClearance = tempHorizontalClearance;
            }

            int possibleRow = currentRow + verticalClearance * verticalMovement;
            int possibleColumn = currentColumn + horizontalClearance * horizontalMovement;
            TryToCreateConnection(vertex, possibleRow, currentColumn);
            TryToCreateConnection(vertex, currentRow, possibleColumn);
        }
    }

    private void TryToCreateConnection(Vertex vertex, int connectionRow, int connectionColumn)
    {
        Vertex possibleVertex = GetVertex(connectionRow, connectionColumn);
        if (possibleVertex != null)
        {
            vertex.ConnectTo(possibleVertex);
            possibleVertex.ConnectTo(vertex);
        }
    }

    /// <summary>
    /// Create a vertex for each corner of the grid.
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
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
                        CreateVertex(rowIndex, columnIndex);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Returns the max distance that a cell can "move" towards a direction.
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="columnIndex"></param>
    /// <param name="verticalMovement"></param>
    /// <param name="horizontalMovement"></param>
    /// <returns></returns>
    private int Clearance(int rowIndex, int columnIndex, int verticalMovement, int horizontalMovement)
    {
        int distance = 1;

        if (verticalMovement == 0 && horizontalMovement == 0)
        {
            return distance - 1;
        }

        int currentRowIndex = rowIndex + verticalMovement;
        int currentColumnIndex = columnIndex + horizontalMovement;

        while (true)
        {
            if (IsIndexValid(currentRowIndex, currentColumnIndex) && _grid[currentRowIndex, currentColumnIndex] != Enums.TerrainType.Wall)
            {
                // If the clearance is being cast diagonally, it should check if the cell is reachable (there is no wall/subgoal on cardinal directions)
                if (verticalMovement != 0 && horizontalMovement != 0)
                {
                    int nextRowIndex = currentRowIndex - verticalMovement;
                    int nextColumnIndex = currentColumnIndex - horizontalMovement;
                    int blockedWall = 0;
                    int blockedGoal = 0;
                    
                    if (IsIndexValid(nextRowIndex, currentColumnIndex))
                    {
                        if (_grid[nextRowIndex, currentColumnIndex] == Enums.TerrainType.Wall)
                        {
                            blockedWall++;
                        }
                        else if (_vertices.ContainsKey(CantorPairing(nextRowIndex, currentColumnIndex)))
                        {
                            blockedGoal++;
                        }
                    } else
                    {
                        blockedWall++;
                    }

                    if (IsIndexValid(currentRowIndex, nextColumnIndex))
                    {
                        if (_grid[currentRowIndex, nextColumnIndex] == Enums.TerrainType.Wall)
                        {
                            blockedWall++;
                        }
                        else if (_vertices.ContainsKey(CantorPairing(currentRowIndex, nextColumnIndex)))
                        {
                            blockedGoal++;
                        }
                    }
                    else
                    {
                        blockedWall++;
                    }

                    if ((blockedWall == 1 && blockedGoal == 1) || blockedWall == 2)
                    {
                        return distance - 1;
                    }
                }

                // If the current cell is already on the dictionary, the cell is a subgoal and the clearance should return the distance.
                if (_vertices.ContainsKey(CantorPairing(currentRowIndex, currentColumnIndex)))
                {
                    return distance;
                }
            } else
            {
                return distance - 1;
            }

            currentRowIndex += verticalMovement;
            currentColumnIndex += horizontalMovement;
            distance++;
        }
    }
}
