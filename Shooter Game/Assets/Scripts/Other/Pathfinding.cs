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
        private CustomDictionary<int, Vector2Int> points = new CustomDictionary<int, Vector2Int>();
        private CustomDictionary<int, List<int>> connections = new CustomDictionary<int, List<int>>();
        private Grid<Node> map;

        public AStar()
        {
            map = TerrainManager.Instance.ReturnWorldMap();
        }

        public void AddPoint(int id, Vector2Int position)
        {
            if (!points.ContainsKey(id))
            {
                points[id] = position;
                connections[id] = new List<int>();
            }
        }
        public void ConnectPoints(int point1, int point2, bool canReturnToPoint1)
        {
            if (!points.ContainsKey(point1) || !points.ContainsKey(point2))
                return;

            if (!connections[point1].Contains(point2))
                connections[point1].Add(point2);

            if (canReturnToPoint1 == true && !connections[point2].Contains(point1))
                connections[point2].Add(point1);
        }
        public int GetClosestPoint(Vector2Int position)
        {
            int closestID = -1;
            float closestDistance = float.MaxValue;

            foreach (var k in points.Keys)
            {
                var point = points[k];
                float distance = CalculateDistance(point, position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestID = k;
                }
            }

            return closestID;
        }
        public float CalculateDistance(Vector2 point1, Vector2 point2)
        {
            float deltaX = point2.x - point1.x;
            float deltaY = point2.y - point1.y;

            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }
        private float HeuristicCost(int iD1, int iD2)
        {
            return CalculateDistance(points[iD1], points[iD2]);
        }
        public List<Node> GetIDPath(int startID, int endID)
        {
            if (!points.ContainsKey(startID) || !points.ContainsKey(endID))
                return null;

            CustomPriorityQueue<int> open = new CustomPriorityQueue<int>();
            List<Node> closed = new List<Node>();
            CustomDictionary<int, float> gCosts = new CustomDictionary<int, float>();
            CustomDictionary<int, float> fCosts = new CustomDictionary<int, float>();

            gCosts[startID] = 0;
            fCosts[startID] = HeuristicCost(startID, endID);
            open.Enqueue(startID, fCosts[startID]);

            while (open.Count > 0)
            {
                int current = open.Dequeue();

                if (current == endID)
                    return ReconstructPath(current);

                foreach (int neighbour in connections[current])
                {
                    float tentativeGCost = gCosts[current] + CalculateDistance(points[current], points[neighbour]);

                    if (!gCosts.ContainsKey(neighbour) || tentativeGCost < gCosts[neighbour])
                    {
                        Node parentNode = map.GetGridObject(points[current].x, points[current].y);
                        Node neighbourNode = map.GetGridObject(points[neighbour].x, points[neighbour].y);

                        neighbourNode.parentNode = parentNode;
                        gCosts[neighbour] = tentativeGCost;
                        fCosts[neighbour] = tentativeGCost + HeuristicCost(neighbour, endID);

                        if (!open.Contains(neighbour))
                            open.Enqueue(neighbour, fCosts[neighbour]);
                    }
                }
            }

            return new List<Node>();
        }

        public IEnumerable<int> GetPoints()
        {
            return points.Keys;
        }
        public Vector2Int GetPointPosition(int iD)
        {
            return (points.ContainsKey(iD)) ? points[iD] : Vector2Int.zero;
        }
        public List<int> GetConnectedPoints(int iD)
        {
            return (connections.ContainsKey(iD)) ? connections[iD] : new List<int>();
        }
        public bool HasPointAtPosition(Vector2Int position)
        {
            foreach (var k in points.Keys)
            {
                if (points[k] == position)
                    return true;
            }
            return false;
        }
        public int GetAvailablePointID()
        {
            int iD = 0;

            while (points.ContainsKey(iD))
            {
                iD++;
            }
            return iD;
        }
        private List<Node> ReconstructPath(int end)
        {
            List<Node> path = new List<Node>();

            Node node = map.GetGridObject(points[end].x, points[end].y);
            path.Add(node);

            while (node.parentNode != null)
            {
                path.Add(node.parentNode);
                node = node.parentNode;
            }

            path.Reverse();
            return path;
        }
        public void RemovePoint(int iD)
        {
            points.Remove(iD);
        }
        public bool CheckConnectionExists(int pointA, int pointB)
        {
            if (!connections.ContainsKey(pointA))
                return false;

            return connections[pointA].Contains(pointB);
        }
    }
}
