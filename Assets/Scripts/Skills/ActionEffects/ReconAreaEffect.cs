using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recon Area Effect", menuName = "Skills/Action Effects/Recon Area")]
public class ReconAreaEffect : ActionEffect
{
    public int baseRange = 1;
    public int bonusConditionThreshold = 3;
    public int bonusRange = 1;
    public Enums.Faction targetFaction = Enums.Faction.Government;
    public bool useFactionCheck = false;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        int range = baseRange;

        // 시전자 소유자 확인
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        // 조건 검사: 인접 아군 수
        int adjacentAllies = 0;
        Grid grid = GameManager.Instance.gameGrid;
        if (grid != null)
        {
            foreach (var unit in GameManager.Instance.unitRegistry.Values)
            {
                // caster가 null이면 (전술 카드) 인접 아군 보너스를 체크할 기준점이 모호하므로 
                // targetTile(카드를 내려놓은 위치)를 기준으로 주변 아군을 세거나, 보너스를 포기함.
                // 여기서는 targetTile 주변을 체크함.
                Vector3 referencePos = (caster != null) ? grid.GetCellCenterWorld(caster.location) : grid.GetCellCenterWorld(targetTile);

                if (unit != caster && unit.owner == casterOwner)
                {
                    // 진영 체크
                    if (useFactionCheck && unit.Faction != targetFaction) continue;

                    // 거리 1.5 이하 인접
                    Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                    if (Vector3.Distance(referencePos, worldPos2) < 1.5f * grid.cellSize.x)
                    {
                        adjacentAllies++;
                    }
                }
            }
        }

        if (adjacentAllies >= bonusConditionThreshold)
        {
            range += bonusRange;
            Debug.Log($"[Recon] 조건 충족 (인접 {adjacentAllies} >= {bonusConditionThreshold}). 정찰 범위 증가: {baseRange} -> {range}");
        }

        // 범위 내 타일 정찰 수행
        List<Vector3Int> tilesToRecon = GetTilesInRange(grid, targetTile, range);
        
        foreach (var tile in tilesToRecon)
        {
            UnitInstance targetUnit = GameManager.Instance.GetUnitAt(tile);
            if (targetUnit != null && targetUnit.owner != casterOwner && !targetUnit.IsVisible)
            {
                TileEffectManager.Instance.HighlightReconTile(tile);
                targetUnit.IsVisible = true; // 유닛 발견!
                Debug.Log($"[Recon Area] {tile}에서 적 발견!");
            }
            else
            {
                TileEffectManager.Instance.HighlightReconTile(tile);
            }
        }
    }

    private List<Vector3Int> GetTilesInRange(Grid grid, Vector3Int center, int range)
    {
        List<Vector3Int> results = new List<Vector3Int>();
        // 육각형 그리드 범위 탐색 (Cube Coordinate 활용 권장)
        // 임시: 사각형 범위로 순회하며 거리 체크
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int checkPos = new Vector3Int(center.x + x, center.y + y, 0);
                // World Distance Check (반지름 = range * cellSize * 적정계수)
                Vector3 centerWorld = grid.GetCellCenterWorld(center);
                Vector3 checkWorld = grid.GetCellCenterWorld(checkPos);
                
                // 육각형 그리드에서 range 1은 중심 거리 약 1.0 ~ 1.732...
                // 넉넉하게 잡고 거른다.
                if (Vector3.Distance(centerWorld, checkWorld) <= range * grid.cellSize.x * 1.5f) // 계수 조정 필요
                {
                    results.Add(checkPos);
                }
            }
        }
        return results;
    }
}
