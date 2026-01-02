using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Adjacent Passive", menuName = "Skills/Passive/Buff Adjacent")]
public class BuffAdjacentPassive : PassiveSkill
{
    public Enums.StatusType statusType;
    public int value;
    public int duration;
    public Enums.Faction targetFaction = Enums.Faction.Government; // 기본값은 정부, 필요시 수정 가능하도록 노출
    public bool useFactionCheck = false; // 진영 체크 여부

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);

        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != owner && unit.owner == owner.owner)
            {
                // 진영 체크가 켜져있다면 진영 확인
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                if (IsAdjacent(owner.location, unit.location))
                {
                    StatusEffect buff = new StatusEffect(statusType, value, duration);
                    unit.AddStatus(buff);
                }
            }
        }
    }

    private bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        // 간단 거리 체크 (HealAdjacentPassive와 동일 로직)
        Vector3 worldPos1 = GameManager.Instance.gameGrid.GetCellCenterWorld(pos1);
        Vector3 worldPos2 = GameManager.Instance.gameGrid.GetCellCenterWorld(pos2);
        return Vector3.Distance(worldPos1, worldPos2) < 1.5f * GameManager.Instance.gameGrid.cellSize.x;
    }
}
