using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Algorithms;
using System.Runtime.InteropServices;
using UnityEngine.Tilemaps;

namespace System.Algorithms.TerrainGeneration
{
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
    public class Chunk
    {
        #region Variable Declaration
        private Grid<Node> grid, chunkGrid;
        private int width, height;
        private int minX, maxX;
        private int index;
        public List<(TileBase tile, int x, int y)> Tiles { get; private set; }
        #endregion
        #region Constructor
        public Chunk(int width, int height, Grid<Node> grid, int minX, int maxX)
        {
            this.width = width;
            this.height = height;
            this.grid = grid;
            this.minX = minX;
            this.maxX = maxX;

            if (grid.Width % 64 != 0)
                throw new Exception("Grid has not been initialised properly");

            this.chunkGrid = new Grid<Node>(width, height, 0.16f, (Grid<Node> g, int x, int y) => new Node(g, x, y));

        }
        #endregion
        #region Properties
        public int Width
        {
            get { return this.width; }
        }
        public int Height
        {
            get { return this.height; }
        }
        public (int MinX, int MaxX) ChunkRange
        {
            get { return (minX,  maxX); }
        }
        public int Index
        {
            get { return index; }
            set { index = value; }
        }
        #endregion
        #region Methods
        public void AddTiles(TileBase tile, int x, int y)
        {
            Tiles.Add((tile, x , y));
        }
        #endregion
    }
}