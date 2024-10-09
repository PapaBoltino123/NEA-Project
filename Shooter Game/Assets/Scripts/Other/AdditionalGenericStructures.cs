using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace System.AdditionalDataStructures
{
    public struct Node
    {
        #region Variables Declaration
        private Grid<Node> grid;
        public int fCost, gCost;
        public int x, y;
        public (int parentX, int parentY) parentCoordinates;
        public int status;
        public int jumpLength;
        private string tileData;
        private int rockTileType;
        private int treeTileType;
        #endregion
        #region Constructor
        public Node(Grid<Node> grid, int x, int y)
        {
            this.grid = grid;
            this.x = x;
            this.y = y;
            this.fCost = 0;
            this.gCost = 0;
            this.parentCoordinates = (0, 0);
            this.status = 0;
            this.jumpLength = 0;
            this.tileData = "";
            this.rockTileType = 100;
            this.treeTileType = 100;
            this.IsRock = false;
            this.IsTree = false;
        }
        #endregion 
        #region Properties
        public string TileData
        {
            get { return this.tileData; }
            set
            {
                this.tileData = value;
                UpdateNode();
            }
        }
        public int RockTileType
        {
            get { return this.rockTileType; }
            set
            {
                this.rockTileType = value;
                UpdateNode();
            }
        }
        public int TreeTileType
        {
            get { return this.treeTileType; }
            set
            {
                this.treeTileType = value;
                UpdateNode();
            }
        }
        public bool IsRock { get; private set; }
        public bool IsTree { get; private set; }

        #endregion
        #region Methods
        public Node UpdateStatus(int newStatus)
        {
            Node newNode = this;
            newNode.status = newStatus;
            return newNode;
        }
        private void UpdateNode()
        {
            this.grid.SetGridObject(this.x, this.y, this);
            if (this.tileData == "0111")
            {
                this.IsTree = true;
                this.IsRock = false;
            }
            else if (this.tileData == "1000")
            {
                this.IsRock = true;
                this.IsTree = false;
            }
            else
            {
                this.IsRock = false;
                this.IsTree = false;
            }
        }
        #endregion
    }
    public class Grid<T>
    {
        #region Variables Declaration
        private int width;
        private int height;
        private float cellSize;
        private T[,] gridArray;
        #endregion
        #region Constructor
        public Grid(int width, int height, float cellSize, Func<Grid<T>, int, int, T> createGridObject)
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
        public float CellSize
        {
            get { return this.cellSize; }
        }
        #endregion
        #region Methods
        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x * cellSize, y * cellSize);
        }
        public (int x, int y) GetXY(Vector3 worldPosition)
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
        #endregion
    }
    public class CustomPriorityQueue<T>
    {
        #region Variable Declaration
        private List<(T item, int priority)> elements = new List<(T item, int priority)>();
        private int maxSize;
        #endregion
        #region Constructor
        public CustomPriorityQueue(int capacity)
        {
            maxSize = capacity;
        }
        #endregion
        #region Properties
        public int Count
        {
            get { return elements.Count; }
        }
        #endregion
        #region Methods
        public void Enqueue(T item, int priority)
        {
            if (elements.Count == maxSize)
                throw new Exception("The priority queue is full.");
            else
            {
                elements.Add((item, priority));
                elements.Sort((x, y) => x.priority.CompareTo(y.priority));
            }
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
            if (elements.Count == 0)
                return true;
            else
                return false;
        }
        public bool CheckUnique(T item, int priority)
        {
            if (elements.Contains((item, priority)))
            {
                return false;
            }
            else
                return true;
        }
        public List<T> ToList()
        {
            List<T> returnList = new List<T>();
            foreach (var item in elements)
                returnList.Add(item.item);

            return returnList;
        }
        public void Clear()
        {
            elements.Clear();
        }
        #endregion
    }
    public class CustomStack<T>
    {
        #region Variable Declaration
        private int maxSize;
        private List<T> elements = new List<T>();
        #endregion
        #region Constructor
        public CustomStack(int capacity)
        {
            maxSize = capacity;
        }
        #endregion
        #region Properties
        public int Count
        {
            get { return elements.Count; }
        }
        #endregion
        #region Methods
        public void Push(T item)
        {
            if (elements.Count == maxSize)
                throw new Exception("The stack is at maximum capacity");
            else
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
        #endregion
    }
    public class CustomHashTable
    {
        #region Variable Declaration
        private dynamic[] elements;
        private int capacity;
        #endregion
        #region Constructor
        public CustomHashTable(int capacity)
        {
            this.capacity = capacity;
            elements = new dynamic[capacity];
        }
        #endregion
        #region Properties
        public int Capacity
        {
            get { return this.capacity; }
            set { this.capacity = value; }
        }
        #endregion
        #region Methods
        public void Add(dynamic item)
        {
            string hashkey = item.hashkey;
            int hashVal = GetHashValue(hashkey);

            if (elements[hashVal] != null && elements[hashVal].hashkey == item.hashkey)
            {
                Update(item);
            }
            else
            {
                Debug.Log("Item has been added to Hash Table");
                elements[hashVal] = item;
            }
        }
        public void Update(dynamic item)
        {
            string hashkey = item.hashkey;
            int hashVal = GetHashValue(hashkey);

            if (elements[hashVal] != null && elements[hashVal].hashkey == item.hashkey)
            {
                Debug.Log("Hash Table has been updated");
                if (item.InventoryObjectType == WEAPON)
                    Debug.Log("Update weapon in some way");
                else if (item.InventoryObjectType == ITEM)
                {
                    elements[hashVal].Count += item.count;
                }
            }
        }
        public List<dynamic> ToList()
        {
            return elements;
        }
        private int GetHashValue(string hashkey)
        {
            if (hashkey != "")
            {
                int hashVal = 0;
                for (int i = 0; i < hashkey.Length; i++)
                {
                    char c = hashkey[i];
                    hashVal += (c * i);
                }
                hashVal = hashVal % capacity;
                return hashVal;
            }
            else
                throw new Exception("There is no readable hashkey within this item");
        }
        #endregion
    }
    public enum InventoryObjectType
    {
        #region Variables Declaration
        DEFAULT,
        WEAPON,
        ITEM
        #endregion
    }
}