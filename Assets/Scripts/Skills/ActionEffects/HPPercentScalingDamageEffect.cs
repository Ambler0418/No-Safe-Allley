using UnityEngine;

[CreateAssetMenu(fileName = "New HP Scaling Damage Effect", menuName = "Skills/Action Effects/HP Scaling Damage")]
public class HPPercentScalingDamageEffect : DealDamageEffect
{
    [Header("Scaling Condition")]
    public float missingHpPercentThreshold = 0.2f; // 잃은 체력 기준 (0.2 = 20%)
    public float bonusCoefficientPerStack = 0.1f;  // 스택당 추가 계수 (0.1 = 10%)

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        if (caster == null)
        {
            Debug.LogError("[HP Scaling Damage] 시전자(caster)가 없습니다. 이 스킬은 유닛 전용입니다.");
            return false;
        }

        float currentHpPercent = (float)caster.currentHealth / caster.maxHealth;
        float missingHpPercent = 1.0f - currentHpPercent;

        // 스택 계산 (예: 잃은 체력 45% -> 2스택)
        int stacks = Mathf.FloorToInt(missingHpPercent / missingHpPercentThreshold);
        float bonus = stacks * bonusCoefficientPerStack;

        float originalCoefficient = attackCoefficient;
        attackCoefficient += bonus;

        Debug.Log($"[HP Scaling] 잃은 체력 {missingHpPercent:P0} ({stacks} 스택). 계수 {originalCoefficient} -> {attackCoefficient}");

        bool result = base.Apply(caster, targetTile);

        attackCoefficient = originalCoefficient;
        return result;
    }
}
