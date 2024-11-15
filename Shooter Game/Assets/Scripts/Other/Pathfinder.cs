using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Linq;
using System.Algorithms.Pathfinding;
using System.Threading;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using System;
using static UnityEngine.GraphicsBuffer;

public class Pathfinder
{
    Grid<Node> nodeMap = null;
    byte[,] byteMap = null;
    CustomPriorityQueue<Node> open = null;
    List<Node> closed = null;
    float travelWeight = 0;
    bool found = false;
    float gravity = System.Math.Abs(Physics2D.gravity.y);
    float speed = Player.Instance.speed;
    float jumpForce = Player.Instance.jumpForce;
    float cellSize = TerrainManager.Instance.cellSize;
    List<Vector2> landingPoints = new List<Vector2>();

    //public Pathfinder(Grid<Node> nodeMap, byte[,] byteMap)
    //{
    //    this.nodeMap = nodeMap;
    //    this.byteMap = byteMap;
    //}
    public List<Node> FindPath(/*Node startNode, Node endNode*/)
    {
        nodeMap = TerrainManager.Instance.ReturnWorldMap();
        byteMap = TerrainManager.Instance.ReturnMapAsByteGrid();

        if (closed != null)
            closed.Clear();
        else
            closed = new List<Node>();

        if (open != null)
            open.Clear();
        else
            open = new CustomPriorityQueue<Node>();

        for (int targetX = 0; targetX < byteMap.GetLength(0); targetX++)
        {
            int targetY = Enumerable.Range(0, byteMap.GetLength(1))
                .Select(i => byteMap[targetX, i])
                .ToList()
                .FindIndex(n => n == 1);

            landingPoints.Add(new Vector2Int(targetX, targetY));
        }

        found = false;

        float maxWorldJumpWidth = CalculateMaxJumpWidth();
        int maxGridJumpWidth = ConvertToGrid(maxWorldJumpWidth);
        float maxWorldJumpHeight = CalculateMaxJumpHeight();
        int maxGridJumpHeight = ConvertToGrid(maxWorldJumpHeight) - 1;
        Node startNode = nodeMap.GetGridObject(Player.Instance.transform.position);

        List<Node> neighbours = new List<Node>();
        neighbours = FindNeighbours(neighbours, startNode, 0, maxGridJumpWidth, maxGridJumpHeight);

        //foreach (Node node in neighbours)
        //{
        //    Debug.Log(node.x + ", " + node.y);
        //}

        //if (neighbours.Count == 0)
        //    Debug.Log("Empty");

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
        float maxHeight = (jumpForce * jumpForce) / (2 * gravity);
        return maxHeight;
    }
    private int ConvertToGrid(float n)
    {
        return (int)System.Math.Floor(n / cellSize);
    }
    private (float x1, float x2) QuadraticFormula(float a, float b, float y)
    {
        float c = -y;
        float discriminant = (b * b) - (4 * a * c);

        if (System.Math.Abs(discriminant) != discriminant)
            throw new System.Exception("No roots which is not possible");

        float x1 = (-b + (float)System.Math.Sqrt(discriminant)) / (2 * a);
        float x2 = (-b - (float)System.Math.Sqrt(discriminant)) / (2 * a);

        return (x1, x2);
    }
    private int PositiveNegativeOne(float n)
    {
        if (System.Math.Abs(n) == n)
            return 1;
        else
            return -1;
    }
    private int DetermineLoopStart(float n)
    {
        if (System.Math.Abs(n) == n)
            return 0;
        else
            return byteMap.GetLength(1);
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
    private List<Node> FindNeighbours(List<Node> neighbourList, Node startNode, int root0, int root1, int maxHeight)
    {
        List<Node> neighbours = neighbourList;
        float a, b;
        Vector2 r0 = new Vector2(root0, 0);
        Vector2 r1 = new Vector2(root1, 0);
        Vector2 turningPoint = new Vector2((root0 + root1) / 2, maxHeight);
        Vector2Int offset = new Vector2Int(startNode.x, startNode.y);
       
        //quadratics found in the form ax^2 + bx + c, but c = 0 here as root 0 will always be 0. a and b need to be calculated
        a = turningPoint.y / ((turningPoint.x - r0.x) * (turningPoint.x - r1.x));
        b = (turningPoint.y - (a * (turningPoint.x * turningPoint.x))) / (turningPoint.x);

        for (int i = 0; i != byteMap.GetLength(1); i++)
        {
            List<Vector2> pointsToAdd = new List<Vector2>();
            try
            {
                float x, y;

                y = i - offset.y;
                (float x1, float x2) solutions = QuadraticFormula(a, b, y);

                if (System.Math.Abs(root1) == root1)
                    x = (float)System.Math.Max((double)solutions.x1, (double)solutions.x2);
                else
                    x = (float)System.Math.Min((double)solutions.x1, (double)solutions.x2);

                int roundedDownX = (int)(System.Math.Floor((double)x + offset.x));
                int roundedUpX = (int)(System.Math.Ceiling((double)x) + offset.x);

                foreach (var point in landingPoints)
                {
                    if ((int)point.y == i)
                    {
                        if (System.Math.Abs(root1) != root1)
                        {
                            Debug.Log(roundedDownX + " ~ " + roundedUpX + ", " + i);

                            for (int j = -5; j < 5; j++)
                            {
                                try
                                {
                                    Debug.Log(landingPoints[roundedDownX + j] + " ~ " + landingPoints[roundedUpX + j] + ", " + i);
                                }
                                catch { }
                            }
                        }
                        if (((int)point.x >= roundedDownX) && ((int)point.x <= roundedUpX))
                        {
                            pointsToAdd.Add(point);
                        }
                    }
                }

                if (pointsToAdd.Count > 1)
                {
                    if (System.Math.Abs(root1) == root1)
                    {
                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
                    }
                    else
                    {
                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
                        pointsToAdd.Reverse();
                    }
                }
                if (pointsToAdd.Count > 0)
                {
                    Node node = nodeMap.GetGridObject((int)pointsToAdd[0].x, (int)pointsToAdd[0].y);
                    neighbours.Add(node);
                    Debug.Log(node);
                }
            }
            catch
            {
                break;
            }
        }

        if (System.Math.Abs(root1) == root1)
        {
            int negativeRoot1 = -root1;
            neighbours = FindNeighbours(neighbours, startNode, root0, negativeRoot1, maxHeight);
        }

        Debug.Log("checking left and right");
        return neighbours;
    }
}

