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
using UnityEngine.UIElements;
using UnityEngine.WSA;
using System.ComponentModel.Design;

public class Pathfinder : MonoBehaviour
{
    private Grid<Node> nodeMap = null;
    private byte[,] byteMap = null;
    private Thread pathfindingThread;

    CustomPriorityQueue<Node> open = null;
    List<Node> closed = null;
    float travelWeight = 0;
    bool found = false;
    float gravity = System.Math.Abs(Physics2D.gravity.y);
    float speed = 0;
    float jumpForce = 0;
    float cellSize = TerrainManager.Instance.cellSize;
    List<Vector2> landingPoints = new List<Vector2>();
    private static SynchronizationContext mainThreadContext;

    public Pathfinder(Grid<Node> nodeMap, byte[,] byteMap, float speed, float jumpForce)
    {
        this.nodeMap = nodeMap;
        this.byteMap = byteMap;
        this.speed = speed;
        this.jumpForce = jumpForce;
        mainThreadContext = SynchronizationContext.Current;
    }
    public void FindPath(int startX, int startY, int endX, int endY, Zombie.PathFoundCallback pathFoundCallback)
    {
        pathfindingThread = new Thread(() =>
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

               neighbours = FindNeighbours(currentNode);

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
                        node.UpdateNode();
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
                    path.Reverse();
                    mainThreadContext.Post(_ => pathFoundCallback(path), null);
                }
            }
        });
        pathfindingThread.Start();
    }
    public float CalculateMaxJumpWidth()
    {
        float maxDistance = (2 * speed * jumpForce) / gravity; //formula for calculating max distance jumped
        return maxDistance;
    }
    public float CalculateMaxJumpHeight()
    {
        float maxHeight = (jumpForce * jumpForce) / (2 * gravity); //formula for calculating max height jumped
        return maxHeight;
    }
    public int ConvertToGrid(float n)
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
    //public List<Node> FindNeighbours(List<Node> neighbourList, Node startNode, int root0, int root1, int maxHeight)
    //{
    //    List<Node> neighbours = neighbourList; //sets all iniial values and initialises solution nodes list
    //    List<Node> solutionNodes = new List<Node>();
    //    float a, b;
    //    Vector2 r0 = new Vector2(root0 + startNode.x, startNode.y);
    //    Vector2 r1 = new Vector2(root1 + startNode.x, startNode.y);

    //    Vector2 turningPoint = new Vector2((r0.x + r1.x) / 2, maxHeight + startNode.y); //the turning point is at the max height and inbetween the two roots

    //    Debug.Log(turningPoint);

    //    for (int i = 0; i != byteMap.GetLength(1); i++)
    //    {
    //        List<Vector2> pointsToAdd = new List<Vector2>();
    //        try
    //        {
    //            float timeSquared = (2 * (turningPoint.y - i)) / gravity; //calculates the time taken to fall from a point for each y value on terrain
    //            if (timeSquared < 0) //cannot root a negative
    //                break;

    //            float time = MathF.Sqrt(timeSquared); //calculates time and substitues into the horizontal motion formula (x = current x coord + horizontal velocity * time)
    //            float x = turningPoint.x + (speed * CheckDirection(root1) * time);

    //            int roundedDownX = (int)(System.Math.Floor((double)x)); //calculates bounds
    //            int roundedUpX = (int)(System.Math.Ceiling((double)x));

    //            foreach (var point in landingPoints)
    //            {
    //                if ((int)point.y == i)
    //                {
    //                    if (((int)point.x >= roundedDownX) && ((int)point.x <= roundedUpX))
    //                    {
    //                        pointsToAdd.Add(point); //if the solution is within the bounds add it to points to add
    //                    }
    //                }
    //            }

    //            if (pointsToAdd.Count > 1)
    //            {
    //                if (System.Math.Abs(CheckDirection(root1)) == CheckDirection(root1))
    //                {
    //                    pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
    //                }
    //                else
    //                {
    //                    pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
    //                    pointsToAdd.Reverse();
    //                }
    //            }
    //            if (pointsToAdd.Count > 0)
    //            {
    //                Node node = nodeMap.GetGridObject((int)pointsToAdd[0].x, (int)pointsToAdd[0].y);
    //                solutionNodes.Add(node);
    //            }
    //        }
    //        catch
    //        {
    //            break;
    //        }
    //    }


    //    solutionNodes = solutionNodes.OrderByDescending(g => g.y).ToList(); //as the parabola has the potential to intersect multiple landing points, sort the nodes from biggest y value to smallest 

    //    try
    //    {
    //        neighbours.Add(solutionNodes[1]); //i found a bug where nodes at corners where the terrain drops can sometimes result in the node not being grounded, so finding the next intersecion after that if possible was the fix here
    //    }
    //    catch
    //    {
    //        try
    //        {
    //            neighbours.Add(solutionNodes[0]); //of course, if this is not possible add the only node in the list
    //        }
    //        catch { } //there are no soluion nodes at this point
    //    }

    //    try
    //    {
    //        neighbours[neighbours.Count - 1].type = NodeMovementType.JUMP;
    //        neighbours[neighbours.Count - 1].tentativeGCost = CalculateGCost(startNode, neighbours[neighbours.Count - 1], NodeMovementType.JUMP);
    //    }
    //    catch { }

    //    if (System.Math.Abs(root1) == root1) //if the max distance is posittive set it to negative
    //    {
    //        int negativeRoot1 = -root1;
    //        neighbours = FindNeighbours(neighbours, startNode, root0, negativeRoot1, maxHeight); //use of recursion for calculating jumping negative values
    //    }

    //    neighbours = CheckHorizontals(neighbours, CheckDirection(root1), startNode); //check nodes when moving one left and right 

    //    return neighbours; //return neighbours list
    //}
    //private List<Node> CheckHorizontals(List<Node> neighboursList, int direction, Node startNode)
    //{
    //    List<Node> neighbours = neighboursList;
    //    Node nextNode = nodeMap.GetGridObject(startNode.x + direction, startNode.y); //gets the node either left or righ depending on positive or negative direction

    //    int groundIndex = Enumerable.Range(0, byteMap.GetLength(1))
    //        .Select(i => byteMap[nextNode.x, i])
    //        .ToList()
    //        .FindIndex(n => n == 1); //finds y coordinates of the landing node at the next node x value

    //    if (groundIndex > startNode.y) //if terrain increases then walking is not possible so skip
    //        goto CHECK_END;
    //    else
    //    {
    //        List<Node> solutionNodes = new List<Node>();
    //        for (int i = 0; i != byteMap.GetLength(1); i++)
    //        {
    //            List<Vector2> pointsToAdd = new List<Vector2>();
    //            try
    //            {
    //                float timeSquared = (2 * (nextNode.y - i)) / gravity; //calculates the time taken to fall from a point for each y value on terrain
    //                if (timeSquared < 0) //cannot root a negative
    //                    break;

    //                float time = MathF.Sqrt(timeSquared); //calculates time and substitues into the horizontal motion formula (x = current x coord + horizontal velocity * time)
    //                float x = nextNode.x + (speed * direction * time);

    //                int roundedDownX = (int)(System.Math.Floor((double)x)); //calculates bounds
    //                int roundedUpX = (int)(System.Math.Ceiling((double)x));

    //                foreach (var point in landingPoints)
    //                {
    //                    if ((int)point.y == i)
    //                    {
    //                        if (((int)point.x >= roundedDownX) && ((int)point.x <= roundedUpX))
    //                        {
    //                            pointsToAdd.Add(point); //if the solution is within the bounds add it to points to add
    //                        }
    //                    }
    //                }

    //                if (pointsToAdd.Count > 1) //this follows the exact process for finding landing nodes when jumping
    //                {
    //                    if (System.Math.Abs(direction) == direction)
    //                    {
    //                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
    //                    }
    //                    else
    //                    {
    //                        pointsToAdd.Sort((a, b) => a.x.CompareTo(b.x));
    //                        pointsToAdd.Reverse();
    //                    }
    //                }
    //                if (pointsToAdd.Count > 0)
    //                {
    //                    Node node = nodeMap.GetGridObject((int)pointsToAdd[0].x, (int)pointsToAdd[0].y);
    //                    solutionNodes.Add(node);
    //                }
    //            }
    //            catch
    //            {
    //                break;
    //            }
    //        }

    //        solutionNodes = solutionNodes.OrderByDescending(g => g.y).ToList();

    //        try
    //        {
    //            neighbours.Add(solutionNodes[1]); //i found a bug where nodes at corners where the terrain drops can sometimes result in the node not being grounded, so finding the next intersecion after that if possible was the fix here
    //        }
    //        catch
    //        {
    //            try
    //            {
    //                neighbours.Add(solutionNodes[0]); //of course, if this is not possible add the only node in the list
    //            }
    //            catch { } //there are no soluion nodes at this point
    //        }

    //        try
    //        {
    //            neighbours[neighbours.Count - 1].type = NodeMovementType.WALK;
    //            neighbours[neighbours.Count - 1].tentativeGCost = CalculateGCost(startNode, neighbours[neighbours.Count - 1], NodeMovementType.WALK);
    //        }
    //        catch { }
    //    }

    //    CHECK_END:
    //        return neighbours;
    //}
    private List<Node> FindNeighbours(Node currentNode)
    {
        List<Node> neighbours = new List<Node>();
        int currentX = currentNode.x;
        int currentY = currentNode.y;

        // Maximum jump dimensions
        float maxWorldJumpWidth = CalculateMaxJumpWidth();
        int maxGridJumpWidth = ConvertToGrid(maxWorldJumpWidth);
        float maxWorldJumpHeight = CalculateMaxJumpHeight();
        int maxGridJumpHeight = ConvertToGrid(maxWorldJumpHeight);

        // Try moving left and right, simulating fall after walking horizontally
        for (int direction = -1; direction <= 1; direction += 2)
        {
            int targetX = currentX + direction;
            int targetY = currentY;

            // Ensure within grid bounds
            if (targetX < 0 || targetX >= byteMap.GetLength(0))
                continue;

            // Check if the position directly to the side is walkable
            if (byteMap[targetX, targetY] == 1)
            {
                // Add horizontal neighbor
                neighbours.Add(nodeMap.GetGridObject(targetX, targetY));
                neighbours.Last().type = NodeMovementType.WALK;
                neighbours.Last().tentativeGCost = CalculateGCost(currentNode, neighbours.Last(), NodeMovementType.WALK);

                // Simulate fall if there’s a void below the walked-to position
                if (targetY > 0 && byteMap[targetX, targetY - 1] == 0)
                {
                    (int? fallX, int? fallY) = SimulateFall(targetX, targetY, 0);

                    if (fallX.HasValue && fallY.HasValue)
                    {
                        neighbours.Add(nodeMap.GetGridObject(fallX.Value, fallY.Value));
                        neighbours.Last().type = NodeMovementType.WALK;
                        neighbours.Last().tentativeGCost = CalculateGCost(currentNode, neighbours.Last(), NodeMovementType.WALK);
                    }
                }
            }
        }

        // Try jumping left and right
        for (int direction = -1; direction <= 1; direction += 2)
        {
            int jumpTargetX = currentX + direction;
            float initialVelocityX = direction * speed;
            float initialVelocityY = jumpForce;

            // Simulate the parabolic jump
            (int? landingX, int? landingY) = SimulateJump(
                currentX,
                currentY,
                initialVelocityX,
                initialVelocityY
            );

            if (landingX.HasValue && landingY.HasValue)
            {
                // Add valid jump landing as a neighbor
                neighbours.Add(nodeMap.GetGridObject(landingX.Value, landingY.Value));
                neighbours.Last().type = NodeMovementType.JUMP;
                neighbours.Last().tentativeGCost = CalculateGCost(currentNode, neighbours.Last(), NodeMovementType.JUMP);
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Simulates a parabolic jump to determine where the character will land, accounting for terrain collisions.
    /// </summary>
    ///
    private (int? x, int? y) SimulateJump(int startX,int startY,float initialVelocityX,float initialVelocityY)
    {
        float timeStep = 0.05f; // Fine granularity for simulation
        float time = 0f;

        float startWorldX = startX * cellSize;
        float startWorldY = startY * cellSize;

        while (true)
        {
            time += timeStep;

            // Update position based on parabolic equations
            float x = startWorldX + initialVelocityX * time;
            float y = startWorldY + initialVelocityY * time - 0.5f * gravity * time * time;

            int gridX = (int)Math.Floor(x / cellSize);
            int gridY = (int)Math.Floor(y / cellSize);

            // Check if out of bounds
            if (gridX < 0 || gridX >= byteMap.GetLength(0) || gridY < 0 || gridY >= byteMap.GetLength(1))
            {
                return (null, null); // No valid landing spot
            }

            // Check for a valid landing spot
            if (byteMap[gridX, gridY] == 1)
            {
                // Ensure the cell below isn't also valid for smoother landings
                if (gridY == 0 || byteMap[gridX, gridY - 1] == 0)
                {
                    return (gridX, gridY);
                }
            }

            // Check for collision with terrain
            if (byteMap[gridX, gridY] != 0 && byteMap[gridX, gridY] != 1)
            {
                return (null, null); // Blocked by terrain
            }

            // Stop simulation if we've gone too far horizontally
            float maxWorldJumpWidth = CalculateMaxJumpWidth();
            if (Math.Abs(x - startWorldX) > maxWorldJumpWidth)
            {
                return (null, null);
            }
        }
    }
    private (int? x, int? y) SimulateFall(
    int startX,
    int apexY,
    float initialVelocityX)
    {
        float timeStep = 0.05f; // Fine granularity for simulation
        float time = 0f;

        float startWorldX = startX * cellSize;
        float startWorldY = apexY * cellSize;

        float currentX = startWorldX;
        float currentY = startWorldY;

        while (true)
        {
            time += timeStep;

            // Update position based on parabolic equations
            float x = currentX + initialVelocityX * time;
            float y = currentY - 0.5f * gravity * time * time;

            int gridX = (int)Math.Floor(x / cellSize);
            int gridY = (int)Math.Floor(y / cellSize);

            // Check if out of bounds
            if (gridX < 0 || gridX >= byteMap.GetLength(0) || gridY < 0)
            {
                return (null, null); // No valid landing spot
            }

            // Check for a valid landing spot
            if (gridY >= 0 && byteMap[gridX, gridY] == 1)
            {
                // Ensure the cell below isn't also valid for smoother landings
                if (gridY == 0 || byteMap[gridX, gridY - 1] == 0)
                {
                    return (gridX, gridY);
                }
            }

            // Check for collision with terrain
            if (byteMap[gridX, gridY] != 0 && byteMap[gridX, gridY] != 1)
            {
                return (null, null); // Blocked by terrain
            }
        }
    }
    /// <summary>
    /// Finds the nearest landing spot, including above or below the given coordinates.
    /// </summary>
    private int FindLandingSpot(int x, int startY)
    {
        // Check downwards first
        for (int y = startY; y >= 0; y--)
        {
            if (byteMap[x, y] == 1)
            {
                return y;
            }
        }

        // Then check upwards
        for (int y = startY + 1; y < byteMap.GetLength(1); y++)
        {
            if (byteMap[x, y] == 1)
            {
                return y;
            }
        }

        return -1; // No landing spot found
    }
    private void OnApplicationQuit()
    {
        if (pathfindingThread != null && pathfindingThread.IsAlive)
        {
            pathfindingThread.Abort();
        }
    }
}

