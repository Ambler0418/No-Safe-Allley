using UnityEngine;

[CreateAssetMenu(fileName = "New Swap Position Effect", menuName = "Skills/Action Effects/Swap Position")]
public class SwapPositionSkillEffect : ActionEffect
{
    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 1. 조건 검사: 시전자가 존재하고 완전히 숨어있어야 함 (위치 노출 안됨, 정보 식별 안됨)
        if (caster == null)
        {
            Debug.LogError("[Swap Position] 시전자(caster)가 없습니다.");
            return false;
        }

        if (caster.isRevealed || caster.isIdentified)
        {
            Debug.LogWarning("스킬 실패: 자신의 위치가 노출되지 않고 공개되지 않았을 때만 사용할 수 있습니다.");
            return false;
        }

        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);

        // 2. 타겟 검사: 아군이고, 정보가 공개된(Identified) 상태여야 함
        if (targetUnit != null && targetUnit.owner == caster.owner && targetUnit.isIdentified)
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
            if (grid != null)
            {
                caster.transform.position = grid.GetCellCenterWorld(targetPos);
                targetUnit.transform.position = grid.GetCellCenterWorld(casterPos);
            }

            Debug.Log($"[Swap] {caster.sourceCardData.cardName}와 {targetUnit.sourceCardData.cardName}의 위치를 교환했습니다.");

            // 4. 후처리
            // 시전자(잠입자) 공개 (정보 식별됨)
            caster.isIdentified = true;
            caster.isRevealed = true;

            // 위치가 바뀐 아군 유닛은 다시 은신 상태로 (사용자 요청)
            targetUnit.isIdentified = false;
            targetUnit.isRevealed = false;
            
            // 보드 상태 변화 알림
            GameManager.Instance.TriggerBoardChangeEvents();
            
            return true;
        }
        else
        {
            Debug.LogWarning("스킬 실패: 대상은 공개된 아군이어야 합니다.");
            return false;
        }
    }
}
