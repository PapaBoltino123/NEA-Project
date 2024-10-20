using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Algorithms.Pathfinding;
using System.AdditionalDataStructures;

namespace System.Algorithms.Pathfinding
{
    public enum HeuristicFormula
    {
        #region Variable Declaration 
        Manhattan = 1,
        MaxDXDY = 2,
        DiagonalShortCut = 3,
        Euclidean = 4,
        EuclideanNoSQR = 5,
        Custom1 = 6
        #endregion
    }
    public struct Location
    {
        #region Variable Declaration
        public int xy;
        #endregion
        #region Constructor
        public Location(int xy)
        {
            this.xy = xy;
        }
        #endregion
    }
    public struct PathRequest
    {
        #region Variable Declaration
        private Node startNode;
        private Node targetNode;
        #endregion
        #region Constructors
        public PathRequest(Vector2 start, Vector2 target, Grid<Node> map)
        {
            this.startNode = map.GetGridObject((int)start.x, (int)start.y);
            this.targetNode = map.GetGridObject((int)target.x, (int)target.y);
        }
        #endregion
        #region Properties
        public Node StartNode
        {
            get { return this.startNode; }
        }
        public Node TargetNode
        {
            get { return this.targetNode; }
        }
        #endregion
    }
}
