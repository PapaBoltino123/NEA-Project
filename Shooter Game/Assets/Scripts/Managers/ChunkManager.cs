using System.Algorithms;
using System.Algorithms.TerrainGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkManager : Manager
{
    #region Constant Declaration
    private const string SKY = "0000"; private const int SKYTILE = 0;
    private const string DIRT = "0001"; private const int DIRTTILE = 1;
    private const string STONE = "0010"; private const int STONETILE = 2;
    private const string WATER = "0011"; private const int WATERTILE = 4;
    private const string FLOWERS = "0100"; private const int FLOWERSTILE = 3;
    private const string LILYPADS = "0101"; private const int LILYPADSTILE = 5;
    private const string GRASS = "0110"; private const int GRASSTILE = 6;
    private const string ROCK = "0111"; private const string TREE = "1000";
    #endregion
    #region Variable Declaration
    [SerializeField] TerrainManager terrainManager;
    [SerializeField] TileBase[] tiles, rock, tree;
    [SerializeField] Tilemap ground, decoration, collisions;
    [SerializeField] private Player player;
    private List<Chunk> totalChunks;
    private CustomPriorityQueue<Chunk> loadedChunks; 
    private int chunkWidth = 64;
    private int chunkHeight = 128;
    #endregion
    #region Methods
    private void Awake()
    {
        totalChunks = new List<Chunk>();
        for (int i = 0; i < 131072 / chunkWidth; i++)
        {
            totalChunks.Add(new Chunk(chunkWidth, chunkHeight, terrainManager.bitmap, (chunkWidth * i), chunkWidth + chunkWidth * (chunkWidth * i)));
        }
        loadedChunks = new CustomPriorityQueue<Chunk>(131072 / chunkWidth);
    }
    private void Update()
    {
        RunManager();
    }
    public override void RunManager()
    {
        int playerX = terrainManager.bitmap.GetXY(player.transform.position).x;
        int currentChunkIndex = playerX / chunkWidth;
        int nextChunkIndexLeft = currentChunkIndex - 1;
        int nextChunkIndexRight = currentChunkIndex + 1;

        LoadChunks(currentChunkIndex, nextChunkIndexLeft, nextChunkIndexRight);
        RenderTilesInLoadedChunks();
        RemoveTilesFromUnloadedChunks();
    }
    private void LoadChunks(int currentIndex, int leftIndex, int rightIndex)
    {
        loadedChunks.Clear();
        totalChunks[currentIndex].Index = currentIndex;

        if (currentIndex >= 0 && currentIndex < totalChunks.Count)
        {
            loadedChunks.Enqueue(totalChunks[currentIndex], 0);
        }

        if (leftIndex >= 0 && leftIndex < totalChunks.Count)
        {
            loadedChunks.Enqueue(totalChunks[leftIndex], 1);
        }

        if (rightIndex >= 0 && rightIndex < totalChunks.Count)
        {
            loadedChunks.Enqueue(totalChunks[rightIndex], 1);
        }

        AdjustLoadedChunksToPowerOfTwo();
    }
    private void AdjustLoadedChunksToPowerOfTwo()
    {
        int loadedCount = loadedChunks.Count;
        int powerOfTwo = Mathf.NextPowerOfTwo(loadedCount);

        while (loadedChunks.Count > powerOfTwo)
        {
            loadedChunks.Dequeue();
        }
    }
    private void RenderTilesInLoadedChunks()
    {
        foreach (var chunk in loadedChunks.ToList())
        {
            RenderTiles(chunk, ground, decoration, collisions);
        }
    }
    private void RemoveTilesFromUnloadedChunks()
    {
        foreach (var chunk in totalChunks)
        {
            if (!loadedChunks.ToList().Contains(chunk))
            {
                RemoveTiles(chunk, ground, decoration, collisions);
            }
        }
    }
    private void RemoveTiles(Chunk chunk, Tilemap ground, Tilemap decorations, Tilemap collisions)
    {
        foreach (var tile in chunk.Tiles)
        {
            ground.SetTile(new Vector3Int(tile.x, tile.y), null);
            collisions.SetTile(new Vector3Int(tile.x, tile.y), null);
            decorations.SetTile(new Vector3Int(tile.x, tile.y), null);
        }

        chunk.Tiles.Clear();
    }
    private void RenderTiles(Chunk chunk, Tilemap ground, Tilemap decorations, Tilemap collisions)
    {
        for (int initialX = 0; initialX < chunk.Width; initialX++)
        {
            for (int initialY = 0; initialY < chunk.Height; initialY++)
            {
                int x = initialX + (initialX * chunk.Width); 
                int y = initialY + (initialY * chunk.Height);
                Node node = terrainManager.bitmap.GetGridObject(x, y);

                if (ground.GetTile(new Vector3Int(x, y)) == null)
                {
                    if (node.BinaryValue == SKY)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                    }
                    else if (node.BinaryValue == DIRT)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[DIRTTILE]);
                        collisions.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                    }
                    else if (node.BinaryValue == STONE)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[STONETILE]);
                        collisions.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                    }
                    else if (node.BinaryValue == WATER)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[WATERTILE]);
                    }
                    else if (node.BinaryValue == FLOWERS)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                        decoration.SetTile(new Vector3Int(x, y), tiles[FLOWERSTILE]);
                    }
                    else if (node.BinaryValue == LILYPADS)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[LILYPADSTILE]);
                        collisions.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                    }
                    else if (node.BinaryValue == GRASS)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[GRASSTILE]);
                        collisions.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                    }
                    else if (node.BinaryValue == TREE)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);

                        if (terrainManager.bitmap.GetGridObject(x, y - 1).BinaryValue == GRASS && terrainManager.bitmap.GetGridObject(x - 1, y).BinaryValue == SKY)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                decoration.SetTile(new Vector3Int(x + i, y), tree[i]);
                            }
                            for (int i = 0; i < 4; i++)
                            {
                                decoration.SetTile(new Vector3Int(x + i, y + 1), tree[5 + i]);
                            }
                            for (int i = 0; i < 7; i++)
                            {
                                for (int j = 2; j < 5; j++)
                                {
                                    int n = 7 * (j - 2);
                                    decoration.SetTile(new Vector3Int(x - 1 + i, y + j), tree[9 + n + i]);
                                }
                            }
                            for (int i = 0; i < 5; i++)
                            {
                                decoration.SetTile(new Vector3Int(x + i, y + 5), tree[30 + i]);
                            }
                        }
                    }
                    else if (node.BinaryValue == ROCK)
                    {
                        ground.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);
                        collisions.SetTile(new Vector3Int(x, y), tiles[SKYTILE]);

                        if (terrainManager.bitmap.GetGridObject(x, y - 1).BinaryValue == GRASS && terrainManager.bitmap.GetGridObject(x - 1, y).BinaryValue == SKY)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                for (int j = 0; j < 3; j++)
                                {
                                    int n = 4 * j;
                                    decoration.SetTile(new Vector3Int(x + i, y + j), rock[n + i]);
                                }
                            }
                            for (int i = 0; i < 3; i++)
                            {
                                decoration.SetTile(new Vector3Int(x + 1 + i, y + 3), rock[12 + i]);
                            }
                            for (int i = 0; i < 2; i++)
                            {
                                decoration.SetTile(new Vector3Int(x + 1 + i, y + 4), rock[15 + i]);
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion
}
