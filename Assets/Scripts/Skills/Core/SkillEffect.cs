using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스킬의 주요 타겟 유형을 정의합니다.
/// </summary>
public enum SkillTargetType
{
    Enemy, // 적군 대상 스킬
    Ally   // 아군 대상 스킬
}

[CreateAssetMenu(fileName = "New Skill Effect", menuName = "Skills/Skill Effect")]
public class SkillEffect : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    [TextArea]
    public string skillDescription;
    public int energyCost;

    // --- 아래 한 줄 추가 ---
    public SkillTargetType targetType; // 스킬의 타겟 유형 (아군/적군)
    // --------------------

    [Header("Skill Composition")]
    public AreaPattern areaPattern; // 이 스킬이 사용할 범위 패턴
    public List<ActionEffect> actionEffects; // 이 스킬이 적용할 효과 목록
    // 실행 로직이 이제는 오케스트레이터 역할을 합니다.
    // 실행 로직이 이제는 오케스트레이터 역할을 합니다.
    public void Execute(UnitInstance caster, Vector3Int primaryTarget)
    {
        if (actionEffects == null || actionEffects.Count == 0) return;

        // AreaPattern이 없는 경우 (타겟이 없는 스킬, 예: 에너지 충전, 카드 드로우)
        if (areaPattern == null)
        {
            Debug.Log($"'{skillName}'(은)는 타겟이 없는 스킬입니다. 효과를 1회 발동합니다.");
            foreach (var effect in actionEffects)
            {
                // caster와 targetTile이 필요없는 효과일 수 있으므로, 현재 가진 정보를 그대로 넘겨줍니다.
                effect.Apply(caster, primaryTarget); 
            }
        }
        else // AreaPattern이 있는 경우 (타겟이 있는 스킬)
        {
            List<Vector3Int> affectedTiles = areaPattern.GetAffectedTiles(primaryTarget);
            if (affectedTiles.Count == 0)
            {
                Debug.Log($"'{skillName}'(은)는 유효한 타겟 타일이 없어 효과가 발동하지 않았습니다.");
                return;
            }

            Debug.Log($"'{skillName}'(은)는 {affectedTiles.Count}개의 타일에 효과를 발동합니다.");
            foreach (var tile in affectedTiles)
            {
                foreach (var effect in actionEffects)
                {
                    effect.Apply(caster, tile);
                }
            }
        }
    }
}