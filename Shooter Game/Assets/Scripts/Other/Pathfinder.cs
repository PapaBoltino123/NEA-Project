using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.Pathfinding;
using System.Threading;
using System;

public class Pathfinder
{
    #region Variable Declaration
    private List<Node>[] nodes;
    private CustomStack<int> touchedLocations;

    private byte[,] grid = null;
    private CustomPriorityQueue<Location> open = null;
    private List<Vector2Int> closed = null;
    private bool prematureStop = false;
    private bool isStopped = true;
    private HeuristicFormula formula = HeuristicFormula.Manhattan;
    private int hEstimate = 2;
    private bool tieBreaker = false;
    private int searchLimit = 100000;
    private float completedTime = 0;
    private bool debugProgress = false;
    private bool debugFoundPath = false;
    private byte openNodeValue = 1;
    private byte closedNodeValue = 2;

    private int hCost = 0;
    private Location location;
    private int newLocation = 0;
    private Node node;
    private (ushort x, ushort y) locationCoordinates = (0, 0);
    private (ushort x, ushort y) newLocationCoordinates = (0, 0);
    private int closedNodeCounter = 0;
    private ushort gridX = 0;
    private ushort gridY = 0;
    private ushort gridXMinusOne = 0;
    private ushort gridXLogTwo = 0;
    private bool found = false;
    private int endLocation = 0;
    private int newGCost = 0;
    private sbyte[,] direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } };
    #endregion
    #region Properties
    public bool Stopped
    {
        get { return isStopped; }
    }
    public int HeuristicEstimate
    {
        get { return hEstimate; }
        set { hEstimate = value; }
    }
    public bool TieBreaker
    {
        get { return tieBreaker; }
        set { tieBreaker = value; }
    }
    public int SearchLimit
    {
        get { return searchLimit; }
        set { searchLimit = value; }
    }

    public float CompletedTime
    {
        get { return completedTime; }
        set { completedTime = value; }
    }
    #endregion
    #region Constructor
    public Pathfinder(byte[,] grid)
    {
        if (grid == null)
            throw new System.Exception("Grid cannot be null");

        this.grid = grid;
        this.gridX = (ushort)(this.grid.GetUpperBound(0) + 1);
        this.gridY = (ushort)(this.grid.GetUpperBound(1) + 1);
        this.gridXMinusOne = (ushort)(gridX - 1);
        this.gridXLogTwo = (ushort)(System.Math.Log(gridX, 2));

        if (System.Math.Log(gridX, 2) != (int)System.Math.Log(gridX, 2) ||
                System.Math.Log(gridY, 2) != (int)System.Math.Log(gridY, 2))
            throw new System.Exception("Invalid Grid, size in X and Y must be power of 2");

        if (nodes == null || nodes.Length != (gridX * gridY))
        {
            this.nodes = new List<Node>[gridX * gridY];
            this.touchedLocations = new CustomStack<int>(gridX * gridY);
            this.closed = new List<Vector2Int>(gridX * gridY);
        }

        this.open = new CustomPriorityQueue<Location>();
    }
    #endregion
    #region Methods
    //public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, short maxJumpHeight)
    //{
    //    while (touchedLocations.Count > 0)
    //        nodes[touchedLocations.Pop()].Clear();

    //    if (grid[end.x, end.y] == 0)
    //        return null;

    //    location.xy = (start.y << gridXLogTwo) + start.x;
    //    location.z = 0;
    //    endLocation = (end.y << gridXLogTwo) + end.x;
    //}
    #endregion
}

