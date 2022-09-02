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

    public int openSetSize;
    public int closedSetSize;

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
        string s = "{\n";

        s += $"\tStart: [{startRow}, {startColumn}] ({startIdentifier})\n";

        s += $"\tGoal: [{goalRow}, {goalColumn}] ({goalIdentifier})\n";

        s += $"\tReached Goal: {reachedGoal}\n";

        s += $"\tDistance: {distance}\n";

        s += $"\tOpen Set Size: {openSetSize}\n";

        s += $"\tClosed Set Size A: {closedSetSize}\n";

        s += $"\tElapsed Time A: {elapsedTime}ms\n";

        s += "}";

        return s;
    }

    public static string JoinLogs(PathfindingLog a, PathfindingLog b)
    {
        string s = "{\n";

        if (a.startIdentifier != b.startIdentifier)
        {
            s += $"\tStart A: [{a.startRow}, {a.startColumn}] ({a.startIdentifier})\n";
            s += $"\tStart B: [{b.startRow}, {b.startColumn}] ({b.startIdentifier})\n";

        } else
        {
            s += $"\tStart: [{a.startRow}, {a.startColumn}] ({a.startIdentifier})\n";
        }

        if (a.goalIdentifier != b.goalIdentifier)
        {
            s += $"\tGoal A: [{a.goalRow}, {a.goalColumn}] ({a.goalIdentifier})\n";
            s += $"\tGoal B: [{b.goalRow}, {b.goalColumn}] ({b.goalIdentifier})\n";
        } else
        {
            s += $"\tGoal: [{a.goalRow}, {a.goalColumn}] ({a.goalIdentifier})\n";
        }

        if (a.reachedGoal != b.reachedGoal)
        {
            s += $"\tReached Goal A: {a.reachedGoal}\n";
            s += $"\tReached Goal B: {b.reachedGoal}\n";
        } else
        {
            s += $"\tReached Goal: {a.reachedGoal}\n";
        }

        if (a.distance != b.distance)
        {
            s += $"\tDistance A: {a.distance}\n";
            s += $"\tDistance B: {b.distance}\n";
        } else
        {
            s += $"\tDistance: {a.distance}\n";
        }

        s += $"\tOpen Set Size A: {a.openSetSize}\n";
        s += $"\tOpen Set Size B: {b.openSetSize}\n";

        s += $"\tClosed Set Size A: {a.closedSetSize}\n";
        s += $"\tClosed Set Size B: {b.closedSetSize}\n";

        s += $"\tElapsed Time A: {a.elapsedTime}ms\n";
        s += $"\tElapsed Time B: {b.elapsedTime}ms\n";

        s += $"\tTime Difference: {Mathf.Abs(a.elapsedTime - b.elapsedTime)}ms\n";

        s += "}";

        return s;
    }
}
