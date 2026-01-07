using UnityEngine;

[CreateAssetMenu(fileName = "New Buff All Ally Passive", menuName = "Skills/Passive/Buff All Ally")]
public class BuffAllAllyPassive : PassiveSkill
{
    public string statusName; // 추가
    public Enums.StatusType statusType;
    public int value;
    public int duration = 1;
    public Enums.Faction targetFaction = Enums.Faction.IronFrame;
    public bool useFactionCheck = false;

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);
        ApplyGlobalBuff(owner);
    }

    public override void OnBoardChange(UnitInstance owner)
    {
        base.OnBoardChange(owner);
        ApplyGlobalBuff(owner);
    }

    private void ApplyGlobalBuff(UnitInstance owner)
    {
        if (GameManager.Instance == null) return;

        GameManager.Player currentPlayer = owner.owner;
        int count = 0;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != null && unit.owner == currentPlayer)
            {
                // 진영 체크
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                // 이미 동일한 버프가 있다면 제거 후 새로 부여 (중첩 방지 및 시간 갱신)
                // 유닛 액티브 스킬과 달리 패시브 오라는 항상 최신 상태를 유지해야 함
                StatusEffect buff = new StatusEffect(statusName, statusType, value, duration, false, owner);
                unit.AddStatus(buff);
                count++;
            }
        }
        
        if (count > 0)
        {
            Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 모든 아군({targetFaction})에게 {statusName}({value}) 부여/갱신 완료.");
        }
    }
}
