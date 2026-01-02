using UnityEngine;

[CreateAssetMenu(fileName = "New Swap Position Effect", menuName = "Skills/Action Effects/Swap Position")]
public class SwapPositionSkillEffect : ActionEffect
{
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 1. 조건 검사: 시전자가 존재하고 숨어있어야 함
        if (caster == null)
        {
            Debug.LogError("[Swap Position] 시전자(caster)가 없습니다. 전술 카드로 사용할 수 없습니다.");
            return;
        }

        if (caster.IsVisible)
        {
            Debug.LogWarning("스킬 실패: 잠입 상태에서만 사용할 수 있습니다.");
            return;
        }

        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);

        // 2. 타겟 검사: 아군이고, 공개된 상태여야 함
        if (targetUnit != null && targetUnit.owner == caster.owner && targetUnit.IsVisible)
        {
            Vector3Int casterPos = caster.location;
            Vector3Int targetPos = targetUnit.location;

            // 3. 위치 교환 로직
            // 레지스트리 임시 제거
            GameManager.Instance.DeregisterUnit(casterPos);
            GameManager.Instance.DeregisterUnit(targetPos);

            // 좌표 정보 갱신
            caster.location = targetPos;
            targetUnit.location = casterPos;

            // 레지스트리 재등록
            GameManager.Instance.RegisterUnit(targetPos, caster);
            GameManager.Instance.RegisterUnit(casterPos, targetUnit);

            // 월드 위치 이동
            Grid grid = GameManager.Instance.gameGrid;
            caster.transform.position = grid.GetCellCenterWorld(targetPos);
            targetUnit.transform.position = grid.GetCellCenterWorld(casterPos);

            Debug.Log($"[Swap] {caster.sourceCardData.cardName}와 {targetUnit.sourceCardData.cardName}의 위치를 교환했습니다.");

            // 4. 후처리: 시전자 공개
            caster.IsVisible = true;
        }
        else
        {
            Debug.LogWarning("스킬 실패: 대상은 공개된 아군이어야 합니다.");
        }
    }
}
