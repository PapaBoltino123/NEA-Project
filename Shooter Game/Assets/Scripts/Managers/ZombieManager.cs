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
    public Pathfinder pathfinder;
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
    }

    void SpawnZombie()
    {
        GameObject zombie = Instantiate(zombiePrefab, new Vector3(Player.Instance.transform.position.x - 3.2f, 20, 0), Quaternion.identity);
    }
}
