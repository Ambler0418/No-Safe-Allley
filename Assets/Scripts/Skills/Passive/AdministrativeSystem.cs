using UnityEngine;

[CreateAssetMenu(fileName = "New Administrative System Passive", menuName = "Skills/Passive/Administrative System")]
public class AdministrativeSystem : PassiveSkill
{
    public int energyAmount = 1;
    public int healAmount = 100;
    public Enums.Faction targetFaction = Enums.Faction.Government;
    public bool useFactionCheck = false;

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);

        // 1. 에너지 획득
        GameManager.Instance.AddEnergy(energyAmount);
        Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 에너지 {energyAmount} 획득.");

        // 2. 인접 아군 회복
        HealAdjacentAllies(owner);
    }

    private void HealAdjacentAllies(UnitInstance owner)
    {
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        int healedCount = 0;
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != owner && unit.owner == owner.owner)
            {
                // 진영 체크
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                // 논리적 좌표 체크로 인접 확인 (Grid 설정 오차 제거)
                if (IsAdjacent(owner.location, unit.location))
                {
                    unit.heal(healAmount);
                    healedCount++;
                }
            }
        }
        if (healedCount > 0)
        {
            Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 인접 아군 {healedCount}명에게 체력 {healAmount} 회복.");
        }
    }

    // 육각형 그리드(Pointy-top) 기준 정확한 인접 체크
    private bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        // 1. 같은 위치면 인접 아님
        if (pos1 == pos2) return false;

        // 2. 짝수 행 (Even Row)
        if (pos1.y % 2 == 0)
        {
            // 인접 오프셋: (-1,1), (0,1), (1,0), (0,-1), (-1,-1), (-1,0)
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;
            
            if (dy == 1) return dx == -1 || dx == 0;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 0 || dx == -1;
        }
        // 3. 홀수 행 (Odd Row)
        else
        {
            // 인접 오프셋: (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,0)
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;

            if (dy == 1) return dx == 0 || dx == 1;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 1 || dx == 0;
        }
        return false;
    }
}
