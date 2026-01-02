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

        // 2. 인접 유닛 체크 및 회복
        if (CheckAdjacentAlly(owner))
        {
            owner.heal(healAmount);
            Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 인접 아군이 있어 체력 {healAmount} 회복.");
        }
    }

    private bool CheckAdjacentAlly(UnitInstance owner)
    {
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return false;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != owner && unit.owner == owner.owner)
            {
                // 진영 체크
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                // 거리 1.5 이하 인접
                Vector3 worldPos1 = grid.GetCellCenterWorld(owner.location);
                Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                if (Vector3.Distance(worldPos1, worldPos2) < 1.5f * grid.cellSize.x)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
