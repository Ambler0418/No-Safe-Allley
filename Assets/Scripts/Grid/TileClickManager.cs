using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 추가

public class TileClickManager : MonoBehaviour
{
    public Grid gameGrid; 
    public Tilemap allyTilemap;
    public Tilemap enemyTilemap;

    private Vector3Int lastHoveredTile;
    private bool isMoveHoverTileSet = false;
    private bool isSkillHoverTileSet = false;

    void Update()
    {
        var gm = GameManager.Instance;
        var tem = TileEffectManager.Instance;

        // --- 유닛 이동 로직 ---
        if (gm.isMovingUnit)
        {
            if (gm.justEnteredMoveMode)
            {
                gm.justEnteredMoveMode = false;
                return;
            }

            // 호버 로직
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int currentHoverTile = gameGrid.WorldToCell(worldPos);
            currentHoverTile.z = 0;

            if (currentHoverTile != lastHoveredTile)
            {
                if (isMoveHoverTileSet)
                {
                    tem.effectTilemap.SetTile(lastHoveredTile, null);
                    isMoveHoverTileSet = false;
                }

                bool isValid = gm.GetUnitAt(currentHoverTile) == null && allyTilemap.HasTile(currentHoverTile);
                if (isValid)
                {
                    tem.effectTilemap.SetTile(currentHoverTile, tem.moveHighlightTile);
                    isMoveHoverTileSet = true;
                }
                lastHoveredTile = currentHoverTile;
            }

            // 클릭 로직
            if (Input.GetMouseButtonDown(1))
            {
                gm.ExitMoveMode();
                return;
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;

                if (currentHoverTile == gm.unitToMove.location)
                {
                    Debug.Log("이동을 취소합니다.");
                    gm.ExitMoveMode();
                    return;
                }
                
                bool isValid = gm.GetUnitAt(currentHoverTile) == null && allyTilemap.HasTile(currentHoverTile);
                if (isValid)
                {
                    gm.ExecuteMove(currentHoverTile);
                }
                else
                {
                    Debug.Log("이동할 수 없는 타일입니다. 이동을 취소합니다.");
                    gm.ExitMoveMode();
                }
            }
            return;
        }
        else
        {
            if (isMoveHoverTileSet)
            {
                tem.effectTilemap.SetTile(lastHoveredTile, null);
                isMoveHoverTileSet = false;
            }
        }

        // --- 스킬 대상 지정 로직 ---
        if (gm.isTargetingSkill)
        {
            // 호버 로직
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int currentHoverTile = gameGrid.WorldToCell(worldPos);
            currentHoverTile.z = 0;

            if (currentHoverTile != lastHoveredTile)
            {
                if (isSkillHoverTileSet)
                {
                    tem.effectTilemap.SetTile(lastHoveredTile, null);
                    isSkillHoverTileSet = false;
                }
                
                SkillEffect skillToUse = gm.currentSkillToUse; // gm.skillCaster?.ActiveSkill 대신 사용
                if (skillToUse != null)
                {
                    bool isValidHover = false;
                    if (skillToUse.targetType == SkillTargetType.Ally)
                    {
                        isValidHover = allyTilemap.HasTile(currentHoverTile);
                    }
                    else // Enemy
                    {
                        isValidHover = enemyTilemap.HasTile(currentHoverTile);
                    }

                    if (isValidHover)
                    {
                        tem.effectTilemap.SetTile(currentHoverTile, tem.moveHighlightTile); 
                        isSkillHoverTileSet = true;
                    }
                }
                lastHoveredTile = currentHoverTile;
            }

            // 클릭 로직
            if (Input.GetMouseButtonDown(1))
            {
                gm.ExitSkillTargetingMode();
                return; 
            }

            if (Input.GetMouseButtonDown(0) && EventSystem.current.IsPointerOverGameObject()) return;

            if (Input.GetMouseButtonDown(0))
            {
                UnitInstance caster = gm.skillCaster;
                SkillEffect skillToUse = gm.currentSkillToUse; // caster?.ActiveSkill 대신 사용

                if (skillToUse != null)
                {
                    // --- 수정된 타겟 유효성 검사 ---
                    // 유닛의 존재 여부가 아닌, 타일맵의 소속으로 타겟 유효성 판단
                    bool isValidTile = false;
                    if (skillToUse.targetType == SkillTargetType.Ally)
                    {
                        isValidTile = allyTilemap.HasTile(currentHoverTile);
                    }
                    else // Enemy
                    {
                        isValidTile = enemyTilemap.HasTile(currentHoverTile);

                        // 💥 도발(Provoked) 체크 추가 💥
                        if (isValidTile && caster.HasStatus(Enums.StatusType.Provoked))
                        {
                            // 시전자에게 걸린 도발 효과 찾기
                            StatusEffect provocation = caster.activeStatuses.Find(s => s.type == Enums.StatusType.Provoked);
                            if (provocation != null && provocation.creator != null)
                            {
                                // 도발 시전자의 위치와 클릭한 위치가 다르면 무효
                                if (currentHoverTile != provocation.creator.location)
                                {
                                    Debug.Log($"도발 효과로 인해 {provocation.creator.sourceCardData.cardName}만 공격할 수 있습니다!");
                                    isValidTile = false;
                                }
                            }
                        }
                    }

                    if (isValidTile)
                    {
                        // 유효한 타일이면 유닛 존재 여부와 관계없이 스킬 발동 및 에너지 소모
                        int finalCost = caster.GetSkillCost(skillToUse);
                        if (gm.SpendEnergy(finalCost))
                        {
                            caster.hasUsedSkillThisTurn = true;
                            skillToUse.Execute(caster, currentHoverTile);
                        }
                        gm.ExitSkillTargetingMode();
                    }
                    else
                    {
                        // 이제 이 부분은 맵 바깥을 클릭했을 때만 호출됨
                        Debug.Log("지정할 수 없는 영역입니다.");
                    }
                }
            }
            return;
        }
        else
        {
            if (isSkillHoverTileSet)
            {
                tem.effectTilemap.SetTile(lastHoveredTile, null);
                isSkillHoverTileSet = false;
            }
        }

        // --- 일반 타일 클릭 로직 ---
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int clickedTile = gameGrid.WorldToCell(worldPos);
            clickedTile.z = 0;

            UnitInstance unit = gm.GetUnitAt(clickedTile);
            if (unit != null)
            {
                // 현재 턴의 플레이어가 유닛의 소유자가 아니면 아무것도 하지 않음
                if (unit.owner != gm.currentPlayer)
                {
                    Debug.Log($"상대방의 유닛({unit.sourceCardData.cardName})은 선택할 수 없습니다.");
                    return;
                }

                // 게임 단계에 따라 다른 행동 수행
                switch (gm.currentPhase)
                {
                    // 배치 단계: 유닛 이동 모드 진입
                    case GameManager.GamePhase.Placement:
                        gm.EnterMoveMode(unit);
                        break;
                    
                    // 행동 단계: 스킬 사용을 위해 유닛 선택
                    case GameManager.GamePhase.Action:
                        gm.SelectUnit(unit);
                        break;
                }
            }
            else
            {
                // 빈 땅을 클릭하면 선택 해제
                gm.DeselectUnit();
            }
        }
    }
}