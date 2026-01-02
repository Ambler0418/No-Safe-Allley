using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button myTurnButton;
    public Button enemyTurnButton;

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
    
    // --- 추가된 상세 정보 UI ---
    public TextMeshProUGUI selectedUnitHPText;
    public TextMeshProUGUI selectedUnitAttackText;
    public TextMeshProUGUI selectedUnitDefenseText;
    public TextMeshProUGUI selectedUnitBuffsText; // 버프/디버프 목록 텍스트
    // -------------------------

    public Button useSkillButton; // 스킬 사용 버튼
    public TextMeshProUGUI useSkillButtonText; // 스킬 버튼 텍스트 (비용 표시 등)

    public Button useConditionalSkillButton; // 조건부 스킬 버튼
    public TextMeshProUGUI useConditionalSkillButtonText;

    void Start()
    {
        // 버튼 클릭 이벤트에 함수 연결
        if (myTurnButton != null) myTurnButton.onClick.AddListener(OnMyTurnButtonClicked);
        if (enemyTurnButton != null) enemyTurnButton.onClick.AddListener(OnEnemyTurnButtonClicked);
        if (useSkillButton != null) useSkillButton.onClick.AddListener(OnUseSkillButtonClicked);
        if (useConditionalSkillButton != null) useConditionalSkillButton.onClick.AddListener(OnUseConditionalSkillButtonClicked);
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

        UnitInstance selectedUnit = GameManager.Instance.selectedUnit;
        SkillEffect skillToUse = isConditional ? selectedUnit.ConditionalSkill : selectedUnit.ActiveSkill;

        if (skillToUse != null)
        {
            // 에너지 확인 (GetSkillCost 반영)
            if (GameManager.Instance.HasEnoughEnergy(selectedUnit.GetSkillCost(skillToUse)))
            {
                // GameManager에 스킬 정보 전달 및 타겟팅 모드 진입
                GameManager.Instance.currentSkillToUse = skillToUse;
                GameManager.Instance.EnterSkillTargetingMode();
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
        
        // UI 컴포넌트 유효성 검사 (Text 컴포넌트는 null일 수도 있으므로 체크)
        if (selectedUnitNameText != null) selectedUnitNameText.text = selectedUnit.sourceCardData.cardName;
        
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
            if (selectedUnit.activeStatuses.Count > 0)
            {
                string buffs = "";
                foreach (var status in selectedUnit.activeStatuses)
                {
                    buffs += $"{status.type}({status.value}) {status.remainingTurns}턴\n";
                }
                selectedUnitBuffsText.text = buffs;
            }
            else
            {
                selectedUnitBuffsText.text = "상태 이상 없음";
            }
        }
        // -------------------------

        // --- 스킬 1 버튼 업데이트 ---
        SkillEffect skill1 = selectedUnit.ActiveSkill;
        if (useSkillButton != null && useSkillButtonText != null)
        {
            if (selectedUnit.hasUsedSkillThisTurn)
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
                bool canUse = selectedUnit.CanUseConditionalSkill();
                int finalCost = selectedUnit.GetSkillCost(skill2);

                if (selectedUnit.hasUsedSkillThisTurn)
                {
                    useConditionalSkillButton.interactable = false;
                    useConditionalSkillButtonText.text = "사용 완료";
                }
                else if (canUse)
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

