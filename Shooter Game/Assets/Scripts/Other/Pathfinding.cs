using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using Unity.VisualScripting;
using System.Runtime.InteropServices;

namespace System.Algorithms.Pathfinding
{
    public class AStar
    {
        List<Vector2Int> points = new List<Vector2Int>();
        private CustomDictionary<Vector2Int, List<Vector2Int>> connections = new CustomDictionary<Vector2Int, List<Vector2Int>>();
        public Grid<Node> nodeMap;
        public byte[,] byteMap;

        public void AddPoint(Vector2Int position)
        {
            if (!points.Contains(position))
            {
                points.Add(position);
                connections[position] = new List<Vector2Int>();
            }
        }
        public void ConnectPoints(Vector2Int point1, Vector2Int point2, bool canReturnToPoint1)
        {
            if (!points.Contains(point1) || !points.Contains(point2))
                return;

            if (!connections[point1].Contains(point2))
                connections[point1].Add(point2);

            if (canReturnToPoint1 == true && !connections[point2].Contains(point1))
                connections[point2].Add(point1);
        }
        public Vector2Int GetClosestPoint(Vector2Int position)
        {
            Vector2Int closestPosition = Vector2Int.zero;
            float closestDistance = float.MaxValue;

            foreach (var p in points)
            {
                float distance = CalculateDistance(p, position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPosition = p;
                }
            }

            return closestPosition;
        }
        public float CalculateDistance(Vector2 point1, Vector2 point2)
        {
            float deltaX = point2.x - point1.x;
            float deltaY = point2.y - point1.y;

            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        private float HeuristicCost(Vector2Int iD1, Vector2Int iD2)
        {
            return CalculateDistance(iD1, iD2);
        }
        public List<Node> GetIDPath(Vector2Int start, Vector2Int end)
        {
            if (!points.Contains(start) || !points.Contains(end))
                return null;

            CustomPriorityQueue<Vector2Int> open = new CustomPriorityQueue<Vector2Int>();
            List<Node> closed = new List<Node>();
            CustomDictionary<Vector2Int, float> gCosts = new CustomDictionary<Vector2Int, float>();
            CustomDictionary<Vector2Int, float> fCosts = new CustomDictionary<Vector2Int, float>();

            gCosts[start] = 0;
            fCosts[start] = HeuristicCost(start, end);
            open.Enqueue(start, fCosts[start]);

            while (open.Count > 0)
            {
                Vector2Int current = open.Dequeue();

                if (current == end)
                    return ReconstructPath(current);

                foreach (var neighbour in connections[current])
                {
                    float tentativeGCost = gCosts[current] + CalculateDistance(current, neighbour);

                    if (!gCosts.ContainsKey(neighbour) || tentativeGCost < gCosts[neighbour])
                    {
                        Node parentNode = nodeMap.GetGridObject(current.x, current.y);
                        Node neighbourNode = nodeMap.GetGridObject(neighbour.x, neighbour.y);

                        neighbourNode.parentNode = parentNode;
                        gCosts[neighbour] = tentativeGCost;
                        fCosts[neighbour] = tentativeGCost + HeuristicCost(neighbour, end);

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, fCosts[neighbour]);
                    }
                }
            }

            return new List<Node>();
        }

        public IEnumerable<Vector2Int> GetPoints()
        {
            return points;
        }
        public List<Vector2Int> GetConnectedPoints(Vector2Int iD)
        {
            return (connections.ContainsKey(iD)) ? connections[iD] : new List<Vector2Int>();
        }
        public bool HasPointAtPosition(Vector2Int position)
        {
            return points.Contains(position);
        }
        private List<Node> ReconstructPath(Vector2Int end)
        {
            List<Node> path = new List<Node>();

            Node node = nodeMap.GetGridObject(end.x, end.y);
            path.Add(node);

            while (node.parentNode != null)
            {
                path.Add(node.parentNode);
                node = node.parentNode;
            }

            path.Reverse();
            return path;
        }
        public void RemovePoint(Vector2Int iD)
        {
            points.Remove(iD);
        }
        public bool CheckConnectionExists(Vector2Int pointA, Vector2Int pointB)
        {
            if (!connections.ContainsKey(pointA))
                return false;

            return connections[pointA].Contains(pointB);
        }
    }
}
