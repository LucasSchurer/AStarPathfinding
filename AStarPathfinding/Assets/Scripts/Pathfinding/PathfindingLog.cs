using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PathfindingLog
{
    public int startRow;
    public int startColumn;
    public int startIdentifier;

    public int goalRow;
    public int goalColumn;
    public int goalIdentifier;

    public float elapsedTime;
    public int distance;

    public bool reachedGoal;

    public void SetStartInfo(int identifier)
    {
        startIdentifier = identifier;
        Graph.ReverseCantorPairing(identifier, out startRow, out startColumn);
    }

    public void SetGoalInfo(int identifier)
    {
        goalIdentifier = identifier;
        Graph.ReverseCantorPairing(identifier, out goalRow, out goalColumn);
    }

    public override string ToString()
    {
        string s = $"Found Path: {reachedGoal} Source: {startIdentifier} = [{startRow}, {startColumn}] Target: {goalIdentifier} = [{goalRow}, {goalColumn}] Time: {elapsedTime}ms Distance: {distance}";

        return s;
    }
}
