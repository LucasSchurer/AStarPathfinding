using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgoalGraph : Graph
{
    public SubgoalGraph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize) : base(grid, rowCount, columnCount, vertexSize)
    {
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
            CreateVertexEdges(vertex);
        }
    }

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
                    CreateTwoMovementHReachableEdges(vertex, i, j, clearance);
                }
            }
        }
    }

    private void CreateTwoMovementHReachableEdges(Vertex vertex, int verticalMovement, int horizontalMovement, int diagonalClearance)
    {
        if (vertex.Identifier == 73)
        {
            int aaa = 1;
        }

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
        int possibleIdentifier = CantorPairing(connectionRow, connectionColumn);

        if (vertex.Identifier == possibleIdentifier)
        {
            return;
        }

        Vertex possibleVertex;
        if (_vertices.TryGetValue(possibleIdentifier, out possibleVertex))
        {
            vertex.ConnectTo(possibleVertex, Pathfinding.DistanceBetweenVertices(vertex, possibleVertex));
            possibleVertex.ConnectTo(vertex, Pathfinding.DistanceBetweenVertices(vertex, possibleVertex));
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
                        TryToCreateVertex(rowIndex, columnIndex, out _);
                    }
                }
            }
        }
    }

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

    public void AddVertex(int rowIndex, int columnIndex, out Vertex vertex)
    {
        TryToCreateVertex(rowIndex, columnIndex, out vertex);
    }

    public void RemoveVertex(Vertex vertex)
    {
        foreach (Vertex connectedVertex in vertex.GetConnectedVertices())
        {
            connectedVertex.RemoveConnectedVertex(vertex);
        }

        _vertices.Remove(vertex.Identifier);
    }

    public bool TryToInsertSubgoalOnPosition(Vector2 position)
    {
        int rowIndex;
        int columnIndex;

        if (TryToGetIndexesOnPosition(position, out rowIndex, out columnIndex))
        {
            AddVertex(rowIndex, columnIndex, out _);
            return true;
        }

        return false;
    }
}
