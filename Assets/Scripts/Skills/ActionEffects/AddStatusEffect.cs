using UnityEngine;

[CreateAssetMenu(fileName = "New Add Status Effect", menuName = "Skills/Action Effects/Add Status")]
public class AddStatusEffect : ActionEffect
{
    [Header("Status To Apply")]
    public string statusName;  // 표시될 이름 (예: [소화])
    public Enums.StatusType statusType;
    public int value;          // 버프/디버프 수치
    public int duration;       // 지속 시간 (턴)
    public bool isPermanent;   // 영구 지속 여부
    
    [Header("Targeting")]
    public bool applyToSelf = false; // true면 타겟 대신 시전자에게 적용
    public bool onlyIfVisible = false; // true면 발각된 유닛에게만 적용 (F005용)

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit;

        if (applyToSelf)
        {
            if (caster == null)
            {
                Debug.LogWarning("[AddStatusEffect] 시전자(caster)가 없어 'applyToSelf'를 적용할 수 없습니다. (전술 카드인 경우)");
                return false;
            }
            targetUnit = caster;
        }
        else
        {
            targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        }

        if (targetUnit != null)
        {
            // F005: 노출되지 않은 유닛에게는 부여 불가 처리
            if (onlyIfVisible && !targetUnit.isRevealed)
            {
                Debug.Log($"[AddStatusEffect] {targetUnit.sourceCardData.cardName}이(가) 숨겨져 있어 효과를 부여하지 못했습니다.");
                return false;
            }

            // 새로운 상태 이상 객체 생성 (시전자 정보 및 이름 포함)
            StatusEffect newStatus = new StatusEffect(statusName, statusType, value, duration, isPermanent, caster);
            targetUnit.AddStatus(newStatus);
            Debug.Log($"[AddStatusEffect] {targetUnit.sourceCardData.cardName}에게 {statusName}({statusType}) 효과를 {duration}턴 동안 부여했습니다.");
            return true;
        }
        else
        {
            // 대상이 없는 것은 에러가 아닐 수 있음 (특히 광역 스킬이나 빈 땅에 쏜 경우)
            // 따라서 Warning 대신 일반 Log로 변경하여 불필요한 콘솔 경고를 줄임
            // Debug.Log($"[AddStatusEffect] 대상 유닛이 없습니다. 효과를 적용하지 않습니다. (Tile: {targetTile})");
            return false;
        }
    }
}
