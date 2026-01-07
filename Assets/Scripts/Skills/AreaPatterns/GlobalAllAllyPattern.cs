using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Global All Ally Pattern", menuName = "Skills/Area Pattern/Global All Ally")]
public class GlobalAllAllyPattern : AreaPattern
{
    public override bool IsGlobal => true;
    
    [Header("Faction Filter")]
    public bool useFactionCheck = false; // 진영 필터 사용 여부
    public Enums.Faction targetFaction = Enums.Faction.Government; // 대상 진영

    public override List<Vector3Int> GetAffectedTiles(Vector3Int center)
    {
        List<Vector3Int> tiles = new List<Vector3Int>();
        
        if (GameManager.Instance == null) return tiles;

        GameManager.Player currentPlayer = GameManager.Instance.currentPlayer;
        
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != null && unit.owner == currentPlayer)
            {
                // 진영 필터링 적용
                if (useFactionCheck && unit.Faction != targetFaction)
                {
                    continue;
                }
                
                tiles.Add(unit.location);
            }
        }
        
        return tiles;
    }
}
