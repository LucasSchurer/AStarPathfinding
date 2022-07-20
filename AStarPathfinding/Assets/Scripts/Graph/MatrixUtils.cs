using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixUtils<T>
{
    public static List<T> GetNeighbours(T[][] grid, int rowCount, int columnCount, int rowIndex, int columnIndex, bool includeDiagonal = false)
    {
        List<T> neighbours = new List<T>();

        if (!IsIndexValid(rowCount, columnCount, rowIndex, columnIndex))
        {
            return neighbours;
        }

        // Checking the left neighbour index
        int neighbourIndex = rowCount - 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(grid[rowIndex][neighbourIndex]);
        }

        // Checking the right neighbour index
        neighbourIndex = rowCount + 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(grid[rowIndex][neighbourIndex]);
        }

        // Checking the up neighbour index
        neighbourIndex = columnCount - 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(grid[neighbourIndex][columnIndex]);
        }

        // Checking the down neighbour index
        neighbourIndex = columnCount + 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(grid[neighbourIndex][columnIndex]);
        }

        return neighbours;
    }

    public static bool IsIndexValid(int rowCount, int columnCount, int rowIndex, int columnIndex) => rowIndex >= 0 && rowIndex < rowCount && columnIndex >= 0 && columnIndex < columnCount;
}
