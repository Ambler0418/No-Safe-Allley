using UnityEngine;

[CreateAssetMenu(fileName = "New Sacrifice For Buff Effect", menuName = "Skills/Action Effects/Sacrifice For Buff")]
public class SacrificeForBuffEffect : ActionEffect
{
    public int hpCost = 100;
    public string statusName; // 추가
    public Enums.StatusType statusType;
    public int value;
    public int duration;

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 시전자 소유자 확인
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        if (targetUnit != null && targetUnit.owner == casterOwner)
        {
            if (targetUnit.owner == casterOwner)
            {
                // 체력 소모 (ModifyHealth 호출로 UI 갱신 보장)
                targetUnit.ModifyHealth(-hpCost);
                
                // 버프 부여
                StatusEffect buff = new StatusEffect(statusName, statusType, value, duration, false, caster);
                targetUnit.AddStatus(buff);
                Debug.Log($"[Sacrifice] {targetUnit.sourceCardData.cardName} 체력 {hpCost} 소모, {statusName}({value}) 획득.");
                return true;
            }
        }
        return false;
    }
}
