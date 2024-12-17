using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZombieManager : Singleton<ZombieManager>
{
    public float speed = 3;
    public float jumpForce = 5;
    private float gravity;
    public byte[,] byteMap;
    public Grid<Node> nodeMap;
    [SerializeField] GameObject zombiePrefab;
    [SerializeField] GameObject[] itemPrefabs;
    public Tilemap testMap;
    public TileBase test;

    private void Awake()
    {
        gravity = System.Math.Abs(Physics2D.gravity.y);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            SpawnZombie();

        if (Input.GetKeyDown(KeyCode.I))
            SpawnItem(new Vector3(Player.Instance.transform.position.x + 0.48f, Player.Instance.transform.position.y));
    }

    void SpawnZombie()
    {
        Vector3 spawnPos = Vector3.zero;
        List<Vector2Int> allSpawnPoints = ChunkManager.Instance.validSpawnPoints.Select(p => new Vector2Int(p.x, p.y)).ToList();
        Rect bounds = ChunkManager.Instance.loadedArea;

        bounds.xMin += 32;
        bounds.xMax -= 32;
        List<Vector2Int> validSpawnPoints = allSpawnPoints
            .Where(point => bounds.Contains(point))
            .ToList();
        Vector2Int gridSpawnPos = validSpawnPoints[Random.Range(0, validSpawnPoints.Count)];
        spawnPos = new Vector3(gridSpawnPos.x * 0.16f + 0.08f, gridSpawnPos.y * 0.16f + 0.32f);

        GameObject zombie = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
    }

    public void SpawnItem(Vector3 spawnPosition)
    {
        System.Random random = new System.Random();
        int index = random.Next(0, itemPrefabs.Length);
        GameObject item = Instantiate(itemPrefabs[index], spawnPosition, Quaternion.identity);
        GameManager.Instance.activePrefabs.Add(item);
    }
}
