using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.TerrainGeneration;

public class ChunkManager : Singleton<ChunkManager>
{
    #region Variables Declaration
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private GameObject chunkRoot;
    [SerializeField] LayerMask chunkLayer;
    private bool isUpdatingChunks = false;
    [NonSerialized] public List<Node> validSpawnPoints;
    private Vector3 playerSpawnPosition;
    [NonSerialized] public List<Chunk> chunksToLoad;
    private const int LEFTRANGE = 2;
    private const int RIGHTRANGE = 1;
    #endregion
    #region Methods
    private void Start()
    {
        validSpawnPoints = TerrainManager.Instance.ValidSpawns();
        playerSpawnPosition = TerrainManager.Instance.SetPlayerPosition(validSpawnPoints);
        chunksToLoad = new List<Chunk>();

        StartCoroutine(LoadChunks());
        StartCoroutine(UnloadChunks());

        Player.Instance.transform.position = playerSpawnPosition;
    }
    public void ClearAllChunks()
    {
        StopAllCoroutines();

        List<Chunk> chunksToUnload = new List<Chunk>();
        foreach (Transform child in chunkRoot.transform)
        {
            Chunk chunk = child.GetComponent<Chunk>();

            if (chunk != null)
                chunksToUnload.Add(chunk);
        }
        foreach (Chunk chunk in chunksToUnload)
        {
            if (chunk != null)
                chunk.UnloadChunk();
        }
        StartCoroutine(LoadChunks());
        StartCoroutine(UnloadChunks());
    }
    public Chunk GetChunk(Vector3Int position)
    {
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(position.x + 0.5f, position.y + 0.5f), Vector2.zero, 0f, chunkLayer);

        if (hit == true)
            return hit.collider.GetComponent<Chunk>();
        else
            return null;
    }
    private Rect GetChunkLoadBounds()
    {
        Vector3 regionStart = playerSpawnPosition + Vector3.left * LEFTRANGE;
        Vector3 regionEnd = playerSpawnPosition + Vector3.right * RIGHTRANGE;

        int startX = (int)regionStart.x / TerrainManager.Instance.chunkWidth;
        int startY = (int)regionStart.y / TerrainManager.Instance.chunkHeight;
        int endX = ((int)regionEnd.x + TerrainManager.Instance.chunkWidth) / TerrainManager.Instance.chunkWidth;
        int endY = ((int)regionEnd.y + TerrainManager.Instance.chunkHeight) / TerrainManager.Instance.chunkHeight;

        return new Rect(startX, startY, endX - startX, endY - startY);
    }
    #endregion
    #region Enumerators
    private IEnumerator LoadChunks()
    {
        while (true)
        {
            isUpdatingChunks = true;
            yield return StartCoroutine(PerformLoadChunks());
            isUpdatingChunks = false;
            yield return null;
        }
    }
    private IEnumerator UnloadChunks()
    {
        while (true)
        {
            if (!isUpdatingChunks)
                yield return StartCoroutine(PerformUnloadChunks());
            yield return null;
        }
    }
    private IEnumerator PerformLoadChunks()
    {
        Rect loadBoundaries = GetChunkLoadBounds();
        
        for (int w = (int)loadBoundaries.xMax; w >= (int)loadBoundaries.xMin; w--)
        {
            for (int h = (int)loadBoundaries.yMax; h >= (int)loadBoundaries.yMin; h--)
            {
                if (w < 0 || w >= TerrainManager.Instance.worldWidth / TerrainManager.Instance.chunkWidth || h < 0 || h >= TerrainManager.Instance.worldHeight / TerrainManager.Instance.chunkHeight)
                    continue;

                Vector3Int chunkPosition = new Vector3Int(w, h, 0);
                Vector3Int worldPosition = new Vector3Int(w * TerrainManager.Instance.chunkWidth, h * TerrainManager.Instance.chunkHeight, 0);

                if (loadBoundaries.Contains(chunkPosition) && GetChunk(worldPosition) == false)
                {
                    chunksToLoad.Add(Instantiate(chunkPrefab, worldPosition, Quaternion.identity, chunkRoot.transform).GetComponent<Chunk>());
                    yield return null;
                }
            }
        }
    }
    private IEnumerator PerformUnloadChunks()
    {
        Rect loadBoundaries = GetChunkLoadBounds();
        List<Chunk> chunksToUnload = new List<Chunk>();
        foreach (Transform child in chunkRoot.transform)
        {
            Chunk chunk = child.GetComponent<Chunk>();
            if (chunk != null)
            {
                if (loadBoundaries.Contains(chunk.ChunkPosition) == false)
                    chunksToUnload.Add(chunk);
            }
        }

        foreach (Chunk chunk in chunksToUnload)
        {
            while (isUpdatingChunks == true)
                yield return null;


            if (chunk != null)
                chunk.UnloadChunk();
            yield return null;
        }
    }
    #endregion
}
