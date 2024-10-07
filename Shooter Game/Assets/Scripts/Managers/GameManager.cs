using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Algorithms;

public class GameManager : Manager
{
    [SerializeField] private int width, height;
    [SerializeField] public int seed, smoothness;
    [SerializeField] TerrainManager terrainManager;
    public List<Node> validSpawnPoints;
    public Grid<Node> grid;

    private void Start()
    {
        RunManager();
    }
    public override void RunManager()
    {
        grid = new Grid<Node>(width, height, 0.16f, (Grid<Node> g, int x, int y) => new Node(g, x, y));
        terrainManager.RunManager();
    }
}
