using System.Algorithms;
using System.Algorithms.TerrainGeneration;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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
    [SerializeField] TerrainManager gameManager;
    [SerializeField] TileBase[] tiles, rock, tree;
    [SerializeField] Tilemap ground, decoration, collisions;
    [SerializeField] private Player player;
    private int chunkWidth = 64;
    #endregion
    #region Methods

    private void Update()
    {
        RunManager();
    }
    public override void RunManager()
    {
        (int x, int y) playerCoordinates = terrainManager.bitmap.GetXY(player.transform.position);
        int minX = playerCoordinates.x - chunkWidth / 2; int maxX = playerCoordinates.x + chunkWidth / 2;
        RenderTiles(terrainManager.bitmap, minX, maxX);
    }
    private void RemoveTiles(Grid<Node> bitmap, Tilemap ground, Tilemap decorations, Tilemap collisions, int lowerX, int upperX)
    {
        for (int y = 0; y < bitmap.Height; y++)
        {
            ground.SetTile(new Vector3Int(lowerX, y), null);
            decorations.SetTile(new Vector3Int(lowerX, y), null);
            collisions.SetTile(new Vector3Int(lowerX, y), null);
            ground.SetTile(new Vector3Int(upperX, y), null);
            decorations.SetTile(new Vector3Int(upperX, y), null);
            collisions.SetTile(new Vector3Int(upperX, y), null);
        }
    }
    private void RenderTiles(Grid<Node> bitmap, int minX, int maxX)
    {
        for (int x = minX; x < maxX; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
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
