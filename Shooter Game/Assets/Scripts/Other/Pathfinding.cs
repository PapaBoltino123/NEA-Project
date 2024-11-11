using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Algorithms.Pathfinding;
using System.AdditionalDataStructures;

namespace System.Algorithms.Pathfinding
{
    public struct Location //a struct used to hold the indexes of the nodes in the nodes array
    {
        #region Variable Declaration
        public int xy;
        public int z;
        #endregion
        #region Constructor
        public Location(int xy, int z)
        {
            this.xy = xy;
            this.z = z;
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
