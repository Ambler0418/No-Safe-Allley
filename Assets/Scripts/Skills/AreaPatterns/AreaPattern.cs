using UnityEngine;
using System.Collections.Generic;

public abstract class AreaPattern : ScriptableObject
   {       
        public abstract List<Vector3Int> GetAffectedTiles(Vector3Int primaryTarget);
        public virtual bool IsGlobal => false; // 전역 스킬 여부 (타겟팅 불필요)
   }