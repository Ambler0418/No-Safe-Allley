using UnityEngine;

[CreateAssetMenu(fileName = "AddStatusToRevealedEffect", menuName = "Skills/Action Effects/Add Status To Revealed")]
public class AddStatusToRevealedEffect : ActionEffect
{
    [Header("Status Settings")]
    public Enums.StatusType statusType;
    public string statusName;
    public int value;
    public int duration;

    public override bool Apply(UnitInstance caster, Vector3Int targetLocation)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetLocation);

        if (targetUnit == null) return false;

        // 1. 아군인지 확인
        if (targetUnit.owner != (caster != null ? caster.owner : GameManager.Instance.currentPlayer))
        {
            Debug.LogWarning("스킬 실패: 아군에게만 사용할 수 있습니다.");
            return false;
        }

        // 2. 공개(Identified) 상태인지 확인 (S003 핵심 조건)
        if (!targetUnit.isIdentified)
        {
            Debug.LogWarning($"스킬 실패: {targetUnit.sourceCardData.cardName}은(는) 아직 은신 상태입니다.");
            return false;
        }

        // 3. 상태 부여
        targetUnit.AddStatus(statusName, statusType, value, duration, caster);
        Debug.Log($"[S003] {targetUnit.sourceCardData.cardName}에게 {statusName} 효과 부여 성공.");
        
        return true;
    }
}
