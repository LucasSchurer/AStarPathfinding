using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixUtils<T>
{
    public static List<Tuple<int, int>> GetNeighboursIndexes(T[,] grid, int rowCount, int columnCount, int rowIndex, int columnIndex, bool includeDiagonal = false)
    {
        List<Tuple<int, int>> neighbours = new List<Tuple<int, int>>();

        if (!IsIndexValid(rowCount, columnCount, rowIndex, columnIndex))
        {
            return neighbours;
        }

        // Checking the left neighbour index
        int neighbourIndex = columnIndex - 1;
        if (IsIndexValid(rowCount, columnCount, rowIndex, neighbourIndex))
        {
            neighbours.Add(new Tuple<int, int>(rowIndex, neighbourIndex));
        }

        // Checking the right neighbour index
        neighbourIndex = columnIndex + 1;
        if (IsIndexValid(rowCount, columnCount, rowIndex, neighbourIndex))
        {
            neighbours.Add(new Tuple<int, int>(rowIndex, neighbourIndex));
        }

        // Checking the down neighbour index
        neighbourIndex = rowIndex - 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(new Tuple<int, int>(neighbourIndex, columnIndex));
        }

        // Checking the right neighbour index
        neighbourIndex = rowIndex + 1;
        if (IsIndexValid(rowCount, columnCount, neighbourIndex, columnIndex))
        {
            neighbours.Add(new Tuple<int, int>(neighbourIndex, columnIndex));
        }

        return neighbours;
    }

    public static bool IsIndexValid(int rowCount, int columnCount, int rowIndex, int columnIndex) => rowIndex >= 0 && rowIndex < rowCount && columnIndex >= 0 && columnIndex < columnCount;
}
