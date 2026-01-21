using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 스킬의 주요 타겟 유형을 정의합니다.
/// </summary>
public enum SkillTargetType
{
    Enemy, // 적군 대상 스킬
    Ally,   // 아군 대상 스킬
    Self,   // 자신에게 즉시 시전 (유닛 액티브)
    None    // 타겟 없음 (전역 전술 카드 등)
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
    public int maxRange = 1; // 사거리 (기본 1: 인접)
    // --------------------

    [Header("Skill Composition")]
    public AreaPattern areaPattern; // 이 스킬이 사용할 범위 패턴
    public List<ActionEffect> actionEffects; // 타겟(범위 내)에게 적용할 효과 목록
    public List<ActionEffect> casterEffects; // 시전자(본인)에게 적용할 효과 목록 (추가)
    public GameObject projectilePrefab; // 스킬 사용 시 발사될 투사체 프리팹 (추가)

    // 실행 로직이 이제는 오케스트레이터 역할을 합니다.
    public bool Execute(UnitInstance caster, Vector3Int primaryTarget)
    {
        bool anyEffectApplied = false;

        // 1. 시전자 본인에게 적용할 효과 처리 (패턴과 상관없이 1회 실행)
        if (casterEffects != null && casterEffects.Count > 0 && caster != null)
        {
            Debug.Log($"'{skillName}' 시전자 효과 발동: {caster.sourceCardData.cardName}");
            foreach (var effect in casterEffects)
            {
                if (effect != null)
                {
                    effect.Apply(caster, caster.location);
                    anyEffectApplied = true;
                }
            }
        }

        // 2. 기존 타겟팅 효과 처리
        if (actionEffects == null || actionEffects.Count == 0) 
        {
            // 타겟 효과는 없지만 시전자 효과가 있었다면 성공으로 간주
            return anyEffectApplied;
        }

        // AreaPattern이 없는 경우 (타겟이 없는 스킬, 예: 에너지 충전, 카드 드로우)
        if (areaPattern == null)
        {
            Debug.Log($"'{skillName}'(은)는 타겟이 없는 스킬입니다. 효과를 1회 발동합니다.");
            foreach (var effect in actionEffects)
            {
                if (effect != null)
                {
                    if (effect.Apply(caster, primaryTarget))
                    {
                        anyEffectApplied = true;
                    }
                }
            }
        }
        else // AreaPattern이 있는 경우 (타겟이 있는 스킬)
        {
            List<Vector3Int> affectedTiles = areaPattern.GetAffectedTiles(primaryTarget);
            if (affectedTiles.Count == 0)
            {
                Debug.Log($"'{skillName}'(은)는 유효한 타겟 타일이 없어 효과가 발동하지 않았습니다.");
                // 시전자 효과라도 발동했으면 true, 아니면 false
                return anyEffectApplied;
            }

            Debug.Log($"'{skillName}'(은)는 {affectedTiles.Count}개의 타일에 효과를 발동합니다.");
            foreach (var tile in affectedTiles)
            {
                foreach (var effect in actionEffects)
                {
                    if (effect != null)
                    {
                        if (effect.Apply(caster, tile))
                        {
                            anyEffectApplied = true;
                        }
                    }
                }
            }
        }

        return anyEffectApplied;
    }
}