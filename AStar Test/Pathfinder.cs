using System.Algorithms;
using System.Algorithms.Pathfinding;

int width = 131072; int height = 128; float cellSize = 0.16f;
Grid<Node> grid = new Grid<Node>(width, height, cellSize, (Grid<Node> g, int x, int y) => new Node(x, y, g));


