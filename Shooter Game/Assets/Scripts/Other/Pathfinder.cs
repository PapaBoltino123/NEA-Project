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
using System.IO;

public class Pathfinder
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

            startNode.gCost = 0; //initial distance travelled is 0
            startNode.hCost = CalculateHeuristicCost(startNode, endNode); //calculates heuristic cost to goal using euclidean distance and weights
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
                    Debug.Log(node);
                    if (closed.Contains(node))
                        continue;

                    float tentativeGCost = currentNode.gCost + node.tentativeGCost;

                    if (tentativeGCost < node.gCost)
                    {
                        node.parentNode = currentNode;
                        node.gCost = tentativeGCost;
                        node.hCost = CalculateHeuristicCost(node, endNode);
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
                    mainThreadContext.Post(addToMainThread => pathFoundCallback(path), null);
                }
            }
            if (found == false)
                mainThreadContext.Post(addToMainThread => pathFoundCallback(new List<Node>()), null);
        });
        ThreadManager.Instance.activeThreads.Add(pathfindingThread);
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
    public float CalculateHeuristicCost(Node start, Node end)
    {
        float distance = MathF.Sqrt(MathF.Pow(start.x - end.x, 2) + MathF.Pow(start.y - end.y, 2));
        return distance * 0.9f; // Ensure weight ≤ 1 for admissibility
    }
    public float CalculateCost(Node start, Node end, NodeMovementType movementType)
    {
        float baseCost;

        // Calculate Euclidean distance between nodes
        float distance = MathF.Sqrt(MathF.Pow(end.x - start.x, 2) + MathF.Pow(end.y - start.y, 2));

        // Assign base costs based on movement type
        switch (movementType)
        {
            case NodeMovementType.WALK:
                baseCost = distance; // Walking is direct, no extra penalty
                break;

            case NodeMovementType.JUMP:
                // Add extra penalty for jumping
                baseCost = distance * 1.5f; // Adjust multiplier as necessary for game balance
                break;

            default:
                throw new ArgumentException("Unsupported movement type");
        }

        // Total cost
        return baseCost;
    }
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

            int groundIndex = Enumerable.Range(0, byteMap.GetLength(1))
                    .Select(i => byteMap[targetX, i])
                    .ToList()
                    .FindIndex(n => n == 1);

            // Ensure within grid bounds
            if (targetX < 0 || targetX >= byteMap.GetLength(0))
                continue;

            // Check if the position directly to the side is walkable
            if (byteMap[targetX, targetY] == 1)
            {
                // Add horizontal neighbor
                neighbours.Add(nodeMap.GetGridObject(targetX, targetY));
                neighbours.Last().type = NodeMovementType.WALK;
                neighbours.Last().tentativeGCost = CalculateCost(currentNode, neighbours.Last(), NodeMovementType.WALK);
            }
            else if (byteMap[targetX, targetY] == 0 && groundIndex < targetY)
            {
                // Simulate fall if there’s a void below the walked-to position
                if (targetY > 0 && byteMap[targetX, targetY - 1] == 0)
                {
                    (int? fallX, int? fallY) = SimulateFall(targetX, targetY, 0);

                    if (fallX.HasValue && fallY.HasValue)
                    {
                        neighbours.Add(nodeMap.GetGridObject(fallX.Value, fallY.Value));
                        neighbours.Last().type = NodeMovementType.WALK;
                        neighbours.Last().tentativeGCost = CalculateCost(currentNode, neighbours.Last(), NodeMovementType.WALK);
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
            (int? landingX, int? landingY) = SimulateJump(currentX, currentY, initialVelocityX, initialVelocityY);

            if (landingX.HasValue && landingY.HasValue)
            {
                // Add valid jump landing as a neighbor
                neighbours.Add(nodeMap.GetGridObject(landingX.Value, landingY.Value));
                neighbours.Last().type = NodeMovementType.JUMP;
                neighbours.Last().tentativeGCost = CalculateCost(currentNode, neighbours.Last(), NodeMovementType.JUMP);
            }
        }

        return neighbours;
    }
    public (int? x, int? y) SimulateJump(int startX, int startY, float initialVelocityX, float initialVelocityY)
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
    private (int? x, int? y) SimulateFall(int startX, int apexY, float initialVelocityX)
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
}

