using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Revealed Scaling Effect", menuName = "Skills/Action Effects/Damage Revealed Scaling")]
public class DamageRevealedScalingEffect : DealDamageEffect
{
    public float revealedBonusCoefficient = 1.0f; // 노출 시 추가될 계수 (1.5 -> 2.5 면 1.0 추가)

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {
            float originalCoeff = attackCoefficient;
            
            // 타겟이 이미 노출된 상태라면 보너스 적용
            if (targetUnit.IsVisible)
            {
                attackCoefficient += revealedBonusCoefficient;
                Debug.Log($"[Backstab] 타겟 노출 상태! 계수 증가: {originalCoeff} -> {attackCoefficient}");
            }

            base.Apply(caster, targetTile);

            attackCoefficient = originalCoeff; // 복구
        }
        else
        {
            base.Apply(caster, targetTile);
        }
    }
}
