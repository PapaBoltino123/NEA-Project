namespace System.Algorithms
{
    class Grid<T>
    {
        private int width;
        private int height;
        private float cellSize;
        private T[,] gridArray;

        public Grid(int width, int height, float cellSize, Func< Grid<T>, int, int, T> createGridObject)
        {
            this.width = width;
            this.height = height;
            this.cellSize = cellSize;
            gridArray = new T[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < gridArray.GetLength(1); y++)
                {
                    gridArray[x, y] = createGridObject(this, x, y);
                }
            }
        }
        public int GetWidth() { return width; }
        public int GetHeight() { return height; }
        public float GetCellSize() { return cellSize; }
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x * cellSize, y * cellSize);
        }
        public (int, int) GetXY(Vector3 worldPosition)
        {
            int x = (int)Math.Floor(worldPosition.x / cellSize);
            int y = (int)Math.Floor(worldPosition.y / cellSize);
            return (x, y);
        }
        public T GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < width && y < height)
                return gridArray[x, y];
            else
                return default(T);
        }
        public T GetGridObject(Vector3 worldPosition)
        {
            (int, int) coordinates;
            coordinates = GetXY(worldPosition);
            return GetGridObject(coordinates.Item1, coordinates.Item2);
        }
        public void SetGridObject(int x, int y, T gridObject)
        {
            gridArray[x, y] = gridObject;
        }
    }
    struct Vector3
    {
        public float x, y;
        public Vector3(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }
    struct Node
    {
        private Grid<Node> grid;
        public int fCost, gCost;
        public int x, y;
        public (int parentX, int parentY) parentCoordinates;
        public int status;
        public int jumpLength;
        private string binary;
        public string binaryValue
        {
            get { return binary; }
            set
            {
                binaryValue = value;
                UpdateNode();
            }
        }

        public Node(int x, int y, Grid<Node> grid)
        {
            this.x = x; this.y = y;
            this.grid = grid;
        }
        public Node UpdateStatus(int newStatus)
        {
            Node newNode = this;
            newNode.status = newStatus;
            return newNode;
        }
        private void UpdateNode()
        {
            this.grid.SetGridObject(this.x, this.y, this);
        }
    }
}
namespace System.Algorithms.TerrainGeneration
{
    class PerlinNoise
    {
        private readonly double[] gradients;
        private readonly System.Random random;

        public PerlinNoise(int seed)
        {
            random = new System.Random(seed);
            gradients = new double[256];
            for (int i = 0; i < 256; i++)
                gradients[i] = (random.NextDouble() * 2) - 1;
        }

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
    }
}
namespace System.Algorithms.Pathfinding
{
    class CustomStack<T>
    {
        private List<T> elements = new List<T>();

        public void Push(T item)
        {
            elements.Add(item);
        }
        public T Pop()
        {
            if (IsEmpty() == true)
            {
                throw new Exception("The stack is empty");
            }

            T item = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            return item;
        }
        public T Peek()
        {
            if (IsEmpty() == true)
            {
                throw new Exception("The stack is empty");
            }

            return elements[elements.Count - 1];
        }
        private bool IsEmpty()
        {
            if (elements.Count == 0)
                return true;
            else
                return false;
        }
        public int Count()
        {
            return elements.Count;
        }
    }
    class CustomPriorityQueue<T>
    {
        private List<(T item, int priority)> elements = new List<(T item, int priority)>();

        public void Enqueue(T item, int priority)
        {
            elements.Add((item, priority));
            elements.Sort((x, y) => x.priority.CompareTo(y.priority));
        }
        public T Dequeue()
        {
            if (IsEmpty() == true)
                throw new Exception("The priority queue is empty.");

            var item = elements[0];
            elements.RemoveAt(0);
            return item.item;
        }
        public T Peek()
        {
            if (IsEmpty() == true)
                throw new Exception("The priority queue is empty.");

            return elements[0].item;
        }
        public bool IsEmpty()
        {
            return elements.Count == 0;
        }
        public int Count()
        {
            return elements.Count;
        }
    }
    public enum HeuristicFormula
    {
        Manhattan = 1,
        MaxDXDY = 2,
        DiagonalShortCut = 3,
        Euclidean = 4,
        EuclideanNoSQR = 5,
        Custom1 = 6
    }
    public struct Location
    {
        public Location(int xy)
        {
            this.xy = xy;
        }

        public int xy;
    }
}
