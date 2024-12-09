using System.AdditionalDataStructures;
using System.Collections;
using System.Collections.Generic;
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
        GameObject zombie = Instantiate(zombiePrefab, new Vector3(Player.Instance.transform.position.x - 3.2f, 20, 0), Quaternion.identity);
    }

    public void SpawnItem(Vector3 spawnPosition)
    {
        System.Random random = new System.Random();
        int index = random.Next(0, itemPrefabs.Length);
        GameObject item = Instantiate(itemPrefabs[index], spawnPosition, Quaternion.identity);
    }
}
