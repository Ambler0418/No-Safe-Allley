using UnityEngine;

[CreateAssetMenu(fileName = "NewTacticsCard", menuName = "Card Data/Tactics Card")]
public class TacticsCard : CardData
{
    [Header("Tactic Skill")]
    // 전술 카드가 발동할 스킬 효과를 연결합니다.
    // 스킬의 에너지 비용, 범위, 효과 등은 모두 이 SkillEffect 애셋에 정의됩니다.

    public int energyCost;
    public SkillEffect tacticSkill;
}