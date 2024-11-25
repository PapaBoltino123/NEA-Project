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
using UnityEngine.Scripting;
using UnityEditor.ShaderKeywordFilter;
using static UnityEditor.PlayerSettings;
using UnityEngine.Analytics;

public class Pathfinder
{
    public int jumpHeight;
    public int jumpDistance;
    public Grid<Node> map;

    private AStar graph;
    private static SynchronizationContext mainThreadContext;
    Thread pathfindingThread;

    public Pathfinder(int jumpHeight, int jumpDistance)
    {
        this.jumpHeight = jumpHeight;
        this.jumpDistance = jumpDistance;
        map = TerrainManager.Instance.ReturnWorldMap();
        graph = new AStar();
        CreateMap();
        CreateConnections();
    }
    private void CreateConnections()
    {
        foreach (int point in graph.GetPoints())
        {
            Vector2Int position = graph.GetPointPosition(point);
            Vector2Int cellType = GetCellType(position, true);

            List<int> twoWayPoints = new List<int>();
            List<int> oneWayPoints = new List<int>();

            foreach (int newPoint in graph.GetPoints())
            {
                Vector2Int newPosition = graph.GetPointPosition(newPoint);

                if (cellType.y  == 0 && Mathf.Approximately(newPosition.y, position.y) && newPosition.x > position.x)
                    twoWayPoints.Add(newPoint);

                if (cellType.x == -1)
                {
                    // Normal drop to left (bidirectional if new point is reachable)
                    if (newPosition.x == position.x - 1 && newPosition.y > position.y)
                    {
                        twoWayPoints.Add(newPoint);
                    }
                    else if (newPosition.y >= position.y - jumpHeight &&
                    newPosition.y <= position.y &&
                             newPosition.x > position.x - (jumpDistance + 2) &&
                             newPosition.x < position.x &&
                             GetCellType(newPosition, true).y == -1)
                    {
                        twoWayPoints.Add(newPoint);
                    }

                    // One-way drop to left (if no way to return)
                    else if (newPosition.x < position.x && newPosition.y >= position.y - jumpHeight)
                    {
                        oneWayPoints.Add(newPoint);
                    }
                }

                // Right-side drop connections
                if (cellType.y == -1)
                {
                    // Normal drop to right (bidirectional if new point is reachable)
                    if (newPosition.x == position.x + 1 && newPosition.y > position.y)
                    {
                        twoWayPoints.Add(newPoint);
                    }
                    else if (newPosition.y >= position.y - jumpHeight &&
                    newPosition.y <= position.y &&
                             newPosition.x < position.x + (jumpDistance + 2) &&
                             newPosition.x > position.x &&
                             GetCellType(newPosition, true).x == -1)
                    {
                        twoWayPoints.Add(newPoint);
                    }

                    else if (newPosition.x > position.x && newPosition.y >= position.y - jumpHeight)
                    {
                        oneWayPoints.Add(newPoint); // Only move right, can't go back
                    }
                }
            }

            // Add connections to the graph
            foreach (int joinPoint in twoWayPoints)
            {
                graph.ConnectPoints(point, joinPoint, true); // Bidirectional connection
            }
            foreach (int joinPoint in oneWayPoints)
            {
                graph.ConnectPoints(point, joinPoint, false); // One-way connection
            }
        }
    }
    private void CreateMap()
    {
        for (int x = (int)ChunkManager.Instance.loadedArea.xMin; x < (int)ChunkManager.Instance.loadedArea.xMax; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                Node node = map.GetGridObject(x, y);

                if (TerrainManager.Instance.CheckSolidNode(new Vector2Int(node.x, node.y)) == false)
                    continue;

                CreatePoint(new Vector2Int(node.x, node.y));

                if (GetCellType(new Vector2Int(node.x, node.y), false).x == -1)
                    CheckForDropPoint(new Vector2Int(node.x, node.y), Vector2Int.left);
                if (GetCellType(new Vector2Int(node.x, node.y), false).y == -1)
                    CheckForDropPoint(new Vector2Int(node.x, node.y), Vector2Int.right);
            }
        }
    }
    private Vector2Int GetCellType(Vector2Int position, bool isAbove)
    {
        Vector2Int cell = position;

        if (isAbove == true)
            cell += Vector2Int.down;

        if (TerrainManager.Instance.CheckSolidNode(cell + Vector2Int.up) == true)
            return Vector2Int.zero;

        Vector2Int result = Vector2Int.zero;

        if (TerrainManager.Instance.CheckSolidNode(cell + Vector2Int.up + Vector2Int.left) == true)
            result.x = 1;
        else if (TerrainManager.Instance.CheckSolidNode(cell + Vector2Int.left) == false)
            result.x = -1;

        if (TerrainManager.Instance.CheckSolidNode(cell + Vector2Int.up + Vector2Int.right) == true)
            result.y = 1;
        else if (TerrainManager.Instance.CheckSolidNode(cell + Vector2Int.right) == false)
            result.y = -1;

        return result;
    }

    private void CreatePoint(Vector2Int position)
    {
        if (graph.HasPointAtPosition(position) == true)
            return;

        int pointID = graph.GetAvailablePointID();
        graph.AddPoint(pointID, position);
    }
    private void CheckForDropPoint(Vector2Int position, Vector2Int direction)
    {
        Vector2Int startGridPosition = position;
        Vector2Int endGridPosition = startGridPosition + direction * jumpDistance + Vector2Int.down * jumpHeight;
        Vector3 startWorldPositon = map.GetWorldPosition(startGridPosition.x, startGridPosition.y);
        Vector3 endWorldPosition = map.GetWorldPosition(endGridPosition.x, endGridPosition.y);

        RaycastHit2D hit = Physics2D.Raycast(startWorldPositon, endWorldPosition - startWorldPositon);

        if (hit.collider != null)
            CreatePoint(new Vector2Int(map.GetXY(hit.point).x, map.GetXY(hit.point).y));
    }
    public void FindPath(Node start, Node end, Zombie.PathFoundCallback pathFoundCallback)
    {
        pathfindingThread = new Thread(() =>
        {
            Vector2Int startPoint = new Vector2Int(start.x, start.y);
            Vector2Int endPoint = new Vector2Int(end.x, end.y);
            // Get the closest points in the graph to the start and end positions
            int firstPoint = graph.GetClosestPoint(startPoint);
            int finish = graph.GetClosestPoint(endPoint);

            // Get the path as a list of point IDs
            List<Node> path = graph.GetIDPath(firstPoint, finish);

            mainThreadContext.Post(_ => pathFoundCallback(path), null);
        });
        pathfindingThread.Start();
        GameManager.Instance.activeThreads.Add(pathfindingThread);
    }

}

