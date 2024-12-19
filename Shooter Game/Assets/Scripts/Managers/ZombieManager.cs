using System;
using System.AdditionalDataStructures;
using System.AddtionalEventStructures;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Tilemaps;

public class ZombieManager : Singleton<ZombieManager>
{
    public float speed;
    public float jumpForce = 5;
    public int healthMax;
    public float spawnRate;
    public int damagePoints;
    public byte[,] byteMap;
    public Grid<Node> nodeMap;

    [SerializeField] GameObject zombiePrefab;
    [SerializeField] GameObject[] itemPrefabs;

    public int zombieCount = 0;
    const int zombieCap = 10;

    private void Start()
    {
        GameManager.Instance.fileManager.dataBroadcast.SendLoadedData += new EventHandler<DataEventArgs>(LoadGame);
        GameManager.Instance.fileManager.dataBroadcast.SendNewData += new EventHandler<DataEventArgs>(NewGame);
        GameManager.Instance.fileManager.dataBroadcast.SaveData += new EventHandler<EventArgs>(SaveGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) //debug purposes
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
        Vector2Int gridSpawnPos = validSpawnPoints[UnityEngine.Random.Range(0, validSpawnPoints.Count)];
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

    private void LoadGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        speed = data.zombieSpeed;
        healthMax = data.zombieHealth;
        spawnRate = data.zombieSpawnRate;
        damagePoints = data.zombieDamagePoints;
    }
    private void SaveGame(object sender, EventArgs e)
    {
        GameManager.Instance.savedData.zombieHealth = healthMax;
        GameManager.Instance.savedData.zombieSpawnRate = spawnRate;
        GameManager.Instance.savedData.zombieSpeed = speed;
        GameManager.Instance.savedData.zombieDamagePoints = damagePoints;
    }
    private void NewGame(object sender, DataEventArgs e)
    {
        GameData data = e.gameData;
        int difficultyLevel = GameManager.Instance.CalculateDifficultyLevel();

        damagePoints = e.gameData.zombieDamagePoints;
        speed = e.gameData.zombieSpeed;
        spawnRate = e.gameData.zombieSpawnRate;
        healthMax = e.gameData.zombieHealth;
    }

    public void StartZombieSpawning()
    {
        StartCoroutine(ZombieSpawning());
    }
    public void StopZombieSpawning()
    {
        StopCoroutine(ZombieSpawning());
    }
    private IEnumerator ZombieSpawning()
    {
        while (true)
        {
            if (zombieCount < zombieCap)
            {
                SpawnZombie();
                zombieCount++;
                yield return new WaitForSeconds(spawnRate);
            }
            yield return null;
        }
    }
}
