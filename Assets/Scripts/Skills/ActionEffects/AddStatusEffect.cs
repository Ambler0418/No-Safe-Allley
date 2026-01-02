using UnityEngine;

[CreateAssetMenu(fileName = "New Add Status Effect", menuName = "Skills/Action Effects/Add Status")]
public class AddStatusEffect : ActionEffect
{
    [Header("Status To Apply")]
    public Enums.StatusType statusType;
    public int value;          // 버프/디버프 수치
    public int duration;       // 지속 시간 (턴)
    public bool isPermanent;   // 영구 지속 여부
    
    [Header("Targeting")]
    public bool applyToSelf = false; // true면 타겟 대신 시전자에게 적용

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit;

        if (applyToSelf)
        {
            if (caster == null)
            {
                Debug.LogWarning("[AddStatusEffect] 시전자(caster)가 없어 'applyToSelf'를 적용할 수 없습니다. (전술 카드인 경우)");
                return;
            }
            targetUnit = caster;
        }
        else
        {
            targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        }

        if (targetUnit != null)
        {
            // 새로운 상태 이상 객체 생성 (시전자 정보 포함)
            StatusEffect newStatus = new StatusEffect(statusType, value, duration, isPermanent, caster);
            targetUnit.AddStatus(newStatus);
            Debug.Log($"[AddStatusEffect] {targetUnit.sourceCardData.cardName}에게 {statusType} (Val:{value}) 효과를 {duration}턴 동안 부여했습니다.");
        }
        else
        {
            Debug.LogWarning($"[AddStatusEffect] 대상이 유효하지 않습니다. (Tile: {targetTile}, Self: {applyToSelf})");
        }
    }
}
