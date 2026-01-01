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
    public Button useSkillButton; // 스킬 사용 버튼
    public TextMeshProUGUI useSkillButtonText; // 스킬 버튼 텍스트 (비용 표시 등)

    void Start()
    {
        // 버튼 클릭 이벤트에 함수 연결
        if (myTurnButton != null) myTurnButton.onClick.AddListener(OnMyTurnButtonClicked);
        if (enemyTurnButton != null) enemyTurnButton.onClick.AddListener(OnEnemyTurnButtonClicked);
        if (useSkillButton != null) useSkillButton.onClick.AddListener(OnUseSkillButtonClicked);
        else Debug.LogError("UIManager의 'Use Skill Button'이 Inspector에 연결되지 않았습니다!");
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
        if (GameManager.Instance.selectedUnit == null) return;

        UnitInstance selectedUnit = GameManager.Instance.selectedUnit;
        SkillEffect activeSkill = selectedUnit.ActiveSkill;

        // activeSkill이 있는지 확인하고 그 energyCost를 사용
        if (activeSkill != null)
        {
            // 1. 에너지 확인
            if (GameManager.Instance.HasEnoughEnergy(activeSkill.energyCost))
            {
                // 2. GameManager에 타겟팅 모드 진입 요청
                GameManager.Instance.EnterSkillTargetingMode();
            }
            else
            {
                Debug.LogWarning($"{selectedUnit.sourceCardData.cardName} 스킬 사용 실패: 에너지가 부족합니다.");
            }
        }
        else
        {
            Debug.LogWarning($"{selectedUnit.sourceCardData.cardName}은(는) 사용할 스킬이 없습니다.");
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
        
        // UI 컴포넌트 유효성 검사
        if (selectedUnitNameText == null || useSkillButtonText == null || useSkillButton == null) return;

        // 이름 표시 (UnitCard, BaseCard 모두 sourceCardData에 cardName이 있음)
        selectedUnitNameText.text = selectedUnit.sourceCardData.cardName;

        SkillEffect activeSkill = selectedUnit.ActiveSkill;

        // 스킬 사용 가능 여부에 따라 버튼 상태 변경
        if (selectedUnit.hasUsedSkillThisTurn)
        {
            useSkillButton.interactable = false;
            useSkillButtonText.text = "사용 완료";
        }
        else if (activeSkill != null) 
        {
            if (activeSkill.energyCost > 0) // 스킬이 있고, 에너지 비용이 0보다 크면
            {
                useSkillButton.interactable = true;
                useSkillButtonText.text = $"스킬 ({activeSkill.energyCost})";
            }
            else // 스킬이 있지만 에너지 비용이 0이거나 음수면 (무료 스킬)
            {
                useSkillButton.interactable = true; // 무료 스킬도 사용 가능하게
                useSkillButtonText.text = "스킬 (무료)";
            }
        }
        else // activeSkill이 null이면 스킬 없음
        {
            useSkillButton.interactable = false;
            useSkillButtonText.text = "스킬 없음";
        }
    }
}

