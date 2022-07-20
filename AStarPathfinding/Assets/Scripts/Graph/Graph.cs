using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph
{
    /* https://gist.github.com/GibsS/fdba8e3cdbd307652fc3c01336b32534 */
    public static int CantorPairing(int i, int j) => (((i + j) * (i + j + 1)) / 2) + j;
    public static void ReverseCantorPairing(int m, out int i, out int j)
    {
        int t = (int)Math.Floor((-1 + Math.Sqrt(1 + 8 * m)) * 0.5f);
        i = t * (t + 3) / 2 - m;
        j = m - t * (t + 1) / 2;
    }

    private Vertex[,] _vertices;
    public Vertex[,] Vertices => _vertices;

    public Graph(int[,] grid, int rowCount, int columnCount)
    {
        _vertices = new Vertex[rowCount, columnCount];

        GenerateGraph(grid, rowCount, columnCount);
    }

    private void GenerateGraph(int[,] grid, int rowCount, int columnCount)
    {
        CreateVertices(grid, rowCount, columnCount);
    }

    private void CreateVertices(int [,] grid, int rowCount, int columnCount)
    {
        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < columnCount; j++)
            {
                if (grid[i, j] == 0)
                {
                    _vertices[i, j] = new Vertex(CantorPairing(i, j), new Vector2(j * 1, i * 1));
                }
            }
        }
    }
}
