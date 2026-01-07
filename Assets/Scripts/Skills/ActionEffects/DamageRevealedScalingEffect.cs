using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Revealed Scaling Effect", menuName = "Skills/Action Effects/Damage Revealed Scaling")]
public class DamageRevealedScalingEffect : DealDamageEffect
{
    public float revealedBonusCoefficient = 1.0f; // 노출 시 추가될 계수 (1.5 -> 2.5 면 1.0 추가)

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {
            float originalCoeff = attackCoefficient;
            
            // 타겟이 이미 노출된 상태라면 보너스 적용
            if (targetUnit.isRevealed)
            {
                attackCoefficient += revealedBonusCoefficient;
                Debug.Log($"[Backstab] 타겟 노출 상태! 계수 증가: {originalCoeff} -> {attackCoefficient}");
            }

            bool result = base.Apply(caster, targetTile);

            attackCoefficient = originalCoeff; // 복구
            return result;
        }
        else
        {
            return base.Apply(caster, targetTile);
        }
    }
}
