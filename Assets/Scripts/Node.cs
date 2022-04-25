using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public List<Node> edges;
    public int x;
    public int y;

    public Node(int x, int y)
    {
        edges = new List<Node>();
        this.x = x;
        this.y = y;
    }

    public float DistanceTo(Node n)
    {
        return Vector2.Distance(
            new Vector2(x, y),
            new Vector2(n.x, n.y)
        );
    }
}
