using UnityEngine;

[CreateAssetMenu(fileName = "New Sacrifice Ritual Passive", menuName = "Skills/Passive/Sacrifice Ritual")]
public class SacrificeRitual : PassiveSkill
{
    public int healAmount = 300;

    public override void OnUnitDied(UnitInstance owner, UnitInstance deadUnit)
    {
        base.OnUnitDied(owner, deadUnit);

        // 아군이 죽었을 때만 발동
        if (deadUnit.owner != owner.owner) return;
        // 나 자신이 죽은 경우는 제외 (이미 파괴 로직 중이므로)
        if (deadUnit == owner) return;

        // 인접한 아군 찾기
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != owner && unit.owner == owner.owner && unit != deadUnit)
            {
                // 거리 1.5 이하 인접
                Vector3 worldPos1 = grid.GetCellCenterWorld(owner.location);
                Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                if (Vector3.Distance(worldPos1, worldPos2) < 1.5f * grid.cellSize.x)
                {
                    // 1명만 회복하고 종료
                    unit.heal(healAmount);
                    Debug.Log($"[Passive] {owner.sourceCardData.cardName}: {deadUnit.sourceCardData.cardName}의 희생으로 {unit.sourceCardData.cardName} 회복.");
                    return; 
                }
            }
        }
    }
}
