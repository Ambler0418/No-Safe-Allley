using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Single Tile Area", menuName = "Skills/Area Patterns/Single Tile")]
    public class SingleTileArea : AreaPattern
    {
    public override List<Vector3Int> GetAffectedTiles(Vector3Int primaryTarget)
    {
            return new List<Vector3Int> { primaryTarget };
    }
    }