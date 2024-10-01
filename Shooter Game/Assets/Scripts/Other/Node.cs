using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string binaryValue {  get; set; }
    private Grid<Node> grid;
    public int x, y;

    public Node(Grid<Node> grid, int x, int y)
    {
        this.x = x; this.y = y;
        this.grid = grid;
    }
}
