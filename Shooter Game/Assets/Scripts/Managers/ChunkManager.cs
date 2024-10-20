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

    private Grid<Node> worldMap;
    private Vector3 initialWorldChunkPosition;
    private Vector2Int initialMapChunkPosition;
    [NonSerialized] public List<Node> validSpawnPoints;
    private Vector3 playerWorldPosition;
    private Vector2Int playerMapPosition;

    private List<Chunk> chunksToLoad;
    private List<Chunk> chunksToUnload;
    private const int LEFTRANGE = -2;
    private const int RIGHTRANGE = 1;
    #endregion
    #region Methods
    private void Start()
    {
        chunksToLoad = new List<Chunk>();
        chunksToUnload = new List<Chunk>();

        worldMap = TerrainManager.Instance.ReturnWorldMap();
        validSpawnPoints = TerrainManager.Instance.ValidSpawns();
        playerWorldPosition = TerrainManager.Instance.SetPlayerPosition(validSpawnPoints);
        playerMapPosition = new Vector2Int(worldMap.GetXY(playerWorldPosition).x, worldMap.GetXY(playerWorldPosition).y);
        initialMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
        initialWorldChunkPosition = worldMap.GetWorldPosition(initialMapChunkPosition.x, initialMapChunkPosition.y);

        GenerateChunks(initialMapChunkPosition);
        StartCoroutine(SpawnPlayerDelay(4f));
    }
    private Rect GetChunkLoadBounds(Vector2Int initialChunkPosition)
    {
        int startX = initialChunkPosition.x - 2 * TerrainManager.Instance.chunkWidth;
        int startY = 0;
        int endX = initialChunkPosition.x + TerrainManager.Instance.chunkWidth;
        int endY = TerrainManager.Instance.chunkHeight;

        return new Rect(startX, startY, endX - startX, endY - startY);
    }
    private void GenerateChunks(Vector2Int initialMapChunkPosition)
    {
        bool isWithinBounds = true; 
        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);
        
        for (int x = (int)bounds.xMin; x < bounds.xMax + TerrainManager.Instance.chunkWidth; x += 32)
        {
            Vector2Int chunkMapPosition = new Vector2Int(x, 0);
            Vector3 chunkWorldPosition = worldMap.GetWorldPosition(chunkMapPosition.x, chunkMapPosition.y);
            Chunk chunk = Instantiate(chunkPrefab, chunkWorldPosition, Quaternion.identity).GetComponent<Chunk>();
            chunksToLoad.Add(chunk);
        }
    }
    private void DeleteChunks(Vector2Int initialChunkPosition)
    {
        Rect bounds = GetChunkLoadBounds(initialChunkPosition);
        List<int> indexes = new List<int>();

        foreach (Chunk chunk in chunksToLoad)
        {
            if (!bounds.Contains(new Vector2(worldMap.GetXY(chunk.transform.position).x, 0)))
            {
                chunksToUnload.Add(chunk);
                indexes.Add(chunksToLoad.FindIndex(c => chunk));
            }
        }
        for (int i = 0; i < indexes.Count; i++)
        {
            chunksToLoad.RemoveAt(indexes[i]);
        }
        foreach (Chunk chunk in chunksToUnload)
        {
            chunk.UnloadChunk();
        }
        chunksToUnload.Clear();
    }
    private IEnumerator SpawnPlayerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Player.Instance.transform.position = playerWorldPosition;
    }
    #endregion
}
