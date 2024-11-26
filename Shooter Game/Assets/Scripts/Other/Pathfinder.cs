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

    public AStar graph;
    private static SynchronizationContext mainThreadContext;
    private CustomDictionary<Vector2Int, Vector2Int> unresolvedDropPoints = new CustomDictionary<Vector2Int, Vector2Int>();
    Thread pathfindingThread;

    public Pathfinder(int jumpHeight, int jumpDistance)
    {
        this.jumpDistance = jumpDistance;
        this.jumpHeight = jumpHeight;
        map = TerrainManager.Instance.ReturnWorldMap();
        graph = new AStar();
        mainThreadContext = SynchronizationContext.Current;
        byte[,] byteMap = TerrainManager.Instance.ReturnMapAsByteGrid();

        for (int x = 0; x < byteMap.GetLength(0); x++)
        {
            int y = Enumerable.Range(0, byteMap.GetLength(1))
                .Select(i => byteMap[x, i])
                .ToList()
                .FindIndex(n => n == 1);

            graph.AddPoint(new Vector2Int(x, y));
        }
    }
    public void CreateConnections()
    {
        foreach (var position in graph.GetPoints())
        {
            List<Vector2Int> twoWayPoints = new List<Vector2Int>();
            List<Vector2Int> oneWayPoints = new List<Vector2Int>();

            foreach (var newPosition in graph.GetPoints())
            {
                if (cellType.y == 0 && Mathf.Approximately(newPosition.y, position.y) && newPosition.x > position.x)
                {
                    if (!graph.CheckConnectionExists(position, newPosition))
                        twoWayPoints.Add(newPosition);
                }

                if (cellType.x == -1)
                {
                    // Normal drop to left (bidirectional if new point is reachable)
                    if (newPosition.x == position.x - 1 && newPosition.y > position.y)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            twoWayPoints.Add(newPosition);
                    }
                    else if (newPosition.y >= position.y - jumpHeight &&
                    newPosition.y <= position.y &&
                             newPosition.x > position.x - (jumpDistance + 2) &&
                             newPosition.x < position.x &&
                             GetCellType(newPosition, true).y == -1)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            twoWayPoints.Add(newPosition);
                    }

                    // One-way drop to left (if no way to return)
                    else if (newPosition.x < position.x && newPosition.y >= position.y - jumpHeight)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            oneWayPoints.Add(newPosition);
                    }
                }

                // Right-side drop connections
                if (cellType.y == -1)
                {
                    // Normal drop to right (bidirectional if new point is reachable)
                    if (newPosition.x == position.x + 1 && newPosition.y > position.y)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            twoWayPoints.Add(newPosition);
                    }
                    else if (newPosition.y >= position.y - jumpHeight &&
                    newPosition.y <= position.y &&
                             newPosition.x < position.x + (jumpDistance + 2) &&
                             newPosition.x > position.x &&
                             GetCellType(newPosition, true).x == -1)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            twoWayPoints.Add(newPosition);
                    }

                    else if (newPosition.x > position.x && newPosition.y >= position.y - jumpHeight)
                    {
                        if (!graph.CheckConnectionExists(position, newPosition))
                            oneWayPoints.Add(newPosition);
                    }
                }
            }

            // Add connections to the graph
            foreach (Vector2Int joinPoint in twoWayPoints)
            {
                graph.ConnectPoints(position, joinPoint, true); // Bidirectional connection
            }
            foreach (Vector2Int joinPoint in oneWayPoints)
            {
                graph.ConnectPoints(position, joinPoint, false); // One-way connection
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

    public void CreatePoint(Vector2Int position)
    {
        if (graph.HasPointAtPosition(position) == true)
            return;

        graph.AddPoint(position);
    }
    public void FindPath(Node start, Node end, Zombie.PathFoundCallback pathFoundCallback)
    {
        pathfindingThread = new Thread(() =>
        {
            Vector2Int startPoint = new Vector2Int(start.x, start.y);
            Vector2Int endPoint = new Vector2Int(end.x, end.y);
            // Get the closest points in the graph to the start and end positions
            Vector2Int firstPoint = graph.GetClosestPoint(startPoint);
            Vector2Int finish = graph.GetClosestPoint(endPoint);

            // Get the path as a list of point IDs
            List<Node> path = graph.GetIDPath(firstPoint, finish);

            mainThreadContext.Post(_ => pathFoundCallback(path), null);
        });
        pathfindingThread.Start();
        GameManager.Instance.activeThreads.Add(pathfindingThread);
    }
    private bool IsChunkLoaded(Vector2Int position)
    {
        Vector2Int chunkPosition = new Vector2Int((position.x / TerrainManager.Instance.chunkWidth)
            * TerrainManager.Instance.chunkWidth, 0);
        return ChunkManager.Instance.activeChunks.ContainsKey(chunkPosition);
    }
}

