using UnityEngine;

[CreateAssetMenu(fileName = "New Deal Damage Effect", menuName = "Skills/Action Effects/Deal Damage")]
public class DealDamageEffect : ActionEffect
{
    [Header("Damage Calculation")]
    [Range(0.1f, 5.0f)] // 공격 계수 범위를 적절히 조절 (예: 0.1배 ~ 5배)
    public float attackCoefficient = 1.0f; // 공격력에 곱해질 계수 (예: 1.0f = 공격력의 100%)

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 유닛이 존재하고, 스킬 시전자와 다른 소유자일 경우에만 데미지 적용
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {
            // 데미지 계산: (공격자의 공격력 * 스킬의 공격 계수) * (500 / (500 + 방어자의 방어력))
            // 500f로 소수점 나눗셈을 강제하여 방어력에 따른 데미지 감소율이 올바르게 계산되도록 합니다.
            int calculatedDamage = Mathf.RoundToInt(caster.Attack * attackCoefficient * (500f / (500f + targetUnit.Defense)));

            if (calculatedDamage > 0)
            {
                targetUnit.TakeDamage(calculatedDamage);
                //Debug.Log($"{targetUnit.sourceCardData.cardName}이 {calculatedDamage} 데미지를 입었습니다. 남은 체력: {targetUnit.currentHealth}");
                //이거 UnitInstance.takeDamage에서 처리함
            }
            else
            {
                Debug.Log($"{targetUnit.sourceCardData.cardName}은 데미지를 입지 않았습니다 (방어력으로 상쇄).");
                // 데미지가 0이라도 공격받았다는 사실을 알려주기 위해 모습을 드러나게 할 수 있습니다.
                targetUnit.IsVisible = true;
            }
        }
        else
        {
            Debug.Log($"대상 타일 ({targetTile})에 유효한 적 유닛이 없습니다.");
        }
    }

}