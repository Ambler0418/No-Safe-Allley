using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Global All Enemy Pattern", menuName = "Skills/Area Pattern/Global All Enemy")]
public class GlobalAllEnemyPattern : AreaPattern
{
    public override List<Vector3Int> GetAffectedTiles(Vector3Int center)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        
        if (GameManager.Instance == null) return tiles;

        GameManager.Player currentPlayer = GameManager.Instance.currentPlayer;
        
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != null && unit.owner != currentPlayer)
            {
                tiles.Add(unit.location);
            }
        }
        
        return tiles;
    }
}
