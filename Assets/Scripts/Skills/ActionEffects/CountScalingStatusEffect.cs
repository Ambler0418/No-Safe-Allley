using UnityEngine;

[CreateAssetMenu(fileName = "New Count Scaling Status Effect", menuName = "Skills/Action Effects/Count Scaling Status")]
public class CountScalingStatusEffect : ActionEffect
{
    [Header("Status To Apply")]
    public string statusName; // 추가
    public Enums.StatusType statusType;
    public int valuePerStack;  // 스택당 감소량 (여기선 50)
    public int duration;
    
    [Header("Condition")]
    public int maxStacks = 4;
    public Enums.Faction targetFaction = Enums.Faction.IronFrame;
    public bool useFactionCheck = false;

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 내 필드 아군 유닛 수 계산
        int allyCount = 0;
        GameManager.Player myPlayer = GameManager.Instance.currentPlayer;
        
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit.owner == myPlayer)
            {
                // 진영 체크
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                allyCount++;
            }
        }

        // 스택 제한
        int stacks = Mathf.Min(allyCount, maxStacks);
        int totalValue = stacks * valuePerStack;

        // 적에게 상태 이상 부여
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        if (targetUnit != null)
        {
            StatusEffect debuff = new StatusEffect(statusName, statusType, totalValue, duration, false, caster);
            targetUnit.AddStatus(debuff);
            Debug.Log($"[Fear] 아군 {allyCount}명 -> {stacks}스택. {targetUnit.sourceCardData.cardName}에게 {statusName}({totalValue}) 부여.");
            return true;
        }
        return false;
    }
}
