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

                // 아군 타일이 아닌 모든 곳을 대상으로 간주
                if (!allyTilemap.HasTile(clickedCell))
                {
                    GameManager.Instance.ExecuteReconSkill(clickedCell);
                }
                else
                {
                    Debug.Log("스킬 사용 실패: 아군 영역에는 사용할 수 없습니다.");
                    GameManager.Instance.ExitSkillTargetingMode(); // 잘못된 위치 클릭 시 타겟팅 모드 종료
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