using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.AdditionalDataStructures;

public class TestingChunks : Singleton<TestingChunks>
{
    public int chunkHeight = 128;
    public int chunkWidth = 32;
    private int worldWidth = 64;
    private int worldHeight = 128;
    private float cellSize = 0.16f;
    private Grid<Node> map;
    [SerializeField] private GameObject chunkPrefab;
    private List<TempChunk> loadedChunks;

    public Grid<Node> WorldMap
    {
        get;
        private set;
    }

    private void Awake()
    {
        map = new Grid<Node>(worldWidth, worldHeight, cellSize, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        loadedChunks = new List<TempChunk>();

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight / 2; y++)
            {
                Node node = map.GetGridObject(x, y);
                node.TileData = "DIRT";
            }
            for (int y = worldHeight / 2; y < worldHeight; y++)
            {
                Node node = map.GetGridObject(x, y);
                node.TileData = "SKY";
            }
        }
    }
    private void Start()
    {
        GenerateChunks();
    }
    private void GenerateChunks()
    {
        TempChunk chunk1 = Instantiate(chunkPrefab, Vector3.zero, Quaternion.identity).GetComponent<TempChunk>();
        loadedChunks.Add(chunk1);
        TempChunk chunk2 = Instantiate(chunkPrefab, new Vector3(chunkWidth * cellSize, 0, 0), Quaternion.identity).GetComponent<TempChunk>();
        loadedChunks.Add(chunk2);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var chunk in loadedChunks)
            {
                chunk.UnloadChunk();
            }
        }
    }
    public Grid<Node> ReturnWorldMap()
    {
        return map;
    }
}
