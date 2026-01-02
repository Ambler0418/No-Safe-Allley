using UnityEngine;

[CreateAssetMenu(fileName = "New Passive Skill", menuName = "Skills/Passive Skill")]
public class PassiveSkill : ScriptableObject
{
    public string skillName;
    [TextArea]
    public string description;

    // 턴 시작 시 발동
    public virtual void OnTurnStart(UnitInstance owner)
    {
        // 기본적으로 아무것도 하지 않음. 상속받아 구현.
        Debug.Log($"[Passive] {skillName} (OnTurnStart) - Owner: {owner.name}");
    }

    // 턴 종료 시 발동 (필요 시 구현)
    public virtual void OnTurnEnd(UnitInstance owner)
    {
    }

    // 데미지를 입었을 때 발동 (필요 시 구현)
    public virtual void OnTakeDamage(UnitInstance owner, int damage, UnitInstance attacker)
    {
    }

    // 유닛 사망 시 발동
    public virtual void OnUnitDied(UnitInstance owner, UnitInstance deadUnit)
    {
    }
}
