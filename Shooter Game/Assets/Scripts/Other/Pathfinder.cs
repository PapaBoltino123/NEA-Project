using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.Pathfinding;
using System.Threading;
using Unity.VisualScripting;

public class Pathfinder
{
    #region Variable Declaration
    private CustomStack<int> touchedLocations; //contains the location indexes that the path uses
    List<Node>[] nodes; //the list of nodes in the map

    private byte[,] grid = null; //the byte grid representing the map (0s represent ground and 1s represent walkable areas)
    private CustomPriorityQueue<Location> open = null; //a priority queue containing the location indexes of nodes that need to be evaluated
    private List<Vector2Int> closed = null; //the closed list of coordinates that have been evaluated 
    private bool stop = false; 
    private bool isStopped = true;
    private int hEstimate = 2; 
    private int searchLimit = 8000; //the amount of nodes that can be evaluated by the algorithm before it determines that no path can be found
    private byte openNodeValue = 1; //the value representing nodes that need to be evaluated
    private byte closedNodeValue = 2; //the value representing nodes that have already been evaluated

    private int hCost = 0; //the heuristic cost to get to the target node calculated by the heuristic formula
    private Location currentLoc; //the current location index that the map is evaluating
    private sbyte[,] direction = new sbyte[4, 2] { { 0, -1 }, { 1, 0 }, { 0, 1 }, { -1, 0 } }; //contains the directions that the algorithm can proceed to in the next node (up, right, left, down)
    private int newLoc = 0; //the new location index
    private Node node; //the current node being evaluated
    private ushort currentLocX = 0; //the x coordinate of the current location index
    private ushort currentLocY = 0; //the y coordinate of the current location index
    private ushort newLocX = 0; //the x coordinate of the new location index
    private ushort newLocY = 0; //the y coordinate of the new location index
    private int closedNodeCounter = 0; //counts the amount of nodes that have been evaluated
    private ushort gridX = 0; //the length of the first dimension of the grid array
    private ushort gridY = 0; //the length of the second dimension of the grid array
    private ushort bitwiseMask = 0; //the mask used in the bitwise AND operation to convert from the xy index back to the x coordinate
    private ushort gridXLog2 = 0; //the length of the first dimension logged to base 2
    private bool found = false; //signifies whether or not the path has been found
    private int endLoc = 0; //the index of the end node
    private int newGCost = 0; //the new gCost of the current node's successor
    #endregion
    #region Constructor
    public Pathfinder(byte[,] grid)
    {
        if (grid == null)
            throw new System.Exception("Grid cannot be null");

        this.grid = grid;
        this.gridX = (ushort)grid.GetLength(0); 
        this.gridY = (ushort)grid.GetLength(1);
        this.gridXLog2 = (ushort)System.Math.Log(gridX, 2);
        this.bitwiseMask = (ushort)((1 << gridXLog2) - 1); //the mask is calculated by shifting 1 by the log of the grid x length and then subtracting one

        if ((System.Math.Log(gridX, 2) != (int)System.Math.Log(gridX, 2)) ||
            (System.Math.Log(gridY, 2) != (int)System.Math.Log(gridY, 2)))
        {
            Debug.Log($"{gridX}, {gridY}");
            throw new System.Exception("Grid lengths must be a power of two");
        }

        if (nodes == null || nodes.Length != (gridX * gridY)) //initializes all of the lists, queues and stacks
        {
            nodes = new List<Node>[gridX * gridY]; 
            touchedLocations = new CustomStack<int>(gridX * gridY);
            closed = new List<Vector2Int>(gridX * gridY);
        }
        for (int i = 0; i < nodes.Length; i++)
        {
            nodes[i] = new List<Node>(1); 
        }
        open = new CustomPriorityQueue<Location>();
    }
    #endregion
    #region Methods
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, short maxJumpHeight)
    {
        while (touchedLocations.Count > 0)
        {
            nodes[touchedLocations.Pop()].Clear(); //clears the touched locations stack and the lists held in the nodes array
        }

        if (grid[end.x, end.y] == 0) //if the target coordinate is unwalkable return null as there is no path to be calculated
            return null;

        found = false; //initially setting all the variables
        stop = false;
        isStopped = false;
        closedNodeCounter = 0;
        openNodeValue += 2;
        closedNodeValue += 2;
        open.Clear();

        currentLoc.xy = (start.y << gridXLog2) + start.x; //obtains the index in the nodes array by shifting the y coordinate left by the gridXLog2 and adding x
        currentLoc.z = 0; //sets the index of the list inside that current index, represented as a z coordinate
        endLoc = (end.y << gridXLog2) + end.x; //performs the same shift to obtain the index of the ending node

        Node firstNode = new Node(); //creates the first node and sets it's values
        firstNode.gCost = 0;
        firstNode.fCost = hEstimate;
        firstNode.parentCoordinates.x = (ushort)start.x;
        firstNode.parentCoordinates.y = (ushort)start.y;
        firstNode.parentCoordinates.z = 0;
        firstNode.status = openNodeValue;

        if (IsGrounded(start, grid)) //check if the node is grounded and if it is, set the jump value to be 0 (the jump value when grounded)
            firstNode.jumpLength = 0;
        else //if the node is in the air, set the jumpLength to be the maximum jump height so that the path immediately starts falling
            firstNode.jumpLength = (short)(maxJumpHeight * 2);
        
        nodes[currentLoc.xy].Add(firstNode); //adds the first node to the list at the index of the nodes array specified by the current location cost
        touchedLocations.Push(currentLoc.xy); //pushes the current location index into the stack so the algorithm knows that it has been through there
        AddToOpen(currentLoc); //adds the current location to the open priority queue

        while (open.Count > 0 && stop == false) //while there are nodes to be evaluated and the algorithm is not stopped
        {
            currentLoc = open.Dequeue(); //dequeue the item with the highest fCost

            if (nodes[currentLoc.xy][currentLoc.z].status == closedNodeValue) //if the dequeued node is already in the closed node list, skip it
                continue;

            currentLocX = (ushort)(currentLoc.xy & bitwiseMask); //uses the bitwise mask to obtain the x from the current location index using a bitwise AND operator
            currentLocY = (ushort)(currentLoc.xy >> gridXLog2); //uses the logged grid length to shift the xy right to obtain the y value

            if (currentLoc.xy == endLoc) //if the current location index is equal to the end location index
            {
                nodes[currentLoc.xy][currentLoc.z] = nodes[currentLoc.xy][currentLoc.z].UpdateStatus(closedNodeValue); //update the node's status to closed
                found = true; //path has been found
                break; //break while loop
            }
            if (closedNodeCounter > searchLimit) //if the closed nde count exceeds the search limit
            {
                isStopped = true; //stop the algorithm
                return null; //there is no path so return null
            }

            for (int i = 0; i < 4; i++) //loops through each direction (one left, right, up down)
            {
                newLocX = (ushort)(currentLocX + direction[i, 0]); //obtains the new location x and y from the current location by checking neighbouring nodes
                newLocY = (ushort)(currentLocY + direction[i, 1]);
                newLoc = (newLocY << gridXLog2) + newLocX; //obtains the location index from the x and y

                bool onGround = false;

                if (grid[newLocX, newLocY] == 0) //if the grid coordinate is a block, skip the node
                    goto LOOP_END;

                if (IsGrounded(new Vector2Int(newLocX, newLocY), grid) == true) //if the current node is on the ground, onGround is true
                    onGround = true;

                short jumpLength = nodes[currentLoc.xy][currentLoc.z].jumpLength; //obtains the jump length of the current node 
                short newJumpLength = jumpLength; 

                if (onGround == true) //if grounded, the new nodes jumpLength is equal to 0
                    newJumpLength = 0;

                else if (newLocY > currentLocY) //if the the new node is above the parent node
                {
                    if (jumpLength < 2)
                        newJumpLength = 3; //this accounts for the velocity at the start of the jump being greater than the velocity at the end of the jump
                    else if (jumpLength % 2 == 0) 
                        newJumpLength = (short)System.Math.Max(jumpLength + 2, 2);
                    else
                        newJumpLength = (short)System.Math.Max(jumpLength + 1, 2);
                }
                else if (newLocY < currentLocY) //if the new node is below the parent node
                {
                    if (jumpLength % 2 == 0)
                        newJumpLength = (short)System.Math.Max(maxJumpHeight * 2, jumpLength + 2);
                    else
                        newJumpLength = (short)System.Math.Max(maxJumpHeight * 2, jumpLength + 1);
                }

                if (jumpLength % 2 != 0 && currentLocX != newLocX)
                    continue;
                if (jumpLength >= maxJumpHeight * 2 && newLocY > currentLocY)
                    continue;
                if (newJumpLength >= maxJumpHeight * 2 + 6 && newLocX != currentLocX && (newJumpLength - (maxJumpHeight * 2 + 6)) % 8 != 3)
                    continue;

                newGCost = nodes[currentLoc.xy][currentLoc.z].gCost + grid[newLocX, newLocY] + newJumpLength / 4;

                if (nodes[newLoc].Count > 0)
                {
                    int lowestJump = short.MaxValue;
                    bool couldMoveSideways = false;

                    for (int j = 0; j < nodes[newLoc].Count; j++)
                    {
                        if (nodes[newLoc][j].jumpLength < lowestJump)
                            lowestJump = nodes[newLoc][j].jumpLength;
                        if (nodes[newLoc][j].jumpLength % 2 == 0 && nodes[newLoc][j].jumpLength < maxJumpHeight * 2 + 6)
                            couldMoveSideways = true;
                    }

                    if (lowestJump <= newJumpLength &&
                        (newJumpLength % 2 != 0 || newJumpLength >= maxJumpHeight * 2 + 6 || couldMoveSideways))
                        continue;
                }

                hCost = hEstimate * (System.Math.Abs(newLocX - end.x) + System.Math.Abs(newLocY - end.y));

                Node newNode = new Node();
                newNode.jumpLength = newJumpLength;
                newNode.parentCoordinates.x = currentLocX;
                newNode.parentCoordinates.y = currentLocY;
                newNode.parentCoordinates.z = (byte)currentLoc.z;
                newNode.gCost = newGCost;
                newNode.fCost = hCost + newGCost;
                newNode.status = openNodeValue;

                if (nodes[newLoc].Count == 0)
                    touchedLocations.Push(newLoc);

                nodes[newLoc].Add(newNode);
                AddToOpen(new Location(newLoc, nodes[newLoc].Count - 1));

                LOOP_END:
                    continue;
            }
        }
        if (found == true)
        {
            closed.Clear();
            var posX = end.x;
            var posY = end.y;
            var fPrevNodeTmp = new Node();
            var fNodeTmp = nodes[endLoc][0];
            var fNode = end;
            var fPrevNode = end;
            var loc = (fNodeTmp.parentCoordinates.y << gridXLog2) + fNodeTmp.parentCoordinates.x;

            while (fNode.x != fNodeTmp.parentCoordinates.x || fNode.y != fNodeTmp.parentCoordinates.y)
            {
                var fNextNodeTmp = nodes[loc][fNodeTmp.parentCoordinates.z];

                if ((closed.Count == 0)
                    || (fNextNodeTmp.jumpLength != 0 && fNodeTmp.jumpLength == 0)
                    || (fNodeTmp.jumpLength == 3 && fPrevNodeTmp.jumpLength != 0)
                    || (fNodeTmp.jumpLength == 0 && fPrevNodeTmp.jumpLength != 0)
                    || (fNode.y > closed[closed.Count - 1].y && fNode.y > fNodeTmp.parentCoordinates.y)
                    || ((IsGrounded(fNode.x - 1, fNode.y) || IsGrounded(fNode.x + 1, fNode.y))
                        && fNode.y != closed[closed.Count - 1].y && fNode.x != closed[closed.Count - 1].x))
                    closed.Add(fNode);

                fPrevNode = fNode;
                posX = fNodeTmp.parentCoordinates.x;
                posY = fNodeTmp.parentCoordinates.y;
                fPrevNodeTmp = fNodeTmp;
                fNodeTmp = fNextNodeTmp;
                loc = (fNodeTmp.parentCoordinates.y << gridXLog2) + fNodeTmp.parentCoordinates.x;
                fNode = new Vector2Int(posX, posY);
            }
            closed.Add(fNode);
            isStopped = true;
            return closed;
        }
        return null;
    }
    private bool IsGrounded(Vector2Int point, byte[,] grid)
    {
        if (grid[point.x, point.y - 1] == 0) //if the coordinate below the current point is a block the point is grounded
            return true;
        else
            return false;
    }
    private bool IsGrounded(int x, int y)
    {
        if (grid[x, y - 1] == 0) //if the coordinate below the current point is a block the point is grounded
            return true;
        else
            return false;
    }
    private void AddToOpen(Location location)
    {
        int priority = nodes[location.xy][location.z].fCost; //obtains the priority through the nodes fCost
        open.Enqueue(location, priority); //enqueues the location
    }
    #endregion
}

