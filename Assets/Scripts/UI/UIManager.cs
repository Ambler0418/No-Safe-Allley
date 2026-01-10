using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가
using System.Linq; // Linq 사용을 위해 추가

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button myTurnButton;
    public Button enemyTurnButton;
    public Button enemyProfileButton; // 적 본체(프로필) 클릭용 버튼

    [Header("Button Texts")]
    public TextMeshProUGUI myTurnButtonText;

    [Header("Stats Display")]
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI playerEnergyText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI enemyEnergyText;

    [Header("Selected Unit UI")]
    public GameObject selectedUnitPanel; // 유닛 선택 시 활성화될 패널
    public TextMeshProUGUI selectedUnitNameText; // 선택된 유닛 이름
    
    // --- 상세 정보 UI ---
    public TextMeshProUGUI selectedUnitHPText;
    public TextMeshProUGUI selectedUnitAttackText;
    public TextMeshProUGUI selectedUnitDefenseText;
    public TextMeshProUGUI selectedUnitBuffsText; // 버프/디버프 목록 텍스트
    // -------------------------

    public Button useSkillButton; // 스킬 사용 버튼
    public TextMeshProUGUI useSkillButtonText; // 스킬 버튼 텍스트 (비용 표시 등)

    public Button useConditionalSkillButton; // 조건부 스킬 버튼
    public TextMeshProUGUI useConditionalSkillButtonText;

    [Header("Special UI")]
    public TextMeshProUGUI deadEnemyCountText; // I004용 사망자 수 표시

    public TextMeshProUGUI enemyUnitCountText; // 디버그용 텍스트 필드
    private bool hasCheckedDeckForI004 = false;
    private bool hasI004InDeck = false;
    private bool hasCheckedDeckForS004 = false;
    private bool hasS004InDeck = false;

    void Start()
    {
        // 버튼 클릭 이벤트에 함수 연결
        if (myTurnButton != null) myTurnButton.onClick.AddListener(OnMyTurnButtonClicked);
        if (enemyTurnButton != null) enemyTurnButton.onClick.AddListener(OnEnemyTurnButtonClicked);
        if (useSkillButton != null) useSkillButton.onClick.AddListener(OnUseSkillButtonClicked);
        if (useConditionalSkillButton != null) useConditionalSkillButton.onClick.AddListener(OnUseConditionalSkillButtonClicked);
        if (enemyProfileButton != null) enemyProfileButton.onClick.AddListener(OnEnemyProfileClicked);

        // 초기 UI 숨김
        if (deadEnemyCountText != null) deadEnemyCountText.gameObject.SetActive(false);
        if (enemyUnitCountText != null) enemyUnitCountText.gameObject.SetActive(false);
    }

    void Update()
    {
        // 매 프레임 UI를 업데이트하여 항상 최신 게임 상태를 반영
        UpdateUI();
    }
    
    void OnDestroy()
    {
        // 특별히 해제할 이벤트가 없으므로 비워둠
    }

    // '내 턴 진행' 버튼 클릭 시
    private void OnMyTurnButtonClicked()
    {
        if (GameManager.Instance.currentPlayer != GameManager.Player.Player1) return;

        switch (GameManager.Instance.currentPhase)
        {
            case GameManager.GamePhase.Preparation:
                GameManager.Instance.EndPreparationPhase();
                break;
            case GameManager.GamePhase.Placement:
                GameManager.Instance.OnPlacementPhaseDone();
                break;
            case GameManager.GamePhase.Action:
                GameManager.Instance.EndPlayerTurn();
                break;
        }
    }

    // '상대 턴 종료' 버튼 클릭 시
    private void OnEnemyTurnButtonClicked()
    {
        GameManager.Instance.OnEnemyTurnEnd();
    }

    // '적 프로필' 버튼 클릭 시 (직격 공격 시도)
    private void OnEnemyProfileClicked()
    {
        if (GameManager.Instance.isTargetingSkill)
        {
            // 스킬 타겟팅 모드라면 스킬로서의 직격 공격 시도
            GameManager.Instance.HandleEnemyProfileClickInTargetingMode();
        }
        else
        {
            // 타겟팅 모드가 아니라면 일반 직격 공격 시도 (혹은 무반응, 기획에 따라 결정)
            GameManager.Instance.TryDirectAttack();
        }
    }

    // '스킬 사용' 버튼 클릭 시
    private void OnUseSkillButtonClicked()
    {
        HandleSkillClick(false);
    }

    // '조건부 스킬 사용' 버튼 클릭 시
    private void OnUseConditionalSkillButtonClicked()
    {
        HandleSkillClick(true);
    }

    private void HandleSkillClick(bool isConditional)
    {
        if (GameManager.Instance.selectedUnit == null) return;

        // 다른 스킬 버튼을 눌렀을 때, 기존 타겟팅 모드가 있다면 먼저 종료
        if (GameManager.Instance.isTargetingSkill)
        {
            GameManager.Instance.ExitSkillTargetingMode();
        }

        UnitInstance selectedUnit = GameManager.Instance.selectedUnit;
        SkillEffect skillToUse = isConditional ? selectedUnit.ConditionalSkill : selectedUnit.ActiveSkill;

        if (skillToUse != null)
        {
            // 에너지 확인 (GetSkillCost 반영)
            if (GameManager.Instance.HasEnoughEnergy(selectedUnit.GetSkillCost(skillToUse)))
            {
                // --- 개선된 즉시 시전 판정 로직 ---
                // 1. targetType이 Self이거나 None(전역/랜덤)인 경우
                // 2. AreaPattern이 없고 actionEffects 이름에 Self가 들어가는 경우 (기존 로직 보완)
                bool isImmediate = (skillToUse.targetType == SkillTargetType.Self || skillToUse.targetType == SkillTargetType.None);
                
                if (!isImmediate && skillToUse.areaPattern == null)
                {
                    if (skillToUse.actionEffects == null || skillToUse.actionEffects.Count == 0 || 
                        (skillToUse.actionEffects.Count > 0 && skillToUse.actionEffects[0].name.Contains("Self")))
                    {
                        isImmediate = true;
                    }
                }

                bool isGlobal = skillToUse.areaPattern != null && skillToUse.areaPattern.IsGlobal;

                Debug.Log($"[Skill Click] {skillToUse.skillName}: Immediate={isImmediate}, Global={isGlobal}");

                if (isImmediate || isGlobal)
                {
                    // 즉시 실행
                    int finalCost = selectedUnit.GetSkillCost(skillToUse);
                    if (GameManager.Instance.HasEnoughEnergy(finalCost))
                    {
                        // 스킬을 먼저 실행해보고 성공 여부를 확인
                        if (skillToUse.Execute(selectedUnit, selectedUnit.location))
                        {
                            GameManager.Instance.SpendEnergy(finalCost);
                            selectedUnit.hasUsedSkillThisTurn = true;
                            UpdateUI(); // 즉시 UI 갱신
                        }
                    }
                }
                else
                {
                    // GameManager에 스킬 정보 전달 및 타겟팅 모드 진입
                    GameManager.Instance.currentSkillToUse = skillToUse;
                    GameManager.Instance.EnterSkillTargetingMode();
                }
            }
            else
            {
                Debug.LogWarning($"{selectedUnit.sourceCardData.cardName} 스킬 사용 실패: 에너지가 부족합니다.");
            }
        }
    }

    // UI 상태를 업데이트하는 함수
    public void UpdateUI()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        UpdateTurnButtons(gm);
        UpdateStatsDisplay(gm);
        UpdateSelectedUnitPanel(gm);
    }

    private void UpdateTurnButtons(GameManager gm)
    {
        if (myTurnButton == null || enemyTurnButton == null || myTurnButtonText == null) return;

        // 현재 플레이어가 내가 아니거나, 상대 턴일 경우 '내 턴 진행' 버튼 비활성화
        if (gm.currentPlayer != GameManager.Player.Player1 || gm.currentPhase == GameManager.GamePhase.EnemyTurn)
        {
            myTurnButton.interactable = false;
        }
        else
        {
            myTurnButton.interactable = true;
        }

        // 상대 턴일 때만 '상대 턴 종료' 버튼 활성화
        enemyTurnButton.interactable = (gm.currentPhase == GameManager.GamePhase.EnemyTurn);

        // '내 턴 진행' 버튼의 텍스트 변경
        switch (gm.currentPhase)
        {
            case GameManager.GamePhase.Preparation:
                myTurnButtonText.text = "준비 완료";
                break;
            case GameManager.GamePhase.Placement:
                myTurnButtonText.text = "배치 완료";
                break;
            case GameManager.GamePhase.Action:
                myTurnButtonText.text = "턴 종료";
                break;
            default:
                myTurnButtonText.text = "대기";
                break;
        }
    }

    private void UpdateStatsDisplay(GameManager gm)
    {
        if (playerHPText != null) playerHPText.text = $"HP: {gm.player1Health}";
        if (playerEnergyText != null) playerEnergyText.text = $"Energy: {gm.player1Energy}";
        if (enemyHPText != null) enemyHPText.text = $"Enemy HP: {gm.player2Health}";
        if (enemyEnergyText != null) enemyEnergyText.text = $"Enemy Energy: {gm.player2Energy}";

        // I004 UI 업데이트
        if (deadEnemyCountText != null)
        {
            if (!hasCheckedDeckForI004)
            {
                if (HandManager.Instance != null)
                {
                    // 덱, 손패, 백업 덱을 모두 검사하여 I004(네크로필리아)가 있는지 확인
                    hasI004InDeck = HandManager.Instance.CheckDeckContainsCard("I004");
                    
                    hasCheckedDeckForI004 = true;
                    
                    // 카드가 없다면 텍스트를 비활성화
                    deadEnemyCountText.gameObject.SetActive(hasI004InDeck);
                }
            }

            if (hasI004InDeck)
            {
                deadEnemyCountText.text = $"사망: {gm.deadEnemyCount}";
            }
        }

        if (enemyUnitCountText != null)
        {
            if (!hasCheckedDeckForS004)
            {
                if (HandManager.Instance != null)
                {
                    // 덱, 손패, 백업 덱을 모두 검사하여 S004(네크로필리아)가 있는지 확인
                    hasS004InDeck = HandManager.Instance.CheckDeckContainsCard("S004");
                    
                    hasCheckedDeckForS004 = true;
                    
                    // 카드가 없다면 텍스트를 비활성화
                    enemyUnitCountText.gameObject.SetActive(hasS004InDeck);
                }
            }

            if (hasS004InDeck)
            {
                enemyUnitCountText.text = $"적 유닛 수: {gm.unitRegistry.Values.Where(u => (u.owner == GameManager.Player.Player2)&&(u.currentHealth>0)).Count()}";
            }
        }
    }

    private void UpdateSelectedUnitPanel(GameManager gm)
    {
        if (selectedUnitPanel == null) return;

        // 선택된 유닛이 없으면 패널을 비활성화
        if (gm.selectedUnit == null)
        {
            selectedUnitPanel.SetActive(false);
            return;
        }

        // 선택된 유닛이 있다면 패널 활성화
        selectedUnitPanel.SetActive(true);

        UnitInstance selectedUnit = gm.selectedUnit;
        
        // 적 유닛이고 식별되지 않았다면 정보 숨김
        if (selectedUnit.owner != GameManager.Player.Player1 && !selectedUnit.isIdentified)
        {
            if (selectedUnitNameText != null) selectedUnitNameText.text = "미식별 유닛";
            if (selectedUnitHPText != null) selectedUnitHPText.text = "HP: ???";
            if (selectedUnitAttackText != null) selectedUnitAttackText.text = "ATK: ???";
            if (selectedUnitDefenseText != null) selectedUnitDefenseText.text = "DEF: ???";
            if (selectedUnitBuffsText != null) selectedUnitBuffsText.text = "";
            
            // 버튼 비활성화
            if (useSkillButton != null) { useSkillButton.interactable = false; useSkillButtonText.text = "???"; }
            if (useConditionalSkillButton != null) { useConditionalSkillButton.gameObject.SetActive(false); }
            return;
        }

        // UI 컴포넌트 유효성 검사 (Text 컴포넌트는 null일 수도 있으므로 체크)
        if (selectedUnitNameText != null) selectedUnitNameText.text = selectedUnit.sourceCardData.cardName;
        
        // --- 가시성 상태 텍스트 생성 ---
        string visibilityStatus = "";
        if (selectedUnit.owner == GameManager.Player.Player1)
        {
            if (selectedUnit.isIdentified) visibilityStatus = "<color=red>[정보 식별됨]</color>\n";
            else if (selectedUnit.isRevealed) visibilityStatus = "<color=yellow>[위치 노출됨]</color>\n";
            else visibilityStatus = "<color=green>[은신 중]</color>\n";
        }

        // --- 추가된 상세 정보 업데이트 ---
        if (selectedUnitHPText != null) selectedUnitHPText.text = $"HP: {selectedUnit.currentHealth} / {selectedUnit.maxHealth}";
        if(selectedUnit.sourceCardData is UnitCard unitData)
        {
            if (selectedUnitAttackText != null) selectedUnitAttackText.text = $"ATK: {unitData.attack}";
            if (selectedUnitDefenseText != null) selectedUnitDefenseText.text = $"DEF: {unitData.defense}";
        }
        else
        {
            if (selectedUnitAttackText != null) selectedUnitAttackText.text = "";
            if (selectedUnitDefenseText != null) selectedUnitDefenseText.text = "";
        }

        if (selectedUnitBuffsText != null)
        {
            string buffs = visibilityStatus; // 가시성 상태(은신/노출)를 처음에 추가
            
            if (selectedUnit.activeStatuses.Count > 0)
            {
                System.Collections.Generic.HashSet<string> displayedNames = new System.Collections.Generic.HashSet<string>();

                foreach (var status in selectedUnit.activeStatuses)
                {
                    // 이름이 없으면 타입 이름 사용 (이전 호환성)
                    string displayName = string.IsNullOrEmpty(status.name) ? status.type.ToString() : status.name;

                    // 이미 표시된 이름이면 건너뜀 (중복 표시 방지)
                    if (displayedNames.Contains(displayName)) continue;

                    buffs += $"{displayName} ({status.remainingTurns}턴)\n";
                    displayedNames.Add(displayName);
                }
                selectedUnitBuffsText.text = buffs;
            }
            else
            {
                // 상태 이상이 없더라도 가시성 정보(buffs)가 있다면 그것을 표시
                selectedUnitBuffsText.text = string.IsNullOrEmpty(buffs) ? "상태 이상 없음" : buffs;
            }
        }
        // -------------------------

        bool isMyUnit = (selectedUnit.owner == gm.currentPlayer);
        bool isActionPhase = (gm.currentPhase == GameManager.GamePhase.Action);

        // --- 스킬 1 버튼 업데이트 ---
        SkillEffect skill1 = selectedUnit.ActiveSkill;
        if (useSkillButton != null && useSkillButtonText != null)
        {
            if (!isMyUnit)
            {
                useSkillButton.interactable = false;
                useSkillButtonText.text = "적 유닛";
            }
            else if (!isActionPhase)
            {
                useSkillButton.interactable = false;
                useSkillButtonText.text = "행동 단계 아님";
            }
            else if (selectedUnit.hasUsedSkillThisTurn)
            {
                useSkillButton.interactable = false;
                useSkillButtonText.text = "사용 완료";
            }
            else if (skill1 != null) 
            {
                int finalCost = selectedUnit.GetSkillCost(skill1);
                useSkillButton.interactable = true;
                useSkillButtonText.text = finalCost > 0 ? $"{skill1.skillName} ({finalCost})" : $"{skill1.skillName} (무료)";
            }
            else 
            {
                useSkillButton.interactable = false;
                useSkillButtonText.text = "스킬 없음";
            }
        }

        // --- 스킬 2 (조건부) 버튼 업데이트 ---
        SkillEffect skill2 = selectedUnit.ConditionalSkill;
        if (useConditionalSkillButton != null && useConditionalSkillButtonText != null)
        {
            if (skill2 == null)
            {
                useConditionalSkillButton.gameObject.SetActive(false);
            }
            else
            {
                useConditionalSkillButton.gameObject.SetActive(true);
                
                if (!isMyUnit)
                {
                    useConditionalSkillButton.interactable = false;
                    useConditionalSkillButtonText.text = "적 유닛";
                }
                else if (!isActionPhase)
                {
                    useConditionalSkillButton.interactable = false;
                    useConditionalSkillButtonText.text = "행동 단계 아님";
                }
                else if (selectedUnit.hasUsedSkillThisTurn)
                {
                    useConditionalSkillButton.interactable = false;
                    useConditionalSkillButtonText.text = "사용 완료";
                }
                else
                {
                    bool canUse = selectedUnit.CanUseConditionalSkill();
                    int finalCost = selectedUnit.GetSkillCost(skill2);

                    if (canUse)
                    {
                        useConditionalSkillButton.interactable = true;
                        useConditionalSkillButtonText.text = finalCost > 0 ? $"{skill2.skillName} ({finalCost})" : $"{skill2.skillName} (무료)";
                    }
                    else
                    {
                        useConditionalSkillButton.interactable = false;
                        useConditionalSkillButtonText.text = "조건 미충족";
                    }
                }
            }
        }
    }
}