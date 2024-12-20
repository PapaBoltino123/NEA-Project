using System.Collections;
using System.Collections.Generic;
using System.ItemStructures;
using System.Linq;
using UnityEngine;

namespace System.AdditionalDataStructures
{
    public class Node
    {
        #region Variables Declaration
        private Grid<Node> grid; //pathfinding variables
        public int x, y;
        public Node parentNode;

        private string tileData; //terrain generation variables
        private int rockTileType;
        private int treeTileType;
        #endregion
        #region Constructor
        public Node(Grid<Node> grid, int x, int y)
        {
            this.grid = grid; //sets initial node values
            this.x = x;
            this.y = y;
            this.tileData = "";
            this.rockTileType = 100;
            this.treeTileType = 100;
        }
        #endregion 
        #region Properties
        public string TileData
        {
            get { return this.tileData; } 
            set
            {
                this.tileData = value; //when tiledata changes, update map
                UpdateNode();
            }
        }
        public int RockTileType
        {
            get { return this.rockTileType; }  //when tiledata changes, update map
            set
            {
                this.rockTileType = value;
                UpdateNode();
            }
        }
        public int TreeTileType
        {
            get { return this.treeTileType; }  //when tiledata changes, update map
            set
            {
                this.treeTileType = value;
                UpdateNode();
            }
        }
        #endregion
        #region Methods
        public void UpdateNode()
        {
            this.grid.SetGridObject(this.x, this.y, this); //updates grid node if any changes are made to the node 
        }
        public override string ToString()
        {
            return $"{x}, {y}"; //output for debugging
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
    public class CustomQueue<T>
    {
        #region Variables Declaration
        private List<T> elements = new List<T>();
        #endregion
        #region Constructors
        public CustomQueue() { }
        public CustomQueue(List<T> list)
        {
            elements = list;
        }
        #endregion
        #region Properties
        public int Count
        {
            get { return elements.Count; }
        }
        #endregion
        #region Methods
        public void Enqueue(T item)
        {
            elements.Add(item);
        }
        public T Dequeue()
        {
            if (IsEmpty() == true)
                throw new Exception("The queue is empty.");

            var item = elements[0];
            elements.RemoveAt(0);
            return item;
        }
        public T Peek()
        {
            if (IsEmpty() == true)
                throw new Exception("The queue is empty.");

            return elements[0];
        }
        public bool IsEmpty()
        {
            if (elements.Count == 0)
                return true;
            else
                return false;
        }
        public List<T> ToList()
        {
            return elements;
        }
        public void Clear()
        {
            elements.Clear();
        }
        public bool Contains(T item)
        {
            if (elements.Contains(item))
                return true;
            else
                return false;
        }
        #endregion
    }
    public class HashTable
    {
        private Item[] elements;
        public int maxSize { get; set; }

        public HashTable(int capacity)
        {
            maxSize = capacity;
            elements= new Item[capacity];
        }

        public void AddOrUpdate(Item item)
        {
            string hashKey = item.name;
            int hashVal = GetHashValue(hashKey);

            if (elements[hashVal] != null && elements[hashVal].name == item.name)
                Update(item);
            else
                elements[hashVal] = item;
        }
        private void Update(Item item)
        {
            int hashVal = GetHashValue(item.name);

            if (elements[hashVal] != null && elements[hashVal].name == item.name)
            {
                if (elements[hashVal].type == typeof(Ammo) ||
                                        elements[hashVal].type == typeof(HealthPack))
                {
                    (elements[hashVal]).count += (item).count;
                }
                else
                    elements[hashVal] = item;
            }
        }
        public int? Contains(Item item)
        {
            string hashKey = item.name;
            int hashVal = GetHashValue(hashKey);

            if (elements[hashVal] == null)
                return null;
            else
                return hashVal;
        }
        private int GetHashValue(string hashKey)
        {
            int hashVal = 0;

            for (int i = 0; i < hashKey.Length; i++)
            {
                char c = hashKey[i];
                hashVal += (c * i);
            }

            return hashVal % maxSize;
        }
        public List<Item> ToList()
        {
            return elements.ToList();
        }
        public int Count()
        {
            return elements.Length;
        }
    }
    public class CustomDictionary<K, V>
    {
        private List<K> keys = new List<K>();
        private List<V> values = new List<V>();

        public void Add(K key, V value)
        {
            if (keys.Contains(key))
                throw new System.Exception("Key already exists in the dictionary.");

            keys.Add(key);
            values.Add(value);
        }

        public bool Remove(K key)
        {
            int index = keys.IndexOf(key);
            if (index == -1)
                return false;

            keys.RemoveAt(index);
            values.RemoveAt(index);
            return true;
        }

        public bool ContainsKey(K key)
        {
            return keys.Contains(key);
        }

        public V this[K key]
        {
            get
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                    throw new System.Exception("Key not found in the dictionary.");

                return values[index];
            }
            set
            {
                int index = keys.IndexOf(key);
                if (index == -1)
                {
                    keys.Add(key);
                    values.Add(value);
                }
                else
                {
                    values[index] = value;
                }
            }
        }
        public int Count
        {
            get { return keys.Count; }
        }

        public IEnumerable<K> Keys
        {
            get { return keys; }
        }

        public IEnumerable<V> Values
        {
            get { return values; }
        }

        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }
    }
    public enum SceneType
    {
        MANAGER = 0, //the scene is either the persisten scene which holds all the managers and is permanently active, the menu scene or the main game scene
        TITLESCREEN = 1,
        MAINGAME = 2
    }
    public enum PlayerState
    {
        ALIVE,
        DEAD
    }
    public enum HotBarType
    {
        RANGED = 0,
        MELEE = 1,
        HEALTH = 2
    }
    public enum AmmoTypes
    {
        PISTOL = 0,
        RIFLE = 1,
        SMG = 2,
        ROCKET = 3
    }
    public enum MeleeWeaponType
    {
        AXE = 0,
        SWORD = 1,
        SPEAR = 2
    }
}
