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
}
