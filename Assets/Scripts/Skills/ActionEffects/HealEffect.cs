using UnityEngine;      

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Skills/Action Effects/Heal")]
public class HealEffect : ActionEffect
{
    [Header("Heal Effect Settings")]
    public int healAmount = 30; // 회복량 설정
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);

        // 해당 타일에 적 유닛이 있고, 그 유닛이 현재 보이지 않는 상태라면
        if (targetUnit != null && targetUnit.owner == caster.owner)
        {
            // 모습을 보이게 하고 타일을 강조표시합니다.
            targetUnit.heal(healAmount);
            Debug.Log($"{caster.sourceCardData.cardName}이(가) {targetUnit.sourceCardData.cardName}을(를) {healAmount}만큼 회복시켰습니다.");
        }
    }
}