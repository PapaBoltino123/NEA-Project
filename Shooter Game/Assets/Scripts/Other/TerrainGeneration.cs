using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Algorithms;
using System.Runtime.InteropServices;
using UnityEngine.Tilemaps;

namespace System.Algorithms.TerrainGeneration
{
    public class Chunk : MonoBehaviour
    {
        #region Variable Declaration
        [SerializeField] public Tilemap ground, decorations, groundCollisions, waterCollisions;
        [SerializeField] private TerrainManager terrainManager;
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
            int chunkWidth = terrainManager.chunkWidth;
            int chunkHeight = terrainManager.chunkHeight;
            Position = new Vector3Int((int)transform.position.x, (int)transform.position.y, 0);
            ChunkPosition = new Vector3Int(Position.x / chunkWidth, Position.y / chunkHeight, 0);

            chunkCollider = GetComponent<BoxCollider2D>();
            chunkCollider.size = new Vector2(chunkWidth, chunkHeight);
            chunkCollider.offset = new Vector2(chunkWidth / 2, chunkHeight / 2);
            LoadChunk();
        }
        private void LoadChunk()
        {
            StartCoroutine(terrainManager.GenerateChunk(this));
        }
        public void UnloadChunk()
        {
            Destroy(gameObject);
            isUnloading = true;
        }
        public void SetChunkTile(Vector3Int position, TileMapType mapType, TileBase tile)
        {
            if (isUnloading)
                return;

            Tilemap targetMap = GetMap(mapType);
            if (targetMap == null)
                return;

            Vector3Int relativePosition = position - Position;
            targetMap.SetTile(relativePosition, tile);
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
                    return waterCollisions;
                default:
                    return null;
            }
        }
        #endregion
    }
    public class PerlinNoise
    {
        #region Variable Declaration
        private readonly double[] gradients;
        private readonly System.Random random;
        #endregion
        #region Constructor
        public PerlinNoise(int seed)
        {
            random = new System.Random(seed);
            gradients = new double[256];
            for (int i = 0; i < 256; i++)
                gradients[i] = (random.NextDouble() * 2) - 1;
        }
        #endregion
        #region Methods
        private double Fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        private double Interpolate(double a, double b, double t)
        {
            return a + t * (b - a);
        }

        public double GenerateNoise(double x)
        {
            int x0 = (int)x;
            int x1 = x0 + 1;
            double t = x - x0;

            double gradient0 = gradients[x0 & 255];
            double gradient1 = gradients[x1 & 255];

            double dot0 = gradient0 * t;
            double dot1 = gradient1 * (t - 1);

            double fadeT = Fade(t); fadeT = Fade(fadeT);
            double result = Interpolate(dot0, dot1, fadeT);
            return System.Math.Abs(result);
        }
        #endregion
    }
    public enum TileType
    {
        #region Variable Declaration
        DEFAULT,
        SKY,
        STONE,
        DIRT,
        GRASS,
        WATER,
        LILYPAD,
        FLOWER,
        TREE,
        ROCK
        #endregion
    }
    public enum TileMapType
    {
        #region Variable Declaration
        DEFAULT,
        GROUND,
        DECORATIONS,
        GROUND_COLLISIONS,
        OTHER_COLLISIONS
        #endregion
    }
}