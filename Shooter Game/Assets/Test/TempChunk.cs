using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using UnityEngine.Tilemaps;

public class TempChunk : MonoBehaviour
{
    private int chunkHeight;
    private int chunkWidth;
    private Grid<Node> chunkMap;
    private Grid<Node> fullMap;
    private Vector2Int chunkPosition;
    [SerializeField] Tilemap tilemap;
    [SerializeField] TileBase dirt, sky;

    void Start()
    {
        chunkHeight = TestingChunks.Instance.chunkHeight;
        chunkWidth = TestingChunks.Instance.chunkWidth;
        chunkMap = new Grid<Node>(chunkWidth, chunkHeight, 0.16f, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        fullMap = TestingChunks.Instance.ReturnWorldMap();
        chunkPosition = new Vector2Int(fullMap.GetXY(transform.position).x, 0);

        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                int mapX = chunkPosition.x + x;
                Node mapNode = fullMap.GetGridObject(mapX, y);
                chunkMap.SetGridObject(x, y, mapNode);
            }
        }
        StartCoroutine(PerformRenderChunk());
    }
    public IEnumerator PerformRenderChunk()
    {
        for (int x = 0; x < chunkWidth; x++)
        {
            for (int y = 0; y < chunkHeight; y++)
            {
                Node node = chunkMap.GetGridObject(x, y);
                Debug.Log(node.TileData);

                if (node.TileData == "DIRT")
                    tilemap.SetTile(new Vector3Int(x, y), dirt);
                else if (node.TileData == "SKY")
                    tilemap.SetTile(new Vector3Int(x, y), sky);

                if (y % chunkHeight == 0)
                    yield return null;
            }
        }
    }
    public void UnloadChunk()
    {
        Destroy(gameObject);
    }
}
