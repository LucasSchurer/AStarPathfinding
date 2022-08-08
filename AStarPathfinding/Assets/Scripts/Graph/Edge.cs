using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    private Vertex _source;
    private Vertex _target;

    public Vertex Source => _source;
    public Vertex Target => _target;

    public Edge(Vertex source, Vertex target)
    {
        _source = source;
        _target = target;
    }
}
