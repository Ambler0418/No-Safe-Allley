using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using System.Security; // UI 클릭 감지를 위해 추가
using System.Collections.Generic;

public class TileClickManager : MonoBehaviour
{
    public Grid gameGrid; 
    public Tilemap allyTilemap;
    public Tilemap enemyTilemap;

    private Vector3Int lastHoveredTile;
    private bool isMoveHoverTileSet = false;
    private bool isSkillHoverTileSet = false;
    private List<Vector3Int> lastHighlightedAoe = new List<Vector3Int>(); // 스킬 범위 하이라이트 타일 추적

    void Update()
    {
        // 카드를 드래그 중일 때는 타일 상호작용(선택, 이동 등)을 막음
        if (HandManager.Instance != null && HandManager.Instance.IsDragging) return;

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
                    tem.RemoveMoveHighlight(lastHoveredTile);
                    isMoveHoverTileSet = false;
                }

                bool isValid = gm.GetUnitAt(currentHoverTile) == null && allyTilemap.HasTile(currentHoverTile);
                if (isValid)
                {
                    tem.AddMoveHighlight(currentHoverTile);
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
                tem.RemoveMoveHighlight(lastHoveredTile);
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
                // 이전 하이라이트 제거
                foreach (var tile in lastHighlightedAoe)
                {
                    tem.ClearEffectTileSafe(tile);
                }
                lastHighlightedAoe.Clear();
                isSkillHoverTileSet = false;

                SkillEffect skillToUse = gm.currentSkillToUse;
                if (skillToUse != null && skillToUse.areaPattern != null)
                {
                    // 마우스 위치가 유효한 타겟 타입의 타일인지 먼저 확인
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
                        // 유효하다면, 스킬의 AreaPattern에 따라 전체 범위 계산
                        lastHighlightedAoe = skillToUse.areaPattern.GetAffectedTiles(currentHoverTile);
                        
                        // 계산된 모든 타일에 하이라이트 적용
                        foreach (var tile in lastHighlightedAoe)
                        {
                            bool shouldHighlight = false;
                            if (skillToUse.targetType == SkillTargetType.Ally)
                            {
                                shouldHighlight = allyTilemap.HasTile(tile);
                            }
                            else if (skillToUse.targetType == SkillTargetType.Enemy)
                            {
                                shouldHighlight = enemyTilemap.HasTile(tile);
                            }

                            if (shouldHighlight)
                            {
                                tem.AddMoveHighlight(tile);
                            }
                        }
                        isSkillHoverTileSet = true;
                    }
                }
                lastHoveredTile = currentHoverTile;
            }

            // 클릭 로직
            if (Input.GetMouseButtonDown(1))
            {
                foreach (var tile in lastHighlightedAoe)
                {
                    tem.ClearEffectTileSafe(tile);
                }
                lastHighlightedAoe.Clear();
                isSkillHoverTileSet = false;

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
                    bool isValidTile = false;
                    UnitInstance targetUnit = gm.GetUnitAt(currentHoverTile);
                    
                    // 1. 사거리 체크
                    float dist = Vector3.Distance(gameGrid.GetCellCenterWorld(caster.location), gameGrid.GetCellCenterWorld(currentHoverTile));
                    // 육각형 그리드 1칸 거리 = cellSize.x * 1.0
                    // 여유분을 두어 1.1배로 계산. maxRange가 1이면 인접(약 1.0~1.2) 허용
                    // 2칸 거리 = 1.732 or 2.0.
                    // 간단히: (maxRange * gameGrid.cellSize.x * 1.1f)
                    
                    bool inRange = dist <= (skillToUse.maxRange * gameGrid.cellSize.x * 1.1f) + 0.1f;
                    
                    // 본인 위치 클릭(거리 0)은 인접으로 치지 않거나, 스킬에 따라 허용될 수 있음.
                    // 보통 버프류는 본인 제외일 수 있으나 일단 포함하고, 필요 시 caster != targetUnit 체크.
                    // G003은 "인접 아군"이므로 거리 > 0.1f 조건이 필요할 수 있음. 
                    // 하지만 maxRange 체크만으로는 충분. (본인에게 쓰는 건 기획 의도에 따라)

                    if (inRange)
                    {
                        if (skillToUse.targetType == SkillTargetType.Ally)
                        {
                            // 아군 타겟 스킬: 해당 위치에 아군 유닛이 있어야 함
                            if (targetUnit != null && targetUnit.owner == GameManager.Player.Player1)
                            {
                                // G003(인접 아군) 같은 경우 본인 제외가 필요할 수 있음.
                                // 일단은 아군이면 OK
                                isValidTile = true;
                            }
                        }
                        else if (skillToUse.targetType == SkillTargetType.Enemy)
                        {
                            // 적군 타겟 스킬
                            // 적 유닛이 있거나, (빈 땅 공격이 가능한 경우) 적 타일맵 위여야 함
                            // 여기서는 일단 기존 로직(타일맵 체크) 유지하되 거리 체크 추가됨
                            if (enemyTilemap.HasTile(currentHoverTile))
                            {
                                isValidTile = true;
                            }

                            // 💥 도발(Provoked) 체크 💥
                            if (isValidTile && caster.HasStatus(Enums.StatusType.Provoked))
                            {
                                StatusEffect provocation = caster.activeStatuses.Find(s => s.type == Enums.StatusType.Provoked);
                                if (provocation != null && provocation.creator != null)
                                {
                                    if (currentHoverTile != provocation.creator.location)
                                    {
                                        Debug.Log($"도발 효과로 인해 {provocation.creator.sourceCardData.cardName}만 공격할 수 있습니다!");
                                        isValidTile = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                         // 사거리 밖
                         Debug.Log($"사거리 밖입니다. (Distance: {dist}, MaxRange: {skillToUse.maxRange})");
                    }

                    if (isValidTile)
                    {
                        // 유효한 타일이면 에너지를 먼저 체크하고 실행을 시도합니다.
                        int finalCost = caster.GetSkillCost(skillToUse);
                        if (gm.HasEnoughEnergy(finalCost))
                        {
                            // 💥 투사체 발사 💥
                            if (skillToUse.projectilePrefab != null)
                            {
                                Vector3 startPos = caster.transform.position;
                                Vector3 endPos = gameGrid.GetCellCenterWorld(currentHoverTile);
                                gm.SpawnProjectile(startPos, endPos, skillToUse.projectilePrefab);
                            }

                            // 실제 스킬 로직을 실행하고 그 결과(성공 여부)를 받습니다.
                            if (skillToUse.Execute(caster, currentHoverTile))
                            {
                                // 성공했을 때만 에너지 소모 및 행동 완료 처리
                                gm.SpendEnergy(finalCost);
                                caster.hasUsedSkillThisTurn = true;
                                Debug.Log($"스킬 {skillToUse.skillName} 사용 성공.");
                            }
                            else
                            {
                                // 조건 불만족 등으로 실패한 경우
                                Debug.Log("스킬 사용 실패: 조건이 맞지 않습니다.");
                            }
                        }
                        
                        // 성공/실패 여부와 상관없이 타겟팅 모드는 종료합니다.
                        foreach (var tile in lastHighlightedAoe)
                        {
                            tem.ClearEffectTileSafe(tile);
                        }
                        lastHighlightedAoe.Clear();
                        isSkillHoverTileSet = false; 
                        gm.ExitSkillTargetingMode();
                    }
                    else
                    {
                        // 잘못된 대상을 클릭하면 즉시 취소
                        Debug.Log("유효하지 않은 대상입니다. 스킬 사용이 취소됩니다.");
                        foreach (var tile in lastHighlightedAoe)
                        {
                            tem.ClearEffectTileSafe(tile);
                        }
                        lastHighlightedAoe.Clear();
                        isSkillHoverTileSet = false;
                        gm.ExitSkillTargetingMode();
                    }
                }
            }
            return;
        }
        else
        {
            if (isSkillHoverTileSet)
            {
                tem.ClearEffectTileSafe(lastHoveredTile);
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
                // 1. 적 유닛(내 유닛이 아님)인 경우 식별 여부 체크
                bool isEnemy = (unit.owner != GameManager.Player.Player1);
                if (isEnemy && !unit.isIdentified)
                {
                    Debug.Log($"[TileClickManager] 선택 불가: 적 유닛 {unit.sourceCardData.cardName}은 식별되지 않았습니다. (Owner: {unit.owner}, isIdentified: {unit.isIdentified})");
                    gm.DeselectUnit();
                    return;
                }

                // 2. 내 유닛인 경우 배치 단계라면 이동 모드 진입
                if (!isEnemy && gm.currentPhase == GameManager.GamePhase.Placement)
                {
                    gm.SelectUnit(unit);
                    gm.EnterMoveMode(unit);
                }
                else
                {
                    // 3. 그 외의 경우 (내 유닛 정보 보기 또는 식별된 적 유닛 정보 보기)
                    gm.SelectUnit(unit);
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