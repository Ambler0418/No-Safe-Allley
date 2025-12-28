using UnityEngine;
using System.Collections.Generic;

public abstract class AreaPattern : ScriptableObject
   {       
        public abstract List<Vector3Int> GetAffectedTiles(Vector3Int primaryTarget);
   }