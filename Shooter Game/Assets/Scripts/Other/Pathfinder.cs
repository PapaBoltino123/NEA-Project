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
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class Pathfinder
{
    Grid<Node> nodeMap = null;
    byte[,] byteMap = null;
    CustomPriorityQueue<Node> open = null;
    List<Node> closed = null;
    float travelWeight = 0;
    bool found = false;
    float gravity = System.Math.Abs(Physics2D.gravity.y);
    float speed = 0;
    float jumpForce = 0;
    float cellSize = TerrainManager.Instance.cellSize;
    List<Vector2> landingPoints = new List<Vector2>();

    public Pathfinder(Grid<Node> nodeMap, byte[,] byteMap, float speed, float jumpForce)
    {
        this.nodeMap = nodeMap;
        this.byteMap = byteMap;
        this.speed = speed;
        this.jumpForce = jumpForce;
    }
    public List<Node> FindPath(int startX, int startY, int endX, int endY)
    {
        Node startNode = nodeMap.GetGridObject(startX, startY);
        Node endNode = nodeMap.GetGridObject(endX, endY);
        List<Node> neighbours = new List<Node>();

        if (closed != null) //initialisation of key aspects of the pathfinding
            closed.Clear();
        else
            closed = new List<Node>();

        if (open != null)
            open.Clear();
        else
            open = new CustomPriorityQueue<Node>();

        for (int targetX = 0; targetX < byteMap.GetLength(0); targetX++) //obtains the coordinates of the nodes where the path could pass through using linq
        {
            int targetY = Enumerable.Range(0, byteMap.GetLength(1))
                .Select(i => byteMap[targetX, i])
                .ToList()
                .FindIndex(n => n == 1);

            landingPoints.Add(new Vector2Int(targetX, targetY));
        }

        for (int x = 0; x < nodeMap.Width; x++)
        {
            for (int y = 0; y < nodeMap.Height; y++)
            {
                Node node = nodeMap.GetGridObject(x, y); //sets the initial values of all the nodes in the grid
                node.gCost = int.MaxValue;
                node.CalculateFCost();
                node.parentNode = null;
                node.UpdateNode();
            }
        }

        float maxWorldJumpWidth = CalculateMaxJumpWidth();
        int maxGridJumpWidth = ConvertToGrid(maxWorldJumpWidth); //calculates the maximum distance that can be jumped at the zombie's jumpforce and speed and converts into the grid units
        float maxWorldJumpHeight = CalculateMaxJumpHeight();
        int maxGridJumpHeight = ConvertToGrid(maxWorldJumpHeight) - 1; //calculates the maximum height that can be jumped at the zombie's jumpforce and speed and converts into the grid units

        startNode.gCost = 0; //initial distance travelled is 0
        startNode.hCost = CalculateHeurisicCost(startNode, endNode); //calculates heuristic cost to goal using euclidean distance and weights
        startNode.CalculateFCost(); //calculates final cost
        open.Enqueue(startNode, startNode.fCost);

        while (open.Count > 0)
        {
            Node currentNode = open.Dequeue(); //obtains the node with the lowest fCost

            if (currentNode == endNode) //if the node is the target node a path has been found
                found = true;

            closed.Add(currentNode); //adds node to closed list

            neighbours = FindNeighbours(neighbours, currentNode, 0, maxGridJumpWidth, maxGridJumpHeight); //obtains neighbours of current node
            foreach (var node in neighbours)
            {
                if (closed.Contains(node))
                    continue;

                float tentativeGCost = currentNode.gCost + node.tentativeGCost;

                if (tentativeGCost < node.gCost)
                {
                    node.parentNode = currentNode;
                    node.gCost = tentativeGCost;
                    node.hCost = CalculateHeurisicCost(node, endNode);
                    node.CalculateFCost();
                }

                if (!open.Contains(node))
                {
                    open.Enqueue(node, node.fCost);
                }
            }

            if (found == true)
            {
                List<Node> path = new List<Node>();
                path.Add(endNode);
                Node cNode = endNode;

                while (cNode.parentNode != null)
                {
                    path.Add(cNode.parentNode);
                    cNode = cNode.parentNode;
                }

                return path;
            }
        }

        return null;
    }
    private float CalculateMaxJumpWidth()
    {
        float maxDistance = (2 * speed * jumpForce) / gravity; //formula for calculating max distance jumped
        return maxDistance;
    }
    private float CalculateMaxJumpHeight()
    {
        float maxHeight = (jumpForce * jumpForce) / (2 * gravity); //formula for calculating max height jumped
        return maxHeight;
    }
    private int ConvertToGrid(float n)
    {
        return (int)System.Math.Floor(n / cellSize); //the conversion to grid units which is what the algorithm uses to find path
    }
    private (float x1, float x2) QuadraticFormula(float a, float b, float y) //the quadratic formula
    {
        float c = -y; //as c = 0 for all of the necessary jumps, c is the negative of the y input
        float discriminant = (b * b) - (4 * a * c); //calculates discriminant

        if (System.Math.Abs(discriminant) != discriminant)
            throw new System.Exception("No roots which is not possible"); //if no real roots, throw exception

        float x1 = (-b + (float)System.Math.Sqrt(discriminant)) / (2 * a); //the first solution 
        float x2 = (-b - (float)System.Math.Sqrt(discriminant)) / (2 * a); //the second solution

        return (x1, x2);
    }
    private int CheckDirection(float n)
    {
        if (System.Math.Abs(n) == n) //if the inputted float is the same as it's absolute value, return 1 else return -1
            return 1;
        else
            return -1;
    }
    private float CalculateHeurisicCost(Node start, Node end)
    {
        int walkingWeight = 1;
        int jumpingWeight = 3;

        int costSquared = (walkingWeight * (int)System.Math.Pow(end.x - start.x, 2)) + (jumpingWeight * (int)System.Math.Pow(end.y - start.y, 2));
        return (float)System.Math.Sqrt(costSquared);
    }
    private float CalculateGCost(Node start, Node end, NodeMovementType type)
    {
        float distance = MathF.Sqrt(MathF.Pow(end.x - start.x, 2) + MathF.Pow(end.y - start.y, 2));
        int penalty = 1;

        if (type == NodeMovementType.JUMP)
            penalty = 3;

        return distance * penalty;
    }
    private List<Node> FindNeighbours(List<Node> neighbourList, Node startNode, int root0, int root1, int maxHeight)
    {
        List<Node> neighbours = neighbourList; //sets all iniial values and initialises solution nodes list
        List<Node> solutionNodes = new List<Node>();
        float a, b;
        Vector2 r0 = new Vector2(root0, 0);
        Vector2 r1 = new Vector2(root1, 0);

        Vector2 turningPoint = new Vector2((root0 + root1) / 2, maxHeight); //the turning point is at the max height and inbetween the two roots
        Vector2Int offset = new Vector2Int(startNode.x, startNode.y); //the offset is how far 0, 0 is in relation to the starting nodes so that the parabola jump curve generated can have c = 0
                                                                      //and then be converted to grid coords
       
        a = turningPoint.y / ((turningPoint.x - r0.x) * (turningPoint.x - r1.x)); //as y = a(x - root 0)(x - root1), a = y / (x - root0) * (x - root1)
        b = (turningPoint.y - (a * (turningPoint.x * turningPoint.x))) / (turningPoint.x); //subbing in a into y = ax^2 + bx, b = (y - ax^2) / x

        for (int i = 0; i != byteMap.GetLength(1); i++)
        {
            List<Vector2> pointsToAdd = new List<Vector2>(); //points to add to solution nodes
            try
            {
                float x, y;

                y = i - offset.y; //gets y in curve coords rather than grid coords
                (float x1, float x2) solutions = QuadraticFormula(a, b, y); //obtains xn solutions

                if (System.Math.Abs(root1) == root1) //if the max distance is positive, return the biggest soluion
                    x = (float)System.Math.Max((double)solutions.x1, (double)solutions.x2);
                else //else return the smallest solution
                    x = (float)System.Math.Min((double)solutions.x1, (double)solutions.x2);

                int roundedDownX = (int)(System.Math.Floor((double)x + offset.x)); //calculates tthe upper and lower bounds of x in grid coords
                int roundedUpX = (int)(System.Math.Ceiling((double)x) + offset.x);

                foreach (var point in landingPoints)
                {
                    if ((int)point.y == i)
                    {
                        if (((int)point.x >= roundedDownX) && ((int)point.x <= roundedUpX)) //if the solution is within the bounds add it to points to add
                        {
                            pointsToAdd.Add(point);
                        }
                    }
                }

                if (pointsToAdd.Count > 1) //a bug occured that resulted in more solutions than intended
                {
                    if (System.Math.Abs(root1) == root1) //if max distance is positive sort nodes smallest x value to biggest
                    {
                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
                    }
                    else
                    {
                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x)); //else sort nodes largest x value to smallest
                        pointsToAdd.Reverse();
                    }
                }
                if (pointsToAdd.Count > 0)
                {
                    Node node = nodeMap.GetGridObject((int)pointsToAdd[0].x, (int)pointsToAdd[0].y);
                    solutionNodes.Add(node); //add the node to solution nodes
                }
            }
            catch
            {
                break; //if no real roots, break the loop
            }
        }

        solutionNodes = solutionNodes.OrderByDescending(g => g.y).ToList(); //as the parabola has the potential to intersect multiple landing points, sort the nodes from biggest y value to smallest 
        Node toAdd = null;

        try
        {
            toAdd = solutionNodes[1]; //i found a bug where nodes at corners where the terrain drops can sometimes result in the node not being grounded, so finding the next intersecion after that if possible was the fix here
        }
        catch
        {
            try
            {
                toAdd = solutionNodes[1]; //of course, if this is not possible add the only node in the list
            }
            catch { } //there are no soluion nodes at this point
        }

        toAdd.type = NodeMovementType.JUMP;
        toAdd.tentativeGCost = CalculateGCost(startNode, toAdd, toAdd.type);
        neighbours.Add(toAdd);
        neighbours.RemoveAll(n => n == null);

        if (System.Math.Abs(root1) == root1) //if the max distance is posittive set it to negative
        {
            int negativeRoot1 = -root1;
            neighbours = FindNeighbours(neighbours, startNode, root0, negativeRoot1, maxHeight); //use of recursion for calculating jumping negative values
        }

        neighbours = CheckHorizontals(neighbours, CheckDirection(root1), startNode); //check nodes when moving one left and right 

        return neighbours; //return neighbours list
    }
    private List<Node> CheckHorizontals(List<Node> neighboursList, int direction, Node startNode)
    {
        List<Node> neighbours = neighboursList;
        Node nextNode = nodeMap.GetGridObject(startNode.x + direction, startNode.y); //gets the node either left or righ depending on positive or negative direction

        int groundIndex = Enumerable.Range(0, byteMap.GetLength(1))
            .Select(i => byteMap[nextNode.x, i])
            .ToList()
            .FindIndex(n => n == 1); //finds y coordinates of the landing node at the next node x value

        if (groundIndex > startNode.y) //if terrain increases then walking is not possible so skip
            goto CHECK_END;
        else
        {
            List<Node> solutionNodes = new List<Node>();
            for (int i = 0; i != byteMap.GetLength(1); i++)
            {
                List<Vector2> pointsToAdd = new List<Vector2>();
                try
                {
                    float timeSquared = (2 * (nextNode.y - i)) / gravity; //calculates the time taken to fall from a point for each y value on terrain
                    if (timeSquared < 0) //cannot root a negative
                        break;

                    float time = MathF.Sqrt(timeSquared); //calculates time and substitues into the horizontal motion formula (x = current x coord + horizontal velocity * time)
                    float x = nextNode.x + (speed * direction * time);

                    int roundedDownX = (int)(System.Math.Floor((double)x)); //calculates bounds
                    int roundedUpX = (int)(System.Math.Ceiling((double)x));

                    foreach (var point in landingPoints)
                    {
                        if ((int)point.y == i)
                        {
                            if (((int)point.x >= roundedDownX) && ((int)point.x <= roundedUpX))
                            {
                                pointsToAdd.Add(point); //if the solution is within the bounds add it to points to add
                            }
                        }
                    }

                    if (pointsToAdd.Count > 1) //this follows the exact process for finding landing nodes when jumping
                    {
                        if (System.Math.Abs(direction) == direction)
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
                        solutionNodes.Add(node);
                    }
                }
                catch
                {
                    break;
                }
            }

            solutionNodes = solutionNodes.OrderByDescending(g => g.y).ToList();
            Node toAdd = null;

            try
            {
                toAdd = solutionNodes[1]; //i found a bug where nodes at corners where the terrain drops can sometimes result in the node not being grounded, so finding the next intersecion after that if possible was the fix here
            }
            catch
            {
                try
                {
                    toAdd = solutionNodes[1]; //of course, if this is not possible add the only node in the list
                }
                catch { } //there are no soluion nodes at this point
            }

            toAdd.type = NodeMovementType.WALK;
            toAdd.tentativeGCost = toAdd.tentativeGCost = CalculateGCost(startNode, toAdd, toAdd.type);
            neighbours.Add(toAdd);
            neighbours.RemoveAll(n => n == null);
        }

        CHECK_END:
            return neighbours;
    }
}

