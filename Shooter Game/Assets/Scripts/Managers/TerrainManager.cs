using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.TerrainGeneration;
using System.Net;
using Unity.Jobs;
using Unity.Mathematics;
using static UnityEditor.PlayerSettings.WSA;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine.UIElements;

public class TerrainManager : Singleton<TerrainManager>
{
    #region Variable Declaration
    [SerializeField] int seed, smoothness, waterLevel;

    [NonSerialized] public int chunkWidth = 64;
    [NonSerialized] public int chunkHeight = 128;
    [NonSerialized] public int rockCount = 0;
    [NonSerialized] public int treeCount = 0;
    [NonSerialized] public int worldWidth = 8192;
    [NonSerialized] public int worldHeight = 128;
    [NonSerialized] public int[] surfaceHeights;

    private PerlinNoise perlinNoise;
    private Grid<Node> map;
    public float cellSize = 0.16f;
    #endregion
    #region Methods
    private void Awake()
    {
        //seed = SetSeed();
        seed = -359;
        Initialize();
    }
    private static int SetSeed()
    {
        System.Random rng = new System.Random();
        int seed = rng.Next(-10000, 10000);
        return seed;
    }
    public void Initialize()
    {
        map = new Grid<Node>(worldWidth, worldHeight, cellSize, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        perlinNoise = new PerlinNoise(seed);
        surfaceHeights = new int[worldWidth];
        LoadMapData(0, 0, worldWidth, worldHeight, TileType.SKY);
        GenerateWorld();
    }
    private void GenerateWorld()
    {
        System.Random rng = new System.Random(seed);
        #region Generate Ground
        for (int x = 0; x < worldWidth; x++)
        {
            double n = System.Math.Round((double)x / 10, 2);
            int perlinHeight = Convert.ToInt32(perlinNoise.GenerateNoise(n / smoothness) * worldHeight) + 12;
            surfaceHeights[x] = perlinHeight;

            LoadMapData(x, 0, x + 1, surfaceHeights[x] - 1, TileType.DIRT);
            LoadMapData(x, surfaceHeights[x] - 1, x + 1, surfaceHeights[x], TileType.GRASS);
            LoadMapData(x, 0, x + 1, surfaceHeights[x] - 10 - rng.Next(1, 4), TileType.STONE);
        }
        #endregion
        #region Generate Water
        int count = 0; int xCoord = 0;
        var gapLengths = new List<(int xCoord, int count)>();

        for (int x = 0; x < worldWidth; x++)
        {
            if (map.GetGridObject(x, waterLevel).TileData == GetTileData(TileType.SKY))
            {
                if (x > 0 && (map.GetGridObject(x - 1, waterLevel).TileData == GetTileData(TileType.DIRT) || map.GetGridObject(x - 1, waterLevel).TileData == GetTileData(TileType.STONE) || map.GetGridObject(x - 1, waterLevel).TileData == GetTileData(TileType.GRASS)))
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
        foreach (var gap in gapLengths)
        {
            if (gap.count >= 3 && gap.count <= 35)
            {
                LoadMapData(gap.xCoord, waterLevel - 6, gap.xCoord + gap.count, waterLevel, TileType.WATER);
                LoadMapData(gap.Item1, 12 - rng.Next(1, 3), gap.Item1 + gap.Item2, waterLevel - 6, TileType.DIRT);
            }
        }
        #endregion
        #region Generate Plants
        bool isWater = false;
        int plantChance;
        for (int x = 0; x < worldWidth; x++)
        {
            plantChance = rng.Next(1, 5);

            if (map.GetGridObject(x, waterLevel - 3).TileData == GetTileData(TileType.WATER))
                isWater = true;
            else
                isWater = false;

            if (plantChance == 4)
            {
                if (isWater == true)
                    LoadMapData(x, waterLevel - 1, x + 1, waterLevel, TileType.LILYPAD);
                else if (isWater == false)
                {
                    LoadMapData(x, surfaceHeights[x], x + 1, surfaceHeights[x] + 1, TileType.FLOWER);
                }
            }
        }
        #endregion
        #region Generate Trees and Rocks
        xCoord = 0;
        gapLengths.Clear(); 

        for (int x = 0; x < surfaceHeights.Length; x++)
        {
            if (x > 0)
            {
                if (surfaceHeights[x - 1] != surfaceHeights[x])
                {
                    xCoord = x;
                }
            }
            if (x < surfaceHeights.Length)
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
        for (int i = 0; i < gapLengths.Count; i++)
        {
            try
            {
                (int xCoord, int count)[] temp = gapLengths.ToArray();
                temp[i].count = temp[i + 1].xCoord - temp[i].xCoord;
                gapLengths = temp.ToList();
            }
            catch
            {
                (int xCoord, int count)[] temp = gapLengths.ToArray();
                temp[i].count = surfaceHeights.Length - temp[i].xCoord;
                gapLengths = temp.ToList();
            }
        }
        gapLengths = gapLengths.Distinct().ToList();
        for (int x = 0; x < gapLengths.Count; x++)
        {
            isWater = false;
            for (int i = gapLengths[x].Item1; i < gapLengths[x].Item1 + gapLengths[x].Item2; i++)
            {
                if (map.GetGridObject(i, waterLevel - 2).TileData == GetTileData(TileType.WATER))
                {
                    isWater = true;
                }
            }
            if (isWater == false)
            {
                if (gapLengths[x].count >= 5 && gapLengths[x].count < 8 && gapLengths[x].count % 2 == 1)
                {
                    int midpoint = gapLengths[x].xCoord + gapLengths[x].count / 2 + 1;

                    rockCount = 0;
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord], midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 3, TileType.ROCK);
                    LoadMapData(midpoint - 1, surfaceHeights[gapLengths[x].xCoord] + 3, midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 4, TileType.ROCK);
                    LoadMapData(midpoint - 1, surfaceHeights[gapLengths[x].xCoord] + 4, midpoint + 1, surfaceHeights[gapLengths[x].xCoord] + 5, TileType.ROCK);
                }
                else if (gapLengths[x].count >= 8 && gapLengths[x].count % 2 == 1)
                {
                    int midpoint = gapLengths[x].xCoord + gapLengths[x].count / 2;
                    if (gapLengths[x].xCoord + gapLengths[x].count / 2 % 2 == 0)
                        midpoint = gapLengths[x].xCoord + gapLengths[x].count / 2 + 1;

                    treeCount = 0;
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord], midpoint + 3, surfaceHeights[gapLengths[x].xCoord] + 1, TileType.TREE);
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord] + 1, midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 2, TileType.TREE);
                    LoadMapData(midpoint - 3, surfaceHeights[gapLengths[x].xCoord] + 2, midpoint + 4, surfaceHeights[gapLengths[x].xCoord] + 5, TileType.TREE);
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord] + 5, midpoint + 3, surfaceHeights[gapLengths[x].xCoord] + 6, TileType.TREE);
                }
            }
        }
        #endregion
    }
    public Grid<Node> ReturnWorldMap()
    {
        return map;
    }
    private string GetTileData(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.STONE:
                return "0001";
            case TileType.DIRT:
                return "0010";
            case TileType.GRASS:
                return "0011";
            case TileType.WATER:
                return "0100";
            case TileType.LILYPAD:
                return "0101";
            case TileType.FLOWER:
                return "0110";
            case TileType.TREE:
                return "0111";
            case TileType.ROCK:
                return "1000";
            default:
                return "0000";
        }
    }
    private void LoadMapData(int startX, int startY, int endX, int endY, TileType tileType)
    {
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                Node node = map.GetGridObject(x, y);
                node.TileData = GetTileData(tileType);

                if (node.TileData == GetTileData(TileType.ROCK))
                    node.RockTileType = GetRockOrTreeType(node);
                else if (node.TileData == GetTileData(TileType.TREE))
                    node.TreeTileType = GetRockOrTreeType(node);
                else
                {
                    node.RockTileType = 100;
                    node.TreeTileType = 100;
                }
;           }
        }
    }
    public List<Node> ValidSpawns()
    {
        List<Node> validSpawnNodes = new List<Node>();

        for (int x = 0; x < worldWidth; x++)
        {
            if (map.GetGridObject(x, waterLevel - 3).TileData != GetTileData(TileType.WATER))
            {
                if (map.GetGridObject(x, surfaceHeights[x]).TileData != GetTileData(TileType.ROCK))
                {
                    Node node = map.GetGridObject(x, surfaceHeights[x]);
                    validSpawnNodes.Add(node);
                }
            }
        }
        return validSpawnNodes;
    }
    public Vector3 SetPlayerPosition(List<Node> validSpawnNodes)
    {
        System.Random random = new System.Random();
        Vector3 spawnPosition = new Vector3();
        int n = 0; bool isInvalidSpawn = true;

        do
        {
            n = random.Next((worldWidth / 2) - 128, (worldWidth / 2) + 128);
            Node node = map.GetGridObject(n, surfaceHeights[n]);
            if (validSpawnNodes.Contains(node))
            {
                spawnPosition = map.GetWorldPosition(node.x, node.y);
                isInvalidSpawn = false;
            }

        } while (isInvalidSpawn == true);
        return new Vector3(spawnPosition.x + 0.08f, spawnPosition.y + 15f);
    }
    private int GetRockOrTreeType(Node node)
    {
        if (node.TileData == GetTileData(TileType.ROCK))
        {
            int type = rockCount;
            rockCount++;
            return type;
        }
        else if (node.TileData == GetTileData(TileType.TREE))
        {
            int type = treeCount;
            treeCount++;
            return type;
        }
        else
            return 0;
    }
    #endregion
}