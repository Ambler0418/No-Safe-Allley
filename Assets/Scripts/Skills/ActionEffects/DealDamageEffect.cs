using UnityEngine;

[CreateAssetMenu(fileName = "New Deal Damage Effect", menuName = "Skills/Action Effects/Deal Damage")]
public class DealDamageEffect : ActionEffect
{
    public int damageAmount;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        if (targetUnit != null && targetUnit.owner != caster.owner)
        {
            targetUnit.TakeDamage(damageAmount);
            Debug.Log($"{targetTile}의 유닛에게 {damageAmount} 피해!");
        }
    }
}