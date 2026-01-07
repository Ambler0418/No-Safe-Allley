using UnityEngine;

[CreateAssetMenu(fileName = "New Deal Damage Effect", menuName = "Skills/Action Effects/Deal Damage")]
public class DealDamageEffect : ActionEffect
{
    [Header("Damage Calculation")]
    [Range(0.1f, 5.0f)] // 공격 계수 범위를 적절히 조절 (예: 0.1배 ~ 5배)
    public float attackCoefficient = 1.0f; // 공격력에 곱해질 계수 (예: 1.0f = 공격력의 100%)
    
    [Header("Condition")]
    public bool onlyIfVisible = false; // 공개된 적에게만 데미지

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 전술 카드로 데미지를 주려고 할 때 caster가 null인 경우 처리
        if (caster == null)
        {
            Debug.LogError("[DealDamageEffect] 시전자(caster)가 없습니다. 전술 카드는 'DealDamageEffect'를 직접 사용할 수 없습니다. (고정 데미지 효과 필요)");
            return false;
        }

        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 유닛이 존재하고, 스킬 시전자와 다른 소유자일 경우에만 데미지 적용
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {

            int atk = caster.Attack;
            float coeff = attackCoefficient;
            float dealtMult = caster.DamageDealtMultiplier;
            int def = targetUnit.Defense;
            float takenMult = targetUnit.DamageTakenMultiplier;

            // 데미지 계산: (공격자의 공격력 * 스킬 계수 * 가하는 피해 배율) * (500 / (500 + 방어력)) * 받는 피해 배율
            int calculatedDamage = Mathf.RoundToInt(
                atk * coeff * dealtMult * 
                (500f / (500f + def)) * 
                takenMult
            );

            Debug.Log($"[Damage Calc] {caster.sourceCardData.cardName} -> {targetUnit.sourceCardData.cardName}: " +
                      $"ATK:{atk} * Coeff:{coeff} * DealtMult:{dealtMult} * (500/(500+{def})) * TakenMult:{takenMult} = Result:{calculatedDamage}");

            if (calculatedDamage > 0)
            {
                targetUnit.TakeDamage(calculatedDamage);
            }
            else
            {
                Debug.Log($"{targetUnit.sourceCardData.cardName}은 데미지를 입지 않았습니다 (최종 데미지 0).");
                targetUnit.isIdentified = true;
            }
            


            return true;
        }
        else
        {
            Debug.Log($"대상 타일 ({targetTile})에 유효한 적 유닛이 없습니다.");
            return false;
        }
    }

}