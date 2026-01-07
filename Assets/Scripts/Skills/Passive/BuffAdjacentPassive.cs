using UnityEngine;

[CreateAssetMenu(fileName = "New Buff Adjacent Passive", menuName = "Skills/Passive/Buff Adjacent")]
public class BuffAdjacentPassive : PassiveSkill
{
    public string statusName; // 추가
    public Enums.StatusType statusType;
    public int value;
    public int duration;
    public Enums.Faction targetFaction = Enums.Faction.Government; // 기본값은 정부, 필요시 수정 가능하도록 노출
    public bool useFactionCheck = false; // 진영 체크 여부

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);
        ApplyBuffToAdjacent(owner);
    }

    public override void OnBoardChange(UnitInstance owner)
    {
        base.OnBoardChange(owner);
        ApplyBuffToAdjacent(owner);
    }

    private void ApplyBuffToAdjacent(UnitInstance owner)
    {
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        int appliedCount = 0;
        // 동시 수정 방지를 위해 리스트 복사 (혹시 모를 오류 대비)
        var units = new System.Collections.Generic.List<UnitInstance>(GameManager.Instance.unitRegistry.Values);

        foreach (var unit in units)
        {
            if (unit != null && unit != owner && unit.owner == owner.owner)
            {
                // 진영 체크가 켜져있다면 진영 확인
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                if (IsAdjacent(owner.location, unit.location))
                {
                    StatusEffect buff = new StatusEffect(statusName, statusType, value, duration, false, owner);
                    unit.AddStatus(buff);
                    appliedCount++;
                }
            }
        }
        if (appliedCount > 0)
        {
            Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 인접 아군 {appliedCount}명에게 {statusName} 부여 (갱신).");
        }
    }

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
