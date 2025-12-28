using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Skill Effect", menuName = "Skills/Skill Effect")]
public class SkillEffect : ScriptableObject
{
    [Header("Skill Info")]
    public string skillName;
    [TextArea]
    public string skillDescription;
    public int energyCost;

    [Header("Skill Composition")]
    public AreaPattern areaPattern; // 이 스킬이 사용할 범위 패턴
    public List<ActionEffect> actionEffects; // 이 스킬이 적용할 효과 목록
    // 실행 로직이 이제는 오케스트레이터 역할을 합니다.
    // 실행 로직이 이제는 오케스트레이터 역할을 합니다.
    public void Execute(UnitInstance caster, Vector3Int primaryTarget)
    {
        if (areaPattern == null || actionEffects.Count == 0) return;

        // 1. 범위 패턴을 사용해 모든 대상 타일을 가져옵니다.
        List<Vector3Int> affectedTiles = areaPattern.GetAffectedTiles(primaryTarget);

        // 2. 각각의 대상 타일에 대해
        foreach (var tile in affectedTiles)
        {
            // 3. 우리가 지정한 모든 액션 효과들을 순서대로 적용합니다.
            foreach (var effect in actionEffects)
            {
                effect.Apply(caster, tile);
            }
        }
    }
}