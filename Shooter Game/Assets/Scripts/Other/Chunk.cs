using System.Algorithms.TerrainGeneration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    #region Variable Declaration
    [SerializeField] public Tilemap ground, decorations, groundCollisions, otherCollisions;
    private BoxCollider2D chunkCollider;
    private bool isUnloading = false;
    #endregion
    #region Properties
    public Vector3Int Position
    {
        get;
        private set;
    }
    public Vector3Int ChunkPosition
    {
        get;
        private set;
    }
    #endregion
    #region Methods
    private void Start()
    {
        int chunkWidth = TerrainManager.Instance.chunkWidth;
        int chunkHeight = TerrainManager.Instance.chunkHeight;
        Position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
        ChunkPosition = new Vector3Int(Position.x / chunkWidth, Position.y / chunkHeight, 0);

        chunkCollider = GetComponent<BoxCollider2D>();
        chunkCollider.size = new Vector2(chunkWidth, chunkHeight);
        chunkCollider.offset = new Vector2(chunkWidth / 2, chunkHeight / 2);
        LoadChunk();
    }
    private void LoadChunk()
    {
        StartCoroutine(TerrainManager.Instance.GenerateChunk(this));
    }
    public void UnloadChunk()
    {
        Destroy(gameObject);
        isUnloading = true;
    }
    public void SetChunkTile(Vector3Int position, TileMapType mapType, TileBase tile)
    {
        Debug.Log(this.transform.position);
        if (isUnloading)
            return;

        Tilemap targetMap = GetMap(mapType);
        if (targetMap == null)
            return;

        Vector3Int relativePosition = position - Position;

        
        for (int x = 0; x < TerrainManager.Instance.chunkWidth; x++)
        {
            for (int y = 0; y < TerrainManager.Instance.chunkHeight; y++)
            {
                targetMap.SetTile(new Vector3Int(relativePosition.x + x, relativePosition.y + y, 0), tile);
            }
        }
    }
    public TileBase GetChunkTile(Vector3Int position, TileMapType mapType)
    {
        if (isUnloading)
            return null;

        Tilemap targetMap = GetMap(mapType);
        if (targetMap == null)
            return null;

        Vector3Int relativePosition = position - Position;
        return targetMap.GetTile(relativePosition);
    }
    private Tilemap GetMap(TileMapType mapType)
    {
        if (isUnloading)
            return null;

        switch (mapType)
        {
            case TileMapType.GROUND:
                return ground;
            case TileMapType.DECORATIONS:
                return decorations;
            case TileMapType.GROUND_COLLISIONS:
                return groundCollisions;
            case TileMapType.OTHER_COLLISIONS:
                return otherCollisions;
            default:
                return null;
        }
    }
    #endregion
}
