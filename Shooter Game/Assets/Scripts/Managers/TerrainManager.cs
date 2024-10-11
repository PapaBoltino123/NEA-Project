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

public class TerrainManager : Singleton<TerrainManager>
{
    #region Variable Declaration
    [SerializeField] int seed, smoothness, waterLevel;
    [SerializeField] TileBase[] mainTiles, treeTiles, rockTiles;

    [NonSerialized] public int chunkWidth = 64;
    [NonSerialized] public int chunkHeight = 128;
    [NonSerialized] public int worldWidth = 131072;
    [NonSerialized] public int worldHeight = 128;
    [NonSerialized] public int[] surfaceHeights;

    private PerlinNoise perlinNoise;
    private Grid<Node> map;
    private float cellSize = 0.16f;
    private List<Node> rockList;
    private List<Node> treeList;
    #endregion
    #region Methods
    private void Awake()
    {
        seed = SetSeed();
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
        rockList = new List<Node>();
        treeList = new List<Node>();
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

            LoadMapData(x, 0, x + 1, perlinHeight, TileType.DIRT);
            LoadMapData(x, perlinHeight - 1, x + 1, perlinHeight, TileType.GRASS);
            LoadMapData(x, 0, x + 1, perlinHeight - 10 - rng.Next(1, 4), TileType.STONE);
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
                    LoadMapData(x, surfaceHeights[x], x + 1, surfaceHeights[x] + 1, TileType.FLOWER);
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
        gapLengths = gapLengths.Distinct().ToList();
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
                if (gapLengths[x].count >= 6 && gapLengths[x].count < 9)
                {
                    int midpoint = gapLengths[x].xCoord + gapLengths[x].count / 2 + 1;
                    int chance = rng.Next(1, 8);

                    if (chance != 5)
                    {
                        LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord], midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 3, TileType.ROCK);
                        LoadMapData(midpoint - 1, surfaceHeights[gapLengths[x].xCoord] + 3, midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 4, TileType.ROCK);
                        LoadMapData(midpoint - 1, surfaceHeights[gapLengths[x].xCoord] + 4, midpoint + 1, surfaceHeights[gapLengths[x].xCoord] + 5, TileType.ROCK);
                    }
                }
                else if (gapLengths[x].count >= 9)
                {
                    int midpoint = gapLengths[x].xCoord + gapLengths[x].count / 2;

                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord], midpoint + 3, surfaceHeights[gapLengths[x].xCoord] + 1, TileType.TREE);
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord] + 1, midpoint + 2, surfaceHeights[gapLengths[x].xCoord] + 2, TileType.TREE);
                    LoadMapData(midpoint - 3, surfaceHeights[gapLengths[x].xCoord] + 2, midpoint + 4, surfaceHeights[gapLengths[x].xCoord] + 5, TileType.TREE);
                    LoadMapData(midpoint - 2, surfaceHeights[gapLengths[x].xCoord] + 5, midpoint + 3, surfaceHeights[gapLengths[x].xCoord] + 6, TileType.TREE);
                }
            }
        }
        #endregion
    }
    private string GetTileData(TileType tileType)
    {
        switch (tileType)
        {
            case TileType.SKY:
                return "0000";
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
                return null;
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
            }
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
            n = random.Next(45000, 55000);
            Node node = map.GetGridObject(n, surfaceHeights[n]);
            if (validSpawnNodes.Contains(node))
            {
                spawnPosition = map.GetWorldPosition(node.x, node.y);
                isInvalidSpawn = false;
            }

        } while (isInvalidSpawn == true);
        return new Vector3(spawnPosition.x + 0.08f, spawnPosition.y + 0.2f);
    }
    #endregion
    #region Enumerators
    public IEnumerator GenerateChunk(Chunk chunk)
    {
        for (int w = 0; w < chunkWidth; w++)
        {
            for (int h = 0; h < chunkHeight; h++)
            {
                Vector3Int tilePosition = new Vector3Int(chunk.Position.x + w, chunk.Position.y + h, 0);
                if ((tilePosition.x < 0 || tilePosition.x >= worldWidth) || tilePosition.y < 0 || tilePosition.y >= worldHeight)
                    continue;

                chunk.SetChunkTile(tilePosition, TileMapType.GROUND_COLLISIONS, ReturnTile(map.GetGridObject(tilePosition.x, tilePosition.y), TileMapType.GROUND_COLLISIONS));
                chunk.SetChunkTile(tilePosition, TileMapType.GROUND, ReturnTile(map.GetGridObject(tilePosition.x, tilePosition.y), TileMapType.GROUND));
                chunk.SetChunkTile(tilePosition, TileMapType.DECORATIONS, ReturnTile(map.GetGridObject(tilePosition.x, tilePosition.y), TileMapType.DECORATIONS));
                chunk.SetChunkTile(tilePosition, TileMapType.OTHER_COLLISIONS, ReturnTile(map.GetGridObject(tilePosition.x, tilePosition.y), TileMapType.OTHER_COLLISIONS));
            }
            yield return null;
        }
        yield break;
    }
    private TileBase ReturnTile(Node node, TileMapType tileMapType)
    {
        if (tileMapType == TileMapType.GROUND)
        {
            if (node.TileData == "0000")
                return mainTiles[0];
            else if (node.TileData == "0001")
                return mainTiles[1];
            else if (node.TileData == "0010")
                return mainTiles[2];
            else if (node.TileData == "0011")
                return mainTiles[3];
            else if (node.TileData == "0100")
                return mainTiles[4];
            else if (node.TileData == "0101")
                return mainTiles[5];
            else if (node.TileData == "0110")
                return mainTiles[0];
            else if (node.TileData == "0111")
                return mainTiles[0];
            else if (node.TileData == "1000")
                return mainTiles[0];
            else
                return null;
        }
        else if (tileMapType == TileMapType.DECORATIONS)
        {
            if (node.TileData == "0110")
                return mainTiles[6];
            else if (node.TileData == "0111")
            {
                try
                {
                    node.TreeTileType = treeList.Count;
                    
                    if (node.TreeTileType < treeTiles.Length)
                    {
                        treeList.Add(node);
                    }
                    else if (node.TreeTileType > treeTiles.Length)
                    {
                        treeList.Clear();
                    }

                    return treeTiles[node.TreeTileType];
                }
                catch 
                {
                    Debug.Log($"Tree list count: {treeList.Count}");
                    Debug.Log($"Tree tile type: {node.TreeTileType}");
                    throw new Exception("Tree won't fucking work");
                }
            }
            else if (node.TileData == "1000")
            {
                try
                {
                    node.RockTileType = rockList.Count;

                    if (node.RockTileType < rockTiles.Length)
                    {
                        rockList.Add(node);
                    }
                    else if (node.RockTileType > rockTiles.Length)
                    {
                        rockList.Clear();
                    }

                    return rockTiles[node.RockTileType];
                }
                catch
                {
                    Debug.Log($"Rock list count: {rockList.Count}");
                    Debug.Log($"Tree tile type: {node.RockTileType}");
                    throw new Exception("Rock won't fucking work");
                }
            }
            else
                return null;
        }
        else if (tileMapType == TileMapType.GROUND_COLLISIONS)
        {
            if (node.TileData == "0000")
                return mainTiles[0];
            else if (node.TileData == "0001")
                return mainTiles[1];
            else if (node.TileData == "0010")
                return mainTiles[2];
            else if (node.TileData == "0011")
                return mainTiles[3];
            else if (node.TileData == "0101")
                return mainTiles[5];
            else if (node.TileData == "1000")
                return mainTiles[0];
            else
                return null;
        }
        else if (tileMapType == TileMapType.OTHER_COLLISIONS)
        {
            if (node.TileData == "0100")
                return mainTiles[0];
            else if (node.TileData == "0111")
                return mainTiles[1];
            else
                return null;
        }
        else
            return null;
    }
    #endregion
}