using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Global All Ally Pattern", menuName = "Skills/Area Pattern/Global All Ally")]
public class GlobalAllAllyPattern : AreaPattern
{
    public override List<Vector3Int> GetAffectedTiles(Vector3Int center)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        
        GameManager.Player currentPlayer = GameManager.Instance.currentPlayer;
        
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit.owner == currentPlayer)
            {
                tiles.Add(unit.location);
            }
        }
        
        return tiles;
    }
}
