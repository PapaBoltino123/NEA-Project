using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject>
{
    private int width;
    private int height;
    private float cellSize;
    private TGridObject[,] gridArray;

    public Grid(int width, int height, float cellSize, Func<Grid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                gridArray[x, y] = createGridObject(this, x, y); 
            }
        }
    }
    public int GetWidth() { return width; }
    public int GetHeight() { return height; }
    public float GetCellSize() { return cellSize; }
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize;
    }
    public (int, int) GetXY(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition).y / cellSize);
        return (x, y);
    }
    public TGridObject GetGridObject(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
            return gridArray[x, y];
        else
            return default(TGridObject);
    }
    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        (int, int) coordinates;
        coordinates = GetXY(worldPosition);
        return GetGridObject(coordinates.Item1, coordinates.Item2);
    }
}
