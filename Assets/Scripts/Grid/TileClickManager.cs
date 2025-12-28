using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 추가

public class TileClickManager : MonoBehaviour
{
    public Grid gameGrid; 
    public Tilemap allyTilemap;
    public Tilemap enemyTilemap;

    void Update()
    {
        // --- 스킬 대상 지정 로직 ---
        if (GameManager.Instance.isTargetingSkill)
        {
            // UI를 클릭했다면, 스킬 타겟팅으로 간주하지 않음
            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("UI 클릭은 스킬 타겟팅에서 제외됩니다.");
                return;
            }

            // 마우스 좌클릭으로 대상 지정
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector3Int clickedCell = gameGrid.WorldToCell(worldPos);
                clickedCell.z = 0;

                UnitInstance caster = GameManager.Instance.skillCaster;
                UnitCard unitCard = caster?.sourceCardData as UnitCard;

                if (unitCard != null && unitCard.activeSkill != null)
                {
                    SkillEffect skillToUse = unitCard.activeSkill;
                    UnitInstance targetUnit = GameManager.Instance.GetUnitAt(clickedCell);

                    // --- 타겟 유효성 검사 로직 (새로 추가/변경) ---
                    bool isValidTarget = false;
                    if (skillToUse.targetType == SkillTargetType.Ally)
                    {
                        // 아군 대상 스킬인데, 클릭한 곳에 아군이 있으면 유효
                        if (targetUnit != null && targetUnit.owner == caster.owner)
                        {
                            isValidTarget = true;
                        }
                    }
                    else // SkillTargetType.Enemy
                    {
                        // 적군 대상 스킬인데, 클릭한 곳에 적군이 있으면 유효
                        if (targetUnit != null && targetUnit.owner != caster.owner)
                        {
                            isValidTarget = true;
                        }
                    }
                    // -------------------------------------------

                    if (isValidTarget)
                    {
                        // 타겟이 유효할 때만 에너지 소모 및 스킬 실행
                        if (GameManager.Instance.SpendEnergy(skillToUse.energyCost))
                        {
                            caster.hasUsedSkillThisTurn = true;
                            skillToUse.Execute(caster, clickedCell);
                        }
                        // 스킬 사용 성공/실패 여부와 관계없이 타겟팅 모드 종료
                        GameManager.Instance.ExitSkillTargetingMode();
                    }
                    else
                    {
                        // 타겟이 유효하지 않으면, 메시지만 띄우고 스킬 모드는 유지
                        Debug.Log("잘못된 대상입니다. 다시 선택해주세요.");
                    }
                }
            }
            // 마우스 우클릭으로 대상 지정 취소
            else if (Input.GetMouseButtonDown(1))
            {
                GameManager.Instance.ExitSkillTargetingMode();
            }
            return; // 타겟팅 중에는 아래의 일반 클릭 로직을 실행하지 않음
        }

        // --- 일반 타일 클릭 로직 (타겟팅 중이 아닐 때) ---
        if (Input.GetMouseButtonDown(0))
        {
            // UI를 클릭했다면, 월드 클릭으로 간주하지 않고 무시
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            // 유닛 클릭은 UnitInstance의 OnMouseDown에서 처리되므로,
            // 여기서는 특별한 로직을 수행하지 않습니다.
            // "빈 공간 클릭 시 선택 해제" 로직을 제거하여 의도치 않은 선택 해제를 방지합니다.
        }
    }
}