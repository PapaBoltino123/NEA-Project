using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Algorithms;
using System.Algorithms.TerrainGeneration;

public class TerrainManager : Manager
{
    #region Variable Declaration
    [SerializeField] TileBase[] tiles, rock, tree;
    [SerializeField] Tilemap ground, decoration, collisions;
    public Grid<Node> bitmap;
    [SerializeField] int waterLevel;
    [SerializeField] GameManager gameManager;
    [SerializeField] Player player;
    PerlinNoise noise;
    int minX; int maxX;
    #endregion
    #region Constant Declaration
    private const string SKY = "0000"; 
    private const string DIRT = "0001";
    private const string STONE = "0010"; 
    private const string WATER = "0011";
    private const string FLOWERS = "0100"; 
    private const string LILYPADS = "0101";
    private const string GRASS = "0110"; 
    private const string ROCK = "0111";
    private const string TREE = "1000";
    #endregion
    #region Methods
    public override void RunManager()
    {
        bitmap = gameManager.grid;
        int seed = gameManager.seed;
        int smoothness = gameManager.smoothness;
        noise = new PerlinNoise(seed);
        bitmap = LoadBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, SKY);
        bitmap = GenerateGround(bitmap, seed, smoothness);
        bitmap = GenerateWater(bitmap, seed);
        bitmap = GeneratePlants(bitmap, seed, smoothness);
        bitmap = GenerateTreesandRocks(bitmap, seed, smoothness);

        gameManager.validSpawnPoints = GetSpawnPoints(bitmap, smoothness);
        player.transform.position = SetPlayerPosition(bitmap);
    }
    private Grid<Node> LoadBitmap(Grid<Node> bitmap, int startX, int startY, int endX, int endY, string binaryValue)
    {
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Node node = bitmap.GetGridObject(x, y);
                node.BinaryValue = binaryValue;
            }
        }
        return bitmap;
    }
    private Grid<Node> GenerateGround(Grid<Node> bitmap, int seed, int smoothness)
    {
        for (int x = 0; x < bitmap.Width; x++)
        {
            System.Random random = new System.Random(seed);
            double n = System.Math.Round((double)x / 10, 1);
            int perlinHeight = Convert.ToInt32(noise.GenerateNoise(n / smoothness) * bitmap.Height) + 12;
            bitmap = LoadBitmap(bitmap, x, 0, x+1, perlinHeight, DIRT);
            bitmap = LoadBitmap(bitmap, x, perlinHeight - 1, x + 1, perlinHeight, GRASS);
            bitmap = LoadBitmap(bitmap, x, 0, x + 1, perlinHeight - 10 - random.Next(1, 4), STONE);
        }
        return bitmap;
    }
    private Grid<Node> GenerateWater(Grid<Node> bitmap, int seed)
    {
        int count = 0; int xCoord = 0;
        System.Random random = new System.Random(seed);

        var gapLengths = new List<(int, int)>();

        for (int x = 0; x < bitmap.Width; x++)
        {
            if (bitmap.GetGridObject(x, waterLevel).BinaryValue == SKY)
            {
                if (x > 0 && (bitmap.GetGridObject(x - 1, waterLevel).BinaryValue == DIRT || bitmap.GetGridObject(x - 1, waterLevel).BinaryValue == STONE || bitmap.GetGridObject(x - 1, waterLevel).BinaryValue == GRASS))
                    xCoord = x;
                count++;
            }
            else
            {
                if (count != 0)
                    gapLengths.Add((xCoord, count));
                count = 0;
            }
        }
        foreach ((int, int) gap in  gapLengths)
        {
            if (gap.Item2 >= 3 && gap.Item2 <= 35)
            {
                bitmap = LoadBitmap(bitmap, gap.Item1, waterLevel - 6, gap.Item1 + gap.Item2, waterLevel, WATER);
                bitmap = LoadBitmap(bitmap, gap.Item1, 12 - random.Next(1, 3), gap.Item1 + gap.Item2, waterLevel - 6, DIRT);
            }
        }
        return bitmap;
    }
    private List<Node> GetSpawnPoints(Grid<Node> bitmap, int smoothness)
    {
        List<Node> spawnPoints = new List<Node>();
        for (int x = 0; x < bitmap.Width; x++)
        {
            if (bitmap.GetGridObject(x, waterLevel - 4).BinaryValue != WATER)
            {
                double n = System.Math.Round((double)x / 10, 1);
                int perlinHeight = Convert.ToInt32(noise.GenerateNoise(n / smoothness) * bitmap.Height) + 12;
                spawnPoints.Add(new Node(bitmap, x, perlinHeight));
            }
        }
        return spawnPoints;
    }
    private Vector3 SetPlayerPosition(Grid<Node> bitmap)
    {
        System.Random random = new System.Random();
        Node spawnNode = bitmap.GetGridObject(0, 0);
        int n;

        bool isNotSpawnable = true;
        do
        {
            n = random.Next(40000, 60000);

            for (int i = 0; i < gameManager.validSpawnPoints.Count; i++)
            {
                if (gameManager.validSpawnPoints[i].x == n)
                {
                    spawnNode = gameManager.validSpawnPoints[i];
                    isNotSpawnable = false;
                    break;
                }
            }
        } while (isNotSpawnable == true);

        Vector3 spawnPosition = bitmap.GetWorldPosition(spawnNode.x, spawnNode.y);
        return new Vector3(spawnPosition.x + 0.08f, spawnPosition.y + 0.3f);
    }
    private Grid<Node> GeneratePlants(Grid<Node> bitmap, int seed, int smoothness)
    {
        System.Random random = new System.Random(seed);
        bool isWater = false;
        int plantChance;

        for (int x = 0; x < bitmap.Width; x++)
        {
            plantChance = random.Next(1, 5);
            double n = System.Math.Round((double)x / 10, 1);
            int perlinHeight = Convert.ToInt32(noise.GenerateNoise(n / smoothness) * bitmap.Height) + 12;

            if (bitmap.GetGridObject(x, waterLevel - 3).BinaryValue == WATER)
                isWater = true;
            else
                isWater = false;

            if (plantChance == 4)
            {
                if (isWater == true)
                    bitmap = LoadBitmap(bitmap, x, waterLevel - 1, x + 1, waterLevel, LILYPADS);
                if (isWater == false)
                    bitmap = LoadBitmap(bitmap, x, perlinHeight, x + 1, perlinHeight + 1, FLOWERS);
            }
        }
        return bitmap;
    }
    private Grid<Node> GenerateTreesandRocks(Grid<Node> bitmap, int seed, int smoothness)
    {
        System.Random random = new System.Random(seed);
        int xCoord = 0;
        List<int> perlinHeights = new List<int>();
        var gapLengths = new List<(int, int)>();

        for (int x = 0; x < bitmap.Width; x++)
        {
            double n = System.Math.Round((double)x / 10, 1);
            int perlinHeight = Convert.ToInt32(noise.GenerateNoise(n / smoothness) * bitmap.Height) + 12;
            perlinHeights.Add(perlinHeight);
        }
        for (int x = 0; x < perlinHeights.Count; x++)
        {
            if (x > 0)
            {
                if (perlinHeights[x - 1] != perlinHeights[x])
                {
                    xCoord = x;
                }
            }
            if (x < perlinHeights.Count)
            {
                try
                {
                    gapLengths.Add((xCoord, 0));
                }
                catch
                {
                    break;
                }
            }
        }
        gapLengths = gapLengths.Distinct().ToList();
        for (int i = 0; i < gapLengths.Count; i++)
        {
            try
            {
                (int, int)[] temp = gapLengths.ToArray();
                temp[i].Item2 = temp[i + 1].Item1 - temp[i].Item1;
                gapLengths = temp.ToList();
            }
            catch
            {
                (int, int)[] temp = gapLengths.ToArray();
                temp[i].Item2 = perlinHeights.Count - temp[i].Item1;
                gapLengths = temp.ToList();
            }
        }
        for (int x = 0; x < gapLengths.Count; x++)
        {
            bool isWater = false;
            for (int i = gapLengths[x].Item1; i < gapLengths[x].Item1 + gapLengths[x].Item2; i++)
            {
                if (bitmap.GetGridObject(i, waterLevel - 2).BinaryValue == WATER)
                {
                    isWater = true;
                }
            }
            if (isWater == false)
            {
                if (gapLengths[x].Item2 >= 6 && gapLengths[x].Item2 < 9)
                {
                    int midpoint = gapLengths[x].Item1 + gapLengths[x].Item2 / 2 + 1;
                    int chance = random.Next(1, 8);

                    if (chance != 5)
                    {
                        bitmap = LoadBitmap(bitmap, midpoint - 2, perlinHeights[gapLengths[x].Item1], midpoint + 2, perlinHeights[gapLengths[x].Item1] + 3, ROCK);
                        bitmap = LoadBitmap(bitmap, midpoint - 1, perlinHeights[gapLengths[x].Item1] + 3, midpoint + 2, perlinHeights[gapLengths[x].Item1] + 4, ROCK);
                        bitmap = LoadBitmap(bitmap, midpoint - 1, perlinHeights[gapLengths[x].Item1] + 4, midpoint + 1, perlinHeights[gapLengths[x].Item1] + 5, ROCK);
                    }
                }
                else if (gapLengths[x].Item2 >= 9)
                {
                    int midpoint = gapLengths[x].Item1 + gapLengths[x].Item2 / 2;

                    bitmap = LoadBitmap(bitmap, midpoint - 2, perlinHeights[gapLengths[x].Item1], midpoint + 3, perlinHeights[gapLengths[x].Item1] + 1, TREE);
                    bitmap = LoadBitmap(bitmap, midpoint - 2, perlinHeights[gapLengths[x].Item1] + 1, midpoint + 2, perlinHeights[gapLengths[x].Item1] + 2, TREE);
                    bitmap = LoadBitmap(bitmap, midpoint - 3, perlinHeights[gapLengths[x].Item1] + 2, midpoint + 4, perlinHeights[gapLengths[x].Item1] + 5, TREE);
                    bitmap = LoadBitmap(bitmap, midpoint - 2, perlinHeights[gapLengths[x].Item1] + 5, midpoint + 3, perlinHeights[gapLengths[x].Item1] + 6, TREE);
                }
            }
        }
        return bitmap;
    }
    #endregion
}