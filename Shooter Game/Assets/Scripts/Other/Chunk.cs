using System;
using System.AdditionalDataStructures;
using System.Algorithms.TerrainGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    #region Variable Declaration
    [SerializeField] public Tilemap ground, decorations, groundCollisions, otherCollisions;
    [SerializeField] TileBase[] mainTiles, treeTiles, rockTiles;

    private int chunkHeight;
    private int chunkWidth;
    private Grid<Node> chunkMap;
    private Grid<Node> fullMap;
    private Vector2Int chunkMapPosition;
    private bool isUnloading = false;
    #endregion
    #region Methods
    private void Start()
    {
        chunkHeight = TerrainManager.Instance.chunkHeight;
        chunkWidth = TerrainManager.Instance.chunkWidth;
        chunkMap = new Grid<Node>(chunkWidth, chunkHeight, 0.16f, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        fullMap = TerrainManager.Instance.ReturnWorldMap();
        chunkMapPosition = new Vector2Int(fullMap.GetXY(transform.position).x, 0);

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                int mapX = chunkMapPosition.x + x;
                Node mapNode = fullMap.GetGridObject(mapX, y);
                chunkMap.SetGridObject(x, y, mapNode);
            }
        }

        LoadChunk();
    }
    private void LoadChunk()
    {
        StartCoroutine(PerformRenderChunk());
    }
    public void UnloadChunk()
    {
        Destroy(gameObject);
        isUnloading = true;
    }
    public IEnumerator PerformRenderChunk()
    {
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                Node node = chunkMap.GetGridObject(x, y);
                TileBase groundTile = GetTile(node, TileMapType.GROUND);
                TileBase collisionsTile = GetTile(node, TileMapType.GROUND_COLLISIONS);
                TileBase decorationsTile = GetTile(node, TileMapType.DECORATIONS);
                TileBase otherTile = GetTile(node, TileMapType.OTHER_COLLISIONS);

                groundCollisions.SetTile(new Vector3Int(x, y), collisionsTile);
                ground.SetTile(new Vector3Int(x, y), groundTile);
                decorations.SetTile(new Vector3Int(x, y), decorationsTile);
                otherCollisions.SetTile(new Vector3Int(x, y), otherTile);

                if (y % chunkHeight == 0)
                    yield return null;
            }
        }
    }
    private TileBase GetTile(Node node, TileMapType tileMapType)
    {
        if (tileMapType == TileMapType.GROUND)
        {
            if (node.TileData == "0001")
                return mainTiles[1];
            else if (node.TileData == "0010")
                return mainTiles[2];
            else if (node.TileData == "0011")
                return mainTiles[3];
            else if (node.TileData == "0100")
                return mainTiles[4];
            else if (node.TileData == "0101")
                return mainTiles[4];
            else
                return mainTiles[0];
        }
        else if (tileMapType == TileMapType.DECORATIONS)
        {
            if (node.TileData == "0110")
                return mainTiles[6];
            else if (node.TileData == "0101")
                return mainTiles[5];
            else if (node.TileData == "0111")
            {
                try
                {
                    return treeTiles[node.TreeTileType];
                }
                catch
                {
                    Debug.Log($"Tree tile type: {node.TreeTileType}");
                    throw new Exception("Tree won't fucking work");
                }
            }
            else if (node.TileData == "1000")
            {
                try
                {
                    return rockTiles[node.RockTileType];
                }
                catch
                {
                    Debug.Log($"Tree tile type: {node.RockTileType}");
                    throw new Exception("Rock won't fucking work");
                }
            }
            else
                return null;
        }
        else if (tileMapType == TileMapType.GROUND_COLLISIONS)
        {
            if (node.TileData == "0001")
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
