using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Linq;
using System.Algorithms.Pathfinding;
using System.Threading;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

public class Pathfinder
{
    Grid<Node> nodeMap = null;
    byte[,] byteMap = null;
    CustomPriorityQueue<Node> open = null;
    List<Node> closed = null;
    float travelWeight = 0;
    bool found = false;
    float gravity = System.Math.Abs(Physics2D.gravity.y);
    int speed = 3;
    int jumpForce = 4;

    //public Pathfinder(Grid<Node> nodeMap, byte[,] byteMap)
    //{
    //    this.nodeMap = nodeMap;
    //    this.byteMap = byteMap;
    //}
    public List<Node> FindPath(/*Node startNode, Node endNode*/)
    {
        //closed.Clear();
        //open.Clear();
        found = false;

        Debug.Log(CalculateMaxJumpHeight());
        Debug.Log(CalculateMaxJumpWidth());

        return null;
    }
    //private List<Node> FindNeighbours(Node currentNode)
    //{
    //    List<Node> neighbours = new List<Node>();


    //}
    private float CalculateMaxJumpWidth()
    {
        float maxDistance = (2 * speed * jumpForce) / gravity;
        return maxDistance;
    }
    private float CalculateMaxJumpHeight()
    {
        float maxHeight = jumpForce / (2 * gravity);
        return maxHeight;
    }
    //private List<Node> ObtainNeighbours(List<Node> neighbourList, int root0, int root1, int maxHeight, byte[,] byteMap, Grid<Node> nodeMap)
    //{
    //    //quadratics found in the form ax^2 + bx + c, but c = 0 here as root 0 will always be 0. a and b need to be calculated
    //    //float a, b;
    //    //Vector2 worldP0 = new Vector2(root0, 0);
    //    //Vector2 worldP1 = new Vector2((root0 + root1) / 2, maxHeight);
    //    //Vector2 worldP2 = new Vector2(root1, 0);

    //    //a = p1.y / ((p1.x - p0.x) * (p1.x - p2.x));
    //    //b = (p1.y - (a * (p1.x * p1.x))) / (p1.x);

    //    //int x = Mathf.FloorToInt(root1 / TerrainManager.Instance.cellSize);
    //    //int y = Enumerable.Range(0, byteMap.GetLength(0))
    //    //        .Select(i => byteMap[x, i])
    //    //        .ToList()
    //    //        .FindIndex(n => n == 0);

    //    //if (y == node.y)
    //    //{
    //    //    neighbours.Add(nodeMap.GetGridObject(x, y));
    //    //}
    //    return neighbours;
    //}
}

