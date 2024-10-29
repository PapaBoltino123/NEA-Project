using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;
using System.Algorithms.TerrainGeneration;
using Unity.VisualScripting;
using System.AddtionalEventStructures;

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
    private GameData loadedData;

    private const int LEFTRANGE = -2;
    private const int RIGHTRANGE = 1;
    public int poolSize = 20;    
    public float chunkLifetime = 10f;   
    public CustomQueue<GameObject> chunkPool;   
    public Dictionary<Vector2Int, GameObject> activeChunks;

    bool canLoadInChunksOnUpdate;
    #endregion
    #region Methods
    private void Awake()
    {
        GameManager.Instance.fileManager.dataBroadcast.SendLoadedData += new EventHandler<DataEventArgs>(LoadGame);
        GameManager.Instance.fileManager.dataBroadcast.SendNewData += new EventHandler<DataEventArgs>(NewGame);
        GameManager.Instance.fileManager.dataBroadcast.SaveData += new EventHandler<EventArgs>(SaveGame);
    }
    private void Update()
    {
        if (canLoadInChunksOnUpdate == true)
        {
            playerWorldPosition = Player.Instance.transform.position;
            playerMapPosition = new Vector2Int(worldMap.GetXY(playerWorldPosition).x, worldMap.GetXY(playerWorldPosition).y);
            Vector2Int currentMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
            initialWorldChunkPosition = worldMap.GetWorldPosition(currentMapChunkPosition.x, currentMapChunkPosition.y);

            if (currentMapChunkPosition != initialMapChunkPosition)
            {
                UpdateChunks(initialMapChunkPosition);
            }

            initialMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
        }
    }
    private void LoadGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        loadedData = data;
    }
    private void SaveGame(object sender, EventArgs e)
    {
        GameManager.Instance.savedData.playerX = playerWorldPosition.x;
        GameManager.Instance.savedData.playerY = playerWorldPosition.y;
    }
    private void NewGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        loadedData = data;
    }
    private Rect GetChunkLoadBounds(Vector2Int initialChunkPosition)
    {
        int startX = initialChunkPosition.x - 4 * TerrainManager.Instance.chunkWidth;
        int startY = 0;
        int endX = initialChunkPosition.x + 3 * TerrainManager.Instance.chunkWidth + 1;
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
            chunk = Instantiate(chunkPrefab);
            chunk.SetActive(false);
        }

        chunk.SetActive(true);
        chunk.transform.position = chunkWorldPosition;
        activeChunks[chunkMapPosition] = chunk;
        chunk.GetComponent<Chunk>().Load();
    }
    private void UnloadChunk(Vector2Int mapChunkPosition)
    {
        if (!activeChunks.ContainsKey(mapChunkPosition))
        {
            return;
        }

        GameObject chunk = activeChunks[mapChunkPosition];
        chunk.SetActive(false);
        chunkPool.Enqueue(chunk);
        activeChunks.Remove(mapChunkPosition);
    }
    public void UpdateChunks(Vector2Int initialChunkMapPosition)
    {
        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        List<Vector2Int> chunksToUnload = new List<Vector2Int>();
        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);

        for (int x = (int)bounds.xMin; x < bounds.xMax; x += TerrainManager.Instance.chunkWidth)
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
    public void SetInitialChunks()
    {
        canLoadInChunksOnUpdate = false;
        chunkPool = new CustomQueue<GameObject>();
        activeChunks = new Dictionary<Vector2Int, GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject chunkGameObject = Instantiate(chunkPrefab);
            chunkGameObject.SetActive(false);
            chunkPool.Enqueue(chunkGameObject);
        }

        worldMap = TerrainManager.Instance.ReturnWorldMap();

        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezePositionY;
        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        try
        {
            playerWorldPosition = GetInitialWorldPosition();
        }
        catch
        {
            if (loadedData == null)
            {
                Debug.Log(loadedData);
                throw new Exception("Shit");
            }
        }

        playerMapPosition = new Vector2Int(worldMap.GetXY(playerWorldPosition).x, worldMap.GetXY(playerWorldPosition).y);
        initialMapChunkPosition = new Vector2Int((playerMapPosition.x / TerrainManager.Instance.chunkWidth) * TerrainManager.Instance.chunkWidth, 0);
        initialWorldChunkPosition = worldMap.GetWorldPosition(initialMapChunkPosition.x, initialMapChunkPosition.y);

        List<Vector2Int> chunksToLoad = new List<Vector2Int>();
        Rect bounds = GetChunkLoadBounds(initialMapChunkPosition);

        for (int x = (int)bounds.xMin; x < bounds.xMax; x += TerrainManager.Instance.chunkWidth)
        {
            Vector2Int chunkMapPosition = new Vector2Int(x, 0);
            chunksToLoad.Add(chunkMapPosition);
        }

        foreach (var chunkMapPosition in chunksToLoad)
        {
            GenerateChunk(chunkMapPosition);
        }

        StartCoroutine(SpawnPlayerDelay(5f));
    }
    private Vector3 GetInitialWorldPosition()
    {
        Vector3 playerWorldPosition = new Vector3(loadedData.playerX, loadedData.playerY, 0);

        if (playerWorldPosition.x < 0)
        {
            validSpawnPoints = TerrainManager.Instance.ValidSpawns();
            playerWorldPosition = TerrainManager.Instance.SetPlayerPosition(validSpawnPoints);
        }

        return playerWorldPosition;
    }
    private IEnumerator SpawnPlayerDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Player.Instance.transform.position = playerWorldPosition;
        Player.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        canLoadInChunksOnUpdate = true;
    }
    #endregion
}
