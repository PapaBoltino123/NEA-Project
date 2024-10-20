using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.TerrainGeneration;
using Unity.VisualScripting;

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

    private const int LEFTRANGE = -2;
    private const int RIGHTRANGE = 1;
    public int poolSize = 30;    
    public float chunkLifetime = 10f;   
    private CustomQueue<GameObject> chunkPool;   
    private Dictionary<Vector2Int, GameObject> activeChunks;
    private Dictionary<GameObject, Coroutine> chunkLifetimes; 
    bool canLoadInChunksOnUpdate;
    #endregion
    #region Methods
    private void Start()
    {
        canLoadInChunksOnUpdate = false;
        chunkPool = new CustomQueue<GameObject>(poolSize);
        activeChunks = new Dictionary<Vector2Int, GameObject>();
        chunkLifetimes = new Dictionary<GameObject, Coroutine>();
        worldMap = TerrainManager.Instance.ReturnWorldMap();

        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        validSpawnPoints = TerrainManager.Instance.ValidSpawns();
        playerWorldPosition = TerrainManager.Instance.SetPlayerPosition(validSpawnPoints);
        playerMapPosition = new Vector2Int(worldMap.GetXY(playerWorldPosition).x, worldMap.GetXY(playerWorldPosition).y);
        initialMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
        initialWorldChunkPosition = worldMap.GetWorldPosition(initialMapChunkPosition.x, initialMapChunkPosition.y);

        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);

        for (int x = (int)bounds.xMin; x < bounds.xMax + TerrainManager.Instance.chunkWidth; x += TerrainManager.Instance.chunkWidth)
        {
            Vector2Int chunkMapPosition = new Vector2Int(x, 0);
            Vector3 chunkWorldPosition = worldMap.GetWorldPosition(chunkMapPosition.x, chunkMapPosition.y);
            GameObject chunkGameObject = Instantiate(chunkPrefab, chunkWorldPosition, Quaternion.identity);
            chunkGameObject.SetActive(false);
            chunkPool.Enqueue(chunkGameObject);
            GenerateChunk(chunkMapPosition);
        }

        StartCoroutine(SpawnPlayerDelay(4f));
    }
    private void Update()
    {
        if (canLoadInChunksOnUpdate == true)
        {
            playerWorldPosition = Player.Instance.transform.position;
            playerMapPosition = new Vector2Int(worldMap.GetXY(playerWorldPosition).x, worldMap.GetXY(playerWorldPosition).y);
            initialMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
            initialWorldChunkPosition = worldMap.GetWorldPosition(initialMapChunkPosition.x, initialMapChunkPosition.y);
            UpdateChunks(initialMapChunkPosition);
        }
    }
    private Rect GetChunkLoadBounds(Vector2Int initialChunkPosition)
    {
        int startX = initialChunkPosition.x - 2 * TerrainManager.Instance.chunkWidth;
        int startY = 0;
        int endX = initialChunkPosition.x + TerrainManager.Instance.chunkWidth;
        int endY = TerrainManager.Instance.chunkHeight;

        return new Rect(startX, startY, endX - startX, endY - startY);
    }
    private void GenerateChunk(Vector2Int chunkMapPosition)
    {
        Vector3 chunkWorldPosition = worldMap.GetWorldPosition(chunkMapPosition.x, chunkMapPosition.y);
        GameObject chunk;

        if (activeChunks.ContainsKey(chunkMapPosition))
        {
            return;
        }
        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
            chunk.SetActive(false);
        }
        else
        {
            chunk = Instantiate(chunkPrefab, chunkWorldPosition, Quaternion.identity);
            chunk.SetActive(false);
        }
        if (chunkLifetimes.ContainsKey(chunk))
        {
            StopCoroutine(chunkLifetimes[chunk]);
            chunkLifetimes.Remove(chunk);
        }

        chunk.SetActive(true);
        activeChunks[chunkMapPosition] = chunk;
    }
    private void UnloadChunk(Vector2Int initialMapChunkPosition)
    {
        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);

        if (!activeChunks.ContainsKey(initialMapChunkPosition))
        {
            return;
        }

        GameObject chunk = activeChunks[initialMapChunkPosition];
        chunk.SetActive(false);
        chunkPool.Enqueue(chunk);
        Coroutine destructionCoroutine = StartCoroutine(DestroyChunkAfterTime(chunk, chunkLifetime));
        chunkLifetimes[chunk] = destructionCoroutine;
        activeChunks.Remove(initialMapChunkPosition);
    }
    public void UpdateChunks(Vector2Int initialChunkMapPosition)
    {
        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);

        for (int x = (int)bounds.xMin; x < bounds.xMax + TerrainManager.Instance.chunkWidth; x += TerrainManager.Instance.chunkWidth)
        {
            Vector2Int chunkMapPosition = new Vector2Int(x, 0);
            if (!activeChunks.ContainsKey(chunkMapPosition))
            {
                chunksToLoad.Add(chunkMapPosition);
            }
        }

        foreach (var chunkMapPosition in activeChunks.Keys)
        {
            if (!bounds.Contains(chunkMapPosition))
            {
                chunksToUnload.Add(chunkMapPosition);
            }
        }

        foreach (var chunkMapPosition in chunksToLoad)
        {
            GenerateChunk(chunkMapPosition);
        }

        foreach (var chunkMapPosition in chunksToUnload)
        {
            UnloadChunk(chunkMapPosition);
        }
    }
    private IEnumerator SpawnPlayerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Player.Instance.transform.position = playerWorldPosition;
        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        canLoadInChunksOnUpdate = true;
    }
    private IEnumerator DestroyChunkAfterTime(GameObject chunk, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!chunk.activeSelf && chunkPool.Contains(chunk))
        {
            List<GameObject> chunkList = chunkPool.ToList();
            chunkList.Remove(chunk);
            chunkPool = new CustomQueue<GameObject>(chunkList);
            Destroy(chunk);
        }
    }
    #endregion
}
