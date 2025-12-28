using UnityEngine;
using System; // 이벤트를 위해 추가
using System.Collections.Generic; // Dictionary 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    // 게임 단계 변경 시 발생하는 이벤트
    public event Action OnPhaseChanged;

    // 게임 단계 정의
    public enum GamePhase
    {
        Preparation,  // 준비 단계
        Placement,    // 배치 단계
        Action,       // 행동 단계
        EnemyTurn     // 상대 턴
    }

    // 플레이어 구분
    public enum Player
    {
        Player1,
        Player2
    }

    [Header("Game State")]
    private GamePhase _currentPhase;
    public GamePhase currentPhase
    {
        get { return _currentPhase; }
        private set
        {
            if (_currentPhase != value)
            {
                _currentPhase = value;
                OnPhaseChanged?.Invoke(); // Phase가 변경될 때 이벤트 발생
            }
        }
    }
    public Player currentPlayer;

    [Header("Turn State")]
    public bool hasPlacedCardThisTurn = false;

    [Header("Selection State")]
    public UnitInstance selectedUnit; // 현재 선택된 유닛
    public bool isTargetingSkill = false; // 스킬 대상 지정 모드 여부
    public UnitInstance skillCaster; // 현재 스킬을 사용하려는 유닛

    [Header("Game Data")]
    public Dictionary<Vector3Int, UnitInstance> unitRegistry = new Dictionary<Vector3Int, UnitInstance>();
    public Grid gameGrid; // 테스트용 유닛 생성 시 좌표 계산을 위해 Grid 참조 추가

    [Header("Card Data for Test")]
    public UnitCard boomCardData; // 테스트용으로 생성할 Boom 카드 데이터

    [Header("Player Stats")]
    public int player1Health = 1000;
    public int player2Health = 1000;
    public int player1Energy = 100;
    public int player2Energy = 100;


    void Awake()
    {
        // 싱글톤 패턴 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 GameManager는 파괴되지 않음
        }
    }

    // --- 유닛 레지스트리 관리 ---
    public void RegisterUnit(Vector3Int position, UnitInstance unit)
    {
        if (unitRegistry.ContainsKey(position))
        {
            Debug.LogError($"위치 {position}에 유닛을 등록하려고 했지만, 이미 다른 유닛이 등록되어 있습니다.");
            return;
        }
        unitRegistry[position] = unit;
    }

    public void DeregisterUnit(Vector3Int position)
    {
        if (unitRegistry.ContainsKey(position))
        {
            unitRegistry.Remove(position);
        }
    }

    public UnitInstance GetUnitAt(Vector3Int position)
    {
        unitRegistry.TryGetValue(position, out UnitInstance unit);
        return unit;
    }
    
    void Start()
    {
        // 게임 시작 (테스트를 위해 Start에서 바로 호출)
        StartGame();
    }

    // 게임 시작
    public void StartGame()
    {
        Debug.Log("===== Game Start! =====");
        currentPlayer = Player.Player1;
        currentPhase = GamePhase.Preparation; // 이벤트 발생
        Debug.Log("Phase: Preparation - 유닛과 거점을 배치하세요.");

        // --- 테스트 코드 호출 ---
        SetupReconTest();
        // ---------------------

        // 여기에 초기 카드 7장 드로우 로직 추가 (HandManager 연동 필요)
        // 예: HandManager.Instance.DrawInitialHand(7);
    }

    // --- 테스트용 적 유닛 생성 ---
    private void SetupReconTest()
    {
        if (boomCardData == null || PlacementManager.Instance == null || gameGrid == null)
        {
            Debug.LogWarning("테스트 유닛 생성에 필요한 데이터(Boom Card, PlacementManager, Grid)가 부족하여 스킵합니다.");
            return;
        }

        // TODO: 아래 좌표는 맵에 맞게 수정해야 합니다.
        Vector3Int topRowPos = new Vector3Int(-1, 9, 0); 
        Vector3Int bottomRowPos = new Vector3Int(-2, 10, 0);

        SpawnEnemyUnitForTest(boomCardData, topRowPos);
        SpawnEnemyUnitForTest(boomCardData, bottomRowPos);
    }

    private void SpawnEnemyUnitForTest(UnitCard card, Vector3Int cellLocation)
    {
        // PlacementManager의 프리팹을 사용하여 유닛 오브젝트 생성
        GameObject newUnitObject = Instantiate(PlacementManager.Instance.unitPrefab, gameGrid.GetCellCenterWorld(cellLocation), Quaternion.identity);
        UnitInstance unitInstance = newUnitObject.GetComponent<UnitInstance>();
        
        unitInstance.Initialize(card);
        unitInstance.owner = Player.Player2; // 소유자를 Player2로 설정
        unitInstance.location = cellLocation;
        unitInstance.IsVisible = false; // 보이지 않게 설정

        RegisterUnit(cellLocation, unitInstance); // 레지스트리에 등록

        // 스프라이트 설정
        SpriteRenderer sr = newUnitObject.GetComponent<SpriteRenderer>();
        if (sr != null && card.unitSprite != null)
        {
            sr.sprite = card.unitSprite;
        }
        Debug.Log($"TEST: 적 유닛 {card.cardName}을(를) {cellLocation}에 생성했습니다.");
    }
    // --- 테스트 코드 끝 ---

    // 플레이어가 준비 단계를 마쳤을 때 호출
    public void EndPreparationPhase()
    {
        if (currentPhase != GamePhase.Preparation) return;

        Debug.Log("Preparation Phase End. Starting first turn.");
        StartPlayerTurn();
    }

    // 플레이어 턴 시작
    public void StartPlayerTurn()
    {
        currentPlayer = Player.Player1;
        hasPlacedCardThisTurn = false; // 턴 시작 시 배치 플래그 초기화
        DeselectUnit(); // 턴 시작 시 유닛 선택 해제

        // 모든 아군 유닛의 '스킬 사용' 상태를 초기화
        foreach (var unit in unitRegistry.Values)
        {
            if (unit.owner == currentPlayer)
            {
                unit.hasUsedSkillThisTurn = false;
            }
        }

        Debug.Log("===== Player 1's Turn =====");
        Debug.Log("Phase: Placement - 카드를 1장 드로우하고 유닛/거점을 배치하세요.");
        // 여기에 카드 1장 드로우 로직 추가
        // 예: HandManager.Instance.DrawCard(1);
        currentPhase = GamePhase.Placement; // 이벤트 발생
    }

    // 플레이어 턴 종료 (배치/행동 단계가 모두 끝났을 때)
    public void EndPlayerTurn()
    {
        if (currentPlayer != Player.Player1) return;

        DeselectUnit(); // 턴 종료 시에도 선택 해제
        Debug.Log("Player 1's turn ends.");
        currentPlayer = Player.Player2;
        Debug.Log("===== Waiting for Enemy's Turn =====");
        currentPhase = GamePhase.EnemyTurn; // 이벤트 발생
    }

    // [중요] 상대방의 턴이 끝났을 때 외부(AI, 네트워크 등)에서 호출해 줄 함수
    public void OnEnemyTurnEnd()
    {
        if (currentPhase != GamePhase.EnemyTurn)
        {
            Debug.LogWarning("OnEnemyTurnEnd was called, but it's not the enemy's turn.");
            return;
        }

        Debug.Log("Enemy's turn ended.");
        // 다음 플레이어 턴 시작
        StartPlayerTurn();
    }

    // 참고: UI 버튼 등에서 호출할 함수들
    // 예: 플레이어가 '배치 완료' 버튼을 누르면 호출
    public void OnPlacementPhaseDone()
    {
        if (currentPhase == GamePhase.Placement && currentPlayer == Player.Player1)
        {
            DeselectUnit(); // 단계 변경 시 선택 해제
            Debug.Log("Phase: Action - 전술 카드나 유닛 스킬을 사용하세요.");
            currentPhase = GamePhase.Action; // 이벤트 발생
        }
    }

    // 예: 플레이어가 '턴 종료' 버튼을 누르면 호출
    public void OnTurnEndButtonPressed()
    {
        if (currentPhase == GamePhase.Action && currentPlayer == Player.Player1)
        {
            EndPlayerTurn();
        }
    }

    // --- 유닛 선택 시스템 ---
    public void SelectUnit(UnitInstance unit)
    {
        // 다른 유닛이 선택된 상태에서 새 유닛을 선택하면 기존 선택을 해제하고 새 유닛을 선택
        if (selectedUnit != null && selectedUnit != unit)
        {
            DeselectUnit();
        }

        // 같은 유닛을 다시 클릭하면 선택 해제
        if (selectedUnit == unit)
        {
            DeselectUnit();
            return;
        }

        selectedUnit = unit;
        Debug.Log($"유닛 선택: {selectedUnit.sourceCardData.cardName}");
        // 참고: 여기서 UIManager에게 이벤트를 보내 UI를 업데이트 하도록 할 수 있음
    }

    public void DeselectUnit()
    {
        if (selectedUnit != null)
        {
            Debug.Log("유닛 선택 해제");
            selectedUnit = null;
        }
    }

    // --- 스킬 사용 시스템 ---
    public void EnterSkillTargetingMode()
    {
        if (selectedUnit == null) return;

        isTargetingSkill = true;
        skillCaster = selectedUnit; // 스킬 시전자 저장
        Debug.Log($"{skillCaster.sourceCardData.cardName}의 스킬 대상 지정 시작. 적 타일을 클릭하세요.");
        
        // 유닛 선택은 유지하되, 다른 행동을 막기 위해 UI를 비활성화 할 수도 있음
    }

    public void ExitSkillTargetingMode()
    {
        isTargetingSkill = false;
        skillCaster = null;
        Debug.Log("스킬 대상 지정 모드 종료.");
    }


    // --- 에너지 관리 시스템 ---

    /// <summary>
    /// 현재 플레이어가 특정 비용만큼의 에너지를 가지고 있는지 확인합니다.
    /// </summary>
    public bool HasEnoughEnergy(int cost)
    {
        if (currentPlayer == Player.Player1)
        {
            return player1Energy >= cost;
        }
        // 현재는 Player1만 플레이하므로 Player2는 false를 반환합니다.
        // else if (currentPlayer == Player.Player2) { return player2Energy >= cost; }
        return false;
    }

    /// <summary>
    /// 현재 플레이어의 에너지를 특정 비용만큼 소모합니다. 성공 시 true, 실패 시 false를 반환합니다.
    /// </summary>
    public bool SpendEnergy(int cost)
    {
        if (!HasEnoughEnergy(cost))
        {
            Debug.LogWarning("에너지가 부족하여 스킬을 사용할 수 없습니다.");
            return false;
        }

        if (currentPlayer == Player.Player1)
        {
            player1Energy -= cost;
            Debug.Log($"에너지 {cost} 소모. 남은 에너지: {player1Energy}");
        }
        
        // UI 업데이트를 위해 이벤트를 발생시키거나 직접 UIManager를 호출할 수 있습니다.
        // 여기서는 UIManager가 매 프레임 UI를 업데이트하므로 별도 호출은 생략합니다.
        return true;
    }

    /// <summary>
    /// 현재 플레이어의 에너지를 특정 양만큼 회복합니다.
    /// </summary>
    public void AddEnergy(int amount)
    {
        if (currentPlayer == Player.Player1)
        {
            player1Energy += amount;
            Debug.Log($"에너지 {amount} 회복. 현재 에너지: {player1Energy}");
        }
        // 추후 Player2 로직 추가
    }
}
