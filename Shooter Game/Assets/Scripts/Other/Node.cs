using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public string binaryValue {  get; set; }
    private Grid<Node> grid;
    public int x, y;
    public int fCost, gCost, hCost;
    public Node parentNode;
    public bool isWalkable;

    private const string SKY = "0000"; 
    private const string DIRT = "0001";
    private const string STONE = "0010";
    private const string WATER = "0011"; 
    private const string FLOWERS = "0100";
    private const string LILYPADS = "0101"; 
    private const string GRASS = "0110";
    private const string ROCK = "0111"; 
    private const string TREE = "1000";

    public Node(Grid<Node> grid, int x, int y)
    {
        this.x = x; this.y = y;
        this.grid = grid;
    }
    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
    public void CheckWalkable()
    {
        try
        {
            if (this.grid.GetGridObject(this.x, this.y).binaryValue != GRASS || grid.GetGridObject(this.x, this.y).binaryValue != LILYPADS || grid.GetGridObject(this.x, this.y).binaryValue != TREE)
            {
                this.isWalkable = false;
            }
            else if (this.grid.GetGridObject(this.x, this.y).binaryValue == ROCK)
            {
                if (this.grid.GetGridObject(this.x + 1, this.y).binaryValue == SKY || this.grid.GetGridObject(this.x - 1, this.y).binaryValue == SKY || this.grid.GetGridObject(this.x, this.y + 1).binaryValue == SKY)
                {
                    this.isWalkable = true;
                }
                else
                {
                    this.isWalkable = false;
                }
            }
            else
            {
                this.isWalkable = true;
            }
        }
        catch
        {
            isWalkable = false;
        }
    }
}
