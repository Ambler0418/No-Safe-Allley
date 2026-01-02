using UnityEngine;

[CreateAssetMenu(fileName = "New Life Steal Damage Effect", menuName = "Skills/Action Effects/Life Steal Damage")]
public class LifeStealDamageEffect : DealDamageEffect
{
    public float lifeStealPercent = 0.5f;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        if (caster == null)
        {
            Debug.LogError("[Life Steal Damage] 시전자(caster)가 없습니다.");
            return;
        }

        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {
            // 데미지 계산 (부모 클래스 로직 복사)
            int calculatedDamage = Mathf.RoundToInt(
                caster.Attack * attackCoefficient * caster.DamageDealtMultiplier * 
                (500f / (500f + targetUnit.Defense)) * 
                targetUnit.DamageTakenMultiplier
            );
            
            if (calculatedDamage > 0)
            {
                // 실제 데미지 적용
                targetUnit.TakeDamage(calculatedDamage);
                
                // 흡혈 적용
                int healAmount = Mathf.RoundToInt(calculatedDamage * lifeStealPercent);
                if (healAmount > 0)
                {
                    caster.heal(healAmount);
                    Debug.Log($"[Life Steal] {caster.sourceCardData.cardName}이 {healAmount}만큼 흡혈했습니다.");
                }
            }
            else
            {
                // 데미지 0이면 효과 없음 (IsVisible 처리 등은 부모에게 맡길 수 없음)
                targetUnit.IsVisible = true;
            }
        }
        else
        {
            // 빈 땅 공격 등은 부모 로직(직접 공격)을 따를지 말지 결정.
            // 흡혈 스킬이므로 직접 공격 시에는 흡혈 안 함.
            base.Apply(caster, targetTile);
        }
    }
}
