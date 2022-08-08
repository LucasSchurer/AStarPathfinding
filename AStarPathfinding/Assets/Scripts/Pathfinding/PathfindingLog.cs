using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathfindingLog
{
    public int sourceRow;
    public int sourceColumn;
    public int sourceIdentifier;

    public int targetRow;
    public int targetColumn;
    public int targetIdentifier;

    public float timeSpent;
    public int distance;

    public bool foundPath;

    public override string ToString()
    {
        string s = $"Found Path: {foundPath} Source: {sourceIdentifier} = [{sourceRow}, {sourceColumn}] Target: {targetIdentifier} = [{targetRow}, {targetColumn}] Time: {timeSpent}ms Distance: {distance}";

        return s;
    }
}
