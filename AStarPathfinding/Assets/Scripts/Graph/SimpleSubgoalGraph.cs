using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgoalGraph : Graph
{
    public SubgoalGraph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize) : base(grid, rowCount, columnCount, vertexSize)
    {
    }

    protected override void GenerateGraph()
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

/*    private int Clearance(Vertex vertex, int verticalMovement, int horizontalMovement)
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
            }
            else
            {
                return distance - 1;
            }

            distance++;
        }
    }*/
}
