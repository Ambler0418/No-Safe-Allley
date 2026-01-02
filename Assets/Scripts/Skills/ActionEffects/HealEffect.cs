using UnityEngine;      

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Skills/Action Effects/Heal")]
public class HealEffect : ActionEffect
{
    [Header("Heal Effect Settings")]
    public int healAmount = 30; // 회복량 설정
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);

        // 시전자 소유자 확인 (전술 카드인 경우 현재 플레이어)
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;
        string casterName = (caster != null) ? caster.sourceCardData.cardName : "전술 카드";

        // 해당 타일에 아군 유닛이 있다면
        if (targetUnit != null && targetUnit.owner == casterOwner)
        {
            targetUnit.heal(healAmount);
            Debug.Log($"{casterName}이(가) {targetUnit.sourceCardData.cardName}을(를) {healAmount}만큼 회복시켰습니다.");
        }
    }
}