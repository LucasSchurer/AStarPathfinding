using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubgoalGraph : Graph
{
    public SubgoalGraph(Enums.TerrainType[,] grid, int rowCount, int columnCount, float vertexSize) : base(grid, rowCount, columnCount, vertexSize)
    {
    }
}
