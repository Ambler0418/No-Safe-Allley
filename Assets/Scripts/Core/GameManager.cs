using UnityEngine;
using System; // 이벤트를 위해 추가
using System.Collections.Generic; // Dictionary 사용을 위해 추가
using System.Collections; // 코루틴 사용을 위해 추가
using System.Linq;       // Linq 사용을 위해 추가
using Map; // BattleEncounter 사용을 위해 추가

public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    // 게임 단계 변경 시 발생하는 이벤트
    public event Action OnPhaseChanged;

    // 현재 진행 중인 (또는 진행 예정인) 전투 데이터
    public BattleEncounter currentEncounter;

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
    public int placementActionsPerTurn = 1; // 배치 단계에서 할 수 있는 행동 수
    public int placementActionsTaken = 0;   // 이번 턴에 한 행동 수

    [Header("Selection State")]
    public UnitInstance selectedUnit; // 현재 선택된 유닛
    public bool isTargetingSkill = false; // 스킬 대상 지정 모드 여부
    public SkillEffect currentSkillToUse; // 현재 사용 중인 스킬 (추가)
    public UnitInstance skillCaster; // 현재 스킬을 사용하려는 유닛
    public bool isMovingUnit = false; // 유닛 이동 모드 여부
    public UnitInstance unitToMove = null; // 현재 이동하려는 유닛
    public bool justEnteredMoveMode = false; // 이동 모드 진입 직후의 클릭을 무시하기 위한 플래그

    [Header("Game Data")]
    public Dictionary<Vector3Int, UnitInstance> unitRegistry = new Dictionary<Vector3Int, UnitInstance>();
    public Grid gameGrid; // 테스트용 유닛 생성 시 좌표 계산을 위해 Grid 참조 추가

    [Header("Card Data for Test")]
    public UnitCard AssaultCardData; // 테스트용으로 생성할 Boom 카드 데이터
    public UnitCard ScoutCardData; // 테스트용으로 생성할 Scout 카드 데이터

    [Header("Player Stats")]
    public int player1Health = 15;
    public int player2Health = 15;
    public int player1Energy = 100;
    public int player2Energy = 100;

    [Header("Combat Tracking")]
    public int deadEnemyCount = 0; // 사망한 적 유닛 수 (I004 스킬용)

    [Header("AI State")]
    public List<CardData> enemyDeck = new List<CardData>();
    public List<CardData> enemyHand = new List<CardData>();

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

        // 게임 단계가 변경될 때마다 HandlePhaseChange 함수를 호출하도록 구독
        OnPhaseChanged += HandlePhaseChange;
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제
        OnPhaseChanged -= HandlePhaseChange;
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

            public void NotifyUnitDeath(UnitInstance deadUnit)
            {
                Debug.Log($"[NotifyUnitDeath] {deadUnit.sourceCardData.cardName} 사망 알림 시작.");
        
                // 적 유닛이 사망한 경우 카운트 증가 (플레이어가 Player1이라고 가정)
                if (deadUnit.owner == Player.Player2 && deadUnit.sourceCardData is UnitCard)
                {
                    deadEnemyCount++;
                    Debug.Log($"적 유닛 사망 카운트 증가: {deadEnemyCount}");
                }
        
                // 모든 유닛의 패시브 스킬에 사망 이벤트 전파
                // 컬렉션 변경 오류 방지를 위해 리스트 복사 후 순회
                List<UnitInstance> myUnits = unitRegistry.Values.Where(u => u.owner == Player.Player1 && u.currentHealth > 0).ToList();
                foreach (var unit in myUnits)
                {
                    if (unit != null && unit.gameObject.activeInHierarchy) // 파괴되지 않은 유닛만
                    {
                        // 디버깅: 현재 필드에 있는 유닛들이 이벤트를 수신하는지 확인
                        Debug.Log($"[Event Check] {unit.sourceCardData.cardName} 확인 중...");
        
                        if (unit.sourceCardData is BaseCard baseCard)
                        {
                            if (baseCard.passiveSkill != null)
                            {
                                Debug.Log($" -> {baseCard.cardName}의 패시브 스킬 {baseCard.passiveSkill.name} 호출 시도.");
                                baseCard.passiveSkill.OnUnitDied(unit, deadUnit);
                            }
                            else
                            {
                                Debug.Log($" -> {baseCard.cardName}은 패시브 스킬이 없음.");
                            }
                        }
                    }
                }
                Debug.Log("[NotifyUnitDeath] 알림 종료.");
            }
    public void ResetDeadEnemyCount()
    {
        deadEnemyCount = 0;
        Debug.Log("적 유닛 사망 카운트가 초기화되었습니다.");
    }

    public UnitInstance GetUnitAt(Vector3Int position)
    {
        unitRegistry.TryGetValue(position, out UnitInstance unit);
        return unit;
    }

    /// <summary>
    /// 해당 플레이어의 유닛이 필드에 존재하는지 확인합니다.
    /// </summary>
    public bool HasUnits(Player player)
    {
        foreach (var unit in unitRegistry.Values)
        {
            if (unit.owner == player)
            {
                return true;
            }
        }
        return false;
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
        
        // 씬 전환 후 Grid 참조가 끊겼을 수 있으므로 다시 찾습니다.
        if (gameGrid == null)
        {
            if (PlacementManager.Instance != null && PlacementManager.Instance.gameGrid != null)
            {
                gameGrid = PlacementManager.Instance.gameGrid;
            }
            else
            {
                // PlacementManager가 아직 초기화되지 않았을 수도 있으므로 Find로 찾기 시도
                GameObject gridObj = GameObject.Find("Grid"); // 씬에 Grid 이름의 오브젝트가 있다고 가정
                if (gridObj != null) gameGrid = gridObj.GetComponent<Grid>();
            }
        }

        currentPlayer = Player.Player1;
        currentPhase = GamePhase.Preparation; // 이벤트 발생
        Debug.Log("Phase: Preparation - 유닛과 거점을 배치하세요.");

        // AI 덱 초기화 (테스트용)
        InitializeEnemyDeck();

        // 전투 데이터가 있으면 그것으로 초기화, 없으면 기존 테스트 로직 실행
        if (currentEncounter != null)
        {
            SetupEncounter();
        }
        else
        {
            Debug.LogWarning("설정된 Encounter가 없습니다. 테스트 모드로 실행합니다.");
            SetupReconTest();
        }

        // 여기에 초기 카드 7장 드로우 로직 추가 (HandManager 연동 필요)
        if (HandManager.Instance != null)
        {
            HandManager.Instance.DrawCards(7);
        }
    }

    private void InitializeEnemyDeck()
    {
        enemyDeck.Clear();
        enemyHand.Clear();
        // 테스트용 기본 덱 (Encounter가 없을 경우 대비)
        if (AssaultCardData != null && ScoutCardData != null && currentEncounter == null)
        {
            for (int i = 0; i < 5; i++)
            {
                enemyDeck.Add(AssaultCardData);
                enemyDeck.Add(ScoutCardData);
            }
            // 초기 핸드 1장
            for (int i=0; i<1; i++) {
                enemyHand.Add(AssaultCardData);
                enemyDeck.Add(ScoutCardData);
            }
        }
    }

    // --- 전투 인카운터 설정 ---
    private void SetupEncounter()
    {
        if (currentEncounter == null) return;
        
        Debug.Log($"전투 초기화: {currentEncounter.encounterName}");
        
        // 1. 적 덱 및 핸드 설정
        enemyDeck.Clear();
        enemyHand.Clear();

        // 덱 설정 (셔플 없음 - 순서대로)
        if (currentEncounter.enemyDeck != null)
        {
            enemyDeck.AddRange(currentEncounter.enemyDeck);
        }

        // 초기 핸드 설정
        if (currentEncounter.initialHand != null && currentEncounter.initialHand.Count > 0)
        {
            enemyHand.AddRange(currentEncounter.initialHand);
        }
        else
        {
            // 초기 핸드가 지정되지 않았다면 덱에서 3장 드로우
            for(int i=0; i<3; i++)
            {
                if(enemyDeck.Count > 0)
                {
                    enemyHand.Add(enemyDeck[0]);
                    enemyDeck.RemoveAt(0);
                }
            }
        }
        
        if (enemyDeck.Count == 0 && AssaultCardData != null && currentEncounter.enemyDeck.Count == 0)
        {
             // 덱이 아예 없으면 테스트용으로 채움
            for (int i = 0; i < 15; i++) enemyDeck.Add(AssaultCardData);
        }

        foreach (var spawnInfo in currentEncounter.enemies)
        {
            if (spawnInfo.enemyCard != null)
            {
                // 적 유닛 생성 (Player2 소유)
                // 좌표는 데이터에 정의된 대로 (적 진영 기준이라면 변환 필요할 수 있음)
                SpawnUnitInternal(spawnInfo.enemyCard, spawnInfo.position, Player.Player2, false);
            }
        }
    }

    // --- 테스트용 적 유닛 생성 ---
    private void SetupReconTest()
    {
        if (AssaultCardData == null)
        {
            Debug.LogError("테스트 실패: Boom Card Data가 할당되지 않았습니다.");
            return;
        }
        if (PlacementManager.Instance == null)
        {
            Debug.LogError("테스트 실패: PlacementManager가 씬에 없습니다.");
            return;
        }
        if (gameGrid == null)
        {
            Debug.LogError("테스트 실패: Grid가 할당되지 않았습니다.");
            return;
        }

        // TODO: 아래 좌표는 맵에 맞게 수정해야 합니다.
        Vector3Int topRowPos = new Vector3Int(-1, 9, 0); 
        Vector3Int bottomRowPos = new Vector3Int(-2, 10, 0);

        SpawnEnemyUnitForTest(AssaultCardData, topRowPos);
        SpawnEnemyUnitForTest(ScoutCardData, bottomRowPos);
    }

    private void SpawnEnemyUnitForTest(UnitCard card, Vector3Int cellLocation)
    {
        // PlacementManager의 프리팹을 사용하여 유닛 오브젝트 생성
        GameObject newUnitObject = Instantiate(PlacementManager.Instance.unitPrefab, gameGrid.GetCellCenterWorld(cellLocation), Quaternion.identity);
        UnitInstance unitInstance = newUnitObject.GetComponent<UnitInstance>();
        
        unitInstance.Initialize(card, Player.Player2);
        unitInstance.location = cellLocation;
        unitInstance.isRevealed = false; // 보이지 않게 설정
        unitInstance.isIdentified = false; // 보이지 않게 설정

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
        placementActionsTaken = 0; // 턴 시작 시 배치 행동 횟수 초기화
        DeselectUnit(); // 턴 시작 시 유닛 선택 해제

        Debug.Log("===== Player 1's Turn Starts (Settling Effects) =====");

        // 1. 턴 시작 처리 (상태 이상 갱신, 데미지 결산 등을 먼저 수행)
        // 리스트를 복사해서 순회 (사망으로 인한 레지스트리 변경 대비)
        List<UnitInstance> units = new List<UnitInstance>(unitRegistry.Values);
        foreach (var unit in units)
        {
            if (unit != null && unit.owner == currentPlayer)
            {
                unit.OnTurnStart();
            }
        }

        // 2. 잠시 대기하거나 연출 후 본격적인 카드 단계 진입
        Debug.Log("Phase: Placement - 카드를 1장 드로우하고 유닛/거점을 배치하세요.");
        HandManager.Instance.DrawCards(1);
        currentPhase = GamePhase.Placement; // 이벤트 발생
    }

    // --- 보드 상태 변화 알림 ---
    public void TriggerBoardChangeEvents()
    {
        // 모든 유닛의 OnBoardChange 호출
        List<UnitInstance> units = new List<UnitInstance>(unitRegistry.Values);
        foreach (var unit in units)
        {
            if (unit != null && unit.sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null)
            {
                baseCard.passiveSkill.OnBoardChange(unit);
            }
        }
    }

    // 플레이어 턴 종료 (배치/행동 단계가 모두 끝났을 때)
    public void EndPlayerTurn()
    {
        if (currentPlayer != Player.Player1) return;

        // 턴 종료 시 모든 특수 모드 강제 종료
        if (isTargetingSkill) ExitSkillTargetingMode();
        if (isMovingUnit) ExitMoveMode();
        
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
        Invoke("StartPlayerTurn", 1.0f);
    }

    // 참고: UI 버튼 등에서 호출할 함수들
    // 예: 플레이어가 '배치 완료' 버튼을 누르면 호출
    public void OnPlacementPhaseDone()
    {
        if (currentPhase == GamePhase.Placement && currentPlayer == Player.Player1)
        {
            if (isMovingUnit) ExitMoveMode(); // 이동 모드 강제 종료
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

        // 이미 UIManager 등에서 currentSkillToUse를 설정했다면 그것을 사용하고, 
        // 없다면 기본적으로 유닛의 ActiveSkill을 사용합니다.
        if (currentSkillToUse == null)
        {
            currentSkillToUse = selectedUnit.ActiveSkill;
        }

        if (currentSkillToUse == null) return;

        // 즉시 시전 스킬 처리 (타겟팅 불필요) - 여기서도 한 번 더 체크하여 안전하게 처리
        if (currentSkillToUse.targetType == SkillTargetType.Self || currentSkillToUse.targetType == SkillTargetType.None)
        {
            int cost = selectedUnit.GetSkillCost(currentSkillToUse);
            if (SpendEnergy(cost))
            {
                selectedUnit.hasUsedSkillThisTurn = true;
                currentSkillToUse.Execute(selectedUnit, selectedUnit.location);
                Debug.Log($"{selectedUnit.sourceCardData.cardName}의 즉시 시전 스킬({currentSkillToUse.skillName}) 발동!");
                currentSkillToUse = null; // 사용 후 초기화
            }
            return;
        }

        isTargetingSkill = true;
        skillCaster = selectedUnit; 
        
        Debug.Log($"{skillCaster.sourceCardData.cardName}의 스킬({currentSkillToUse.skillName}) 대상 지정 시작. 타겟을 클릭하세요.");
    }

    public void ExitSkillTargetingMode()
    {
        isTargetingSkill = false;
        skillCaster = null;
        currentSkillToUse = null; // 스킬 참조 해제
        
        // 스킬 모드 종료 시 붉은색 잔상(임시 타일) 제거
        if (TileEffectManager.Instance != null)
        {
            TileEffectManager.Instance.ClearTemporaryTiles();
        }
        
        Debug.Log("스킬 대상 지정 모드 종료.");
    }

    // --- 유닛 이동 시스템 ---
    public void EnterMoveMode(UnitInstance unit)
    {
        // 1. 행동 횟수가 남아있는지 확인
        if (placementActionsTaken >= placementActionsPerTurn)
        {
            Debug.Log("행동 횟수를 모두 소모하여 이동할 수 없습니다.");
            return;
        }

        // 2. 유닛이 공개된 상태인지 확인
        //if (!unit.isIdentified)
        //{
        //    Debug.Log("공개되지 않은 유닛은 이동할 수 없습니다.");
        //    return;
        //}

        // 3. 이동 모드 시작
        isMovingUnit = true;
        unitToMove = unit;
        justEnteredMoveMode = true; // 이동 모드에 방금 진입했다고 알림
        DeselectUnit(); // 이동 시작 시 기존 선택은 해제

        Debug.Log($"{unit.sourceCardData.cardName}의 이동 시작. 이동할 타일을 클릭하세요.");
    }

    public void ExitMoveMode()
    {
        isMovingUnit = false;
        unitToMove = null;
        justEnteredMoveMode = false; // 플래그 초기화
        // TileEffectManager.Instance.ClearAllEffectTiles(); // 타일 정리는 TileClickManager가 스스로 하도록 변경
        Debug.Log("유닛 이동 모드 종료.");
    }

    public void ExecuteMove(Vector3Int destination)
    {
        if (unitToMove == null) return;

        Vector3Int originalLocation = unitToMove.location;

        // 1. 유닛 레지스트리 업데이트
        DeregisterUnit(originalLocation);
        RegisterUnit(destination, unitToMove);

        // 2. 유닛 인스턴스 위치 정보 업데이트
        unitToMove.location = destination;

        // 3. 유닛 오브젝트의 실제 월드 위치 변경
        unitToMove.transform.position = gameGrid.GetCellCenterWorld(destination);
        
        // 4. [매의 눈] 상태 체크: 이동 시 위치가 발각됨
        if (unitToMove.isTracking)
        {
            unitToMove.isRevealed = true;
            Debug.Log($"[Tracking] {unitToMove.sourceCardData.cardName}이(가) 이동하여 위치가 노출되었습니다!");
        }

        // 5. 행동 횟수 소모
        placementActionsTaken++;
        
        Debug.Log($"{unitToMove.sourceCardData.cardName}이(가) {originalLocation}에서 {destination}으로 이동했습니다. 남은 행동: {placementActionsPerTurn - placementActionsTaken}");

        // 보드 상태 변경 알림 (패시브 갱신)
        TriggerBoardChangeEvents();

        // 5. 이동 모드 종료
        ExitMoveMode();
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
        if (currentPlayer == Player.Player2)
        {
            return player2Energy >= cost;
        }
        
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

    /// <summary>
    /// 지정된 플레이어의 HP를 감소시키고 게임 오버 조건을 체크합니다.
    /// 이 메서드는 플레이어의 HP에 직접적인 피해가 가해질 때 호출되어야 합니다.
    /// (예: 유닛이 플레이어의 본진에 도달했거나, 특정 스킬이 플레이어를 직접 공격할 때)
    /// </summary>
    public void ReducePlayerHealth(Player player, int amount)
    {
        if (player == Player.Player1)
        {
            player1Health -= amount;
            Debug.Log($"Player 1 Health reduced by {amount}. Current Health: {player1Health}");
        }
        else if (player == Player.Player2)
        {
            player2Health -= amount;
            Debug.Log($"Player 2 Health reduced by {amount}. Current Health: {player2Health}");
        }

        CheckGameOver(); // HP 감소 후 게임 오버 체크
    }

    /// <summary>
    /// 현재 플레이어들의 HP를 확인하여 게임 오버 조건을 판단합니다.
    /// </summary>
    private void CheckGameOver()
    {
        if (player1Health <= 0)
        {
            Debug.Log("===== Game Over! Player 2 Wins! =====");
            // 여기에 실제 게임 종료 처리 로직 추가 (예: 씬 전환, 게임 오버 UI 표시, 게임 정지 등)
            Time.timeScale = 0; // 예시: 게임 일시정지
            // UIManager.Instance.ShowGameOverScreen("Player 2 Wins!"); // 예시: UIManager를 통해 게임 오버 화면 표시
        }
        else if (player2Health <= 0)
        {
            Debug.Log("===== Game Over! Player 1 Wins! =====");
            // 여기에 실제 게임 종료 처리 로직 추가
            Time.timeScale = 0; // 예시: 게임 일시정지
            // UIManager.Instance.ShowGameOverScreen("Player 1 Wins!"); // 예시: UIManager를 통해 게임 오버 화면 표시
        }
    }

    /// <summary>
    /// 게임 단계 변경을 감지하고, 적 턴일 경우 AI를 실행합니다.
    /// </summary>
    private void HandlePhaseChange()
    {
        if (_currentPhase == GamePhase.EnemyTurn)
        {
            StartCoroutine(ExecuteEnemyTurn());
        }
    }

    /// <summary>
    /// 적 AI 턴 실행
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log("===== Enemy's Turn Starts =====");

        // 1. 턴 시작 처리
        foreach (var unit in unitRegistry.Values)
        {
            if (unit.owner == Player.Player2) unit.OnTurnStart();
        }

        // AI 에너지 회복 (매 턴 100으로 리셋)
        player2Energy = 100;
        Debug.Log("Player 2 (AI) 에너지 회복 완료 (100).");

        // 카드 드로우 시뮬레이션
        if (enemyDeck.Count > 0)
        {
            CardData drawn = enemyDeck[0];
            enemyDeck.RemoveAt(0);
            enemyHand.Add(drawn);
            Debug.Log($"[AI] 카드 드로우: {drawn.cardName}. 현재 핸드: {enemyHand.Count}");
        }

        // 2. 배치 단계 (Placement Phase)
        // AI는 핸드에 카드가 있고 자리가 있으면 70% 확률로 배치, 아니면 이동 시도
        bool placed = false;
        if (enemyHand.Count > 0 && UnityEngine.Random.value < 0.7f)
        {
            placed = AI_TryPlaceCard();
        }
        
        if (!placed)
        {
            AI_TryMoveUnit();
        }

        // 3. 행동 단계 (Action Phase) - 정찰 -> 공격
        yield return StartCoroutine(AI_ActionPhase());

        Debug.Log("===== Enemy's Turn Ends =====");
        // 적 턴 종료 처리 (페이즈가 바뀌지 않았을 때만 호출)
        if (currentPhase == GamePhase.EnemyTurn)
        {
            OnEnemyTurnEnd();
        }
    }

    private bool AI_TryPlaceCard()
    {
        if (enemyHand.Count == 0) return false;

        // 배치할 카드 선택 (랜덤)
        CardData cardToPlace = enemyHand[UnityEngine.Random.Range(0, enemyHand.Count)];
        
        // 유효한 배치 타일 찾기
        List<Vector3Int> validTiles = GetValidEnemyPlacementTiles();
        
        if (validTiles.Count > 0)
        {
            Vector3Int targetPos = validTiles[UnityEngine.Random.Range(0, validTiles.Count)];
            SpawnUnitInternal(cardToPlace, targetPos, Player.Player2, false); // 적 유닛은 처음에 숨겨짐
            enemyHand.Remove(cardToPlace);
            Debug.Log($"[AI] {cardToPlace.cardName} 배치 @ {targetPos}");
            return true;
        }

        return false;
    }

    private void AI_TryMoveUnit()
    {
        // 이동 가능한 유닛 찾기 (공개된 유닛만 이동 가능)
        List<UnitInstance> movableUnits = unitRegistry.Values
            .Where(u => u.owner == Player.Player2 && u.isIdentified) 
            .ToList();

        if (movableUnits.Count == 0) return;

        UnitInstance unitToMove = movableUnits[UnityEngine.Random.Range(0, movableUnits.Count)];
        List<Vector3Int> validMoves = GetValidMoveTiles(unitToMove);

        if (validMoves.Count > 0)
        {
            Vector3Int dest = validMoves[UnityEngine.Random.Range(0, validMoves.Count)];
            
            // 이동 처리 (직접 레지스트리 업데이트)
            DeregisterUnit(unitToMove.location);
            RegisterUnit(dest, unitToMove);
            unitToMove.location = dest;
            unitToMove.transform.position = gameGrid.GetCellCenterWorld(dest);
            
            // [매의 눈] 상태 체크: 이동 시 위치가 발각됨
            if (unitToMove.isTracking)
            {
                unitToMove.isRevealed = true;
                Debug.Log($"[AI Tracking] {unitToMove.sourceCardData.cardName}이(가) 이동하여 위치가 노출되었습니다!");
            }

            Debug.Log($"[AI] {unitToMove.sourceCardData.cardName} 이동 -> {dest}");
        }
    }

    private IEnumerator AI_ActionPhase()
    {
        List<UnitInstance> myUnits = unitRegistry.Values.Where(u => u.owner == Player.Player2 && u.currentHealth > 0).ToList();

        // 행동 우선순위:
        // 1. Scout 유닛: 적이 아직 숨겨져 있다면 정찰 우선
        // 2. Assault 유닛: 공개된 적이 있다면 공격

        // Scout 유닛들 먼저 행동
        var scouts = myUnits.Where(u => 
            u.sourceCardData is UnitCard uc && uc.unitClass == Enums.UnitClass.Scout
        ).ToList();

        foreach (var unit in scouts)
        {
            if(unit.hasUsedSkillThisTurn) continue;
            
            // 아직 보이지 않는 플레이어 유닛이 있는가?
            var hiddenEnemies = unitRegistry.Values.Where(u => u.owner == Player.Player1 && !u.isRevealed).ToList();
            
            if (hiddenEnemies.Count > 0)
            {
                // 숨겨진 적 근처를 정찰 (여기서는 간단히 해당 위치 타겟팅 - 실제 정찰 스킬은 범위를 가지므로 효과적)
                Vector3Int targetPos;
                if (UnityEngine.Random.value < 0.5f) {
                    targetPos = hiddenEnemies[UnityEngine.Random.Range(0, hiddenEnemies.Count)].location;
                    Debug.Log($"[AI Scout] {unit.sourceCardData.cardName} -> 의심스러운 위치 정찰 시도 {targetPos}");
                } else {
                    // 랜덤한 아군 타일 위치 선정
                    List<Vector3Int> allyTiles = new List<Vector3Int>();
                    if (PlacementManager.Instance != null && PlacementManager.Instance.allyTilemap != null)
                    {
                        var bounds = PlacementManager.Instance.allyTilemap.cellBounds;
                        foreach (var pos in bounds.allPositionsWithin)
                        {
                            if (PlacementManager.Instance.allyTilemap.HasTile(pos))
                            {
                                allyTiles.Add(pos);
                            }
                        }
                    }

                    if (allyTiles.Count > 0)
                    {
                        targetPos = allyTiles[UnityEngine.Random.Range(0, allyTiles.Count)];
                        Debug.Log($"[AI Scout] {unit.sourceCardData.cardName} -> 랜덤 아군 지역 정찰 시도 {targetPos}");
                    }
                    else
                    {
                        // Fallback (타일맵 정보를 가져오지 못했을 때)
                        targetPos = new Vector3Int(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(0, 4), 0);
                    }
                }

                if (unit.ActiveSkill != null && SpendEnergy(unit.GetSkillCost(unit.ActiveSkill)))
                {
                    unit.ActiveSkill.Execute(unit, targetPos);
                    unit.hasUsedSkillThisTurn = true;
                }
            }
        }

        // Assault 유닛들 행동 (그리고 남은 Scout도 공격 스킬이 있다면 사용)
        // Logistics는? 일단 공격 가능하면 공격 (보조 스킬 로직은 추후 고도화)
        var attackers = myUnits.Where(u => !u.hasUsedSkillThisTurn).ToList();

        foreach (var unit in attackers)
        {
            // 공개된 적 유닛 찾기
            var visibleEnemies = unitRegistry.Values.Where(u => u.owner == Player.Player1 && u.isRevealed && u.currentHealth > 0).ToList();

            if (visibleEnemies.Count > 0)
            {
                // 가장 가까운 적 찾기
                UnitInstance target = visibleEnemies
                    .OrderBy(e => Vector3.Distance(unit.transform.position, e.transform.position))
                    .FirstOrDefault();

                if (target != null)
                {
                     if (unit.ActiveSkill != null && SpendEnergy(unit.GetSkillCost(unit.ActiveSkill)))
                     {
                        Debug.Log($"[AI Attack] {unit.sourceCardData.cardName} -> {target.sourceCardData.cardName}");
                        unit.ActiveSkill.Execute(unit, target.location);
                        unit.hasUsedSkillThisTurn = true;
                     }
                }
            }
            else
            {
                // 공개된 적이 없다면? 
                // 필드에 적 유닛이 아예 없다면 본체 직접 공격 (규칙)
                bool anyEnemyExists = unitRegistry.Values.Any(u => u.owner == Player.Player1);
                
                if (!anyEnemyExists)
                {
                    Debug.Log($"[AI] 적 유닛이 없어 직접 공격 기회! (구현 필요)");
                }
            }
        }
        yield break;
    }

    private List<Vector3Int> GetValidEnemyPlacementTiles()
    {
        List<Vector3Int> valid = new List<Vector3Int>();
        
        if (PlacementManager.Instance != null && PlacementManager.Instance.enemyTilemap != null)
        {
            UnityEngine.Tilemaps.Tilemap enemyMap = PlacementManager.Instance.enemyTilemap;
            
            // 타일맵의 Bounds 내 모든 좌표 순회
            foreach (var pos in enemyMap.cellBounds.allPositionsWithin)
            {
                // 실제 타일이 존재하는 곳이고, 유닛이 없는 곳
                if (enemyMap.HasTile(pos) && !unitRegistry.ContainsKey(pos))
                {
                    valid.Add(pos);
                }
            }
        }
        else
        {
            // Fallback (기존 로직)
            for (int x = -5; x <= 5; x++)
            {
                for (int y = 7; y <= 11; y++) 
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (!unitRegistry.ContainsKey(pos)) 
                    {
                        valid.Add(pos); 
                    }
                }
            }
        }
        return valid;
    }

    private List<Vector3Int> GetValidMoveTiles(UnitInstance unit)
    {
        List<Vector3Int> valid = new List<Vector3Int>();
        // 간단히 상하좌우 1칸 체크
        Vector3Int[] directions = new Vector3Int[] {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0)
        };
        
        foreach (var dir in directions)
        {
            Vector3Int target = unit.location + dir;
            
            // 1. 이미 유닛이 있는 곳은 제외
            if (unitRegistry.ContainsKey(target)) continue;

            // 2. 적군 타일맵 위인지 확인 (AI는 적군 영토 내에서만 이동)
            if (PlacementManager.Instance != null && PlacementManager.Instance.enemyTilemap != null)
            {
                if (!PlacementManager.Instance.enemyTilemap.HasTile(target)) continue;
            }

            valid.Add(target);
        }
        return valid;
    }

    // 내부 유닛 생성 로직 (AI 및 테스트 공용)
    private UnitInstance SpawnUnitInternal(CardData card, Vector3Int cellLocation, Player owner, bool isVisible)
    {
        if (PlacementManager.Instance == null) return null;

        // PlacementManager의 프리팹을 사용하여 유닛 오브젝트 생성
        GameObject newUnitObject = Instantiate(PlacementManager.Instance.unitPrefab, gameGrid.GetCellCenterWorld(cellLocation), Quaternion.identity);
        UnitInstance unitInstance = newUnitObject.GetComponent<UnitInstance>();
        
        unitInstance.Initialize(card, owner); // Owner 전달
        unitInstance.location = cellLocation;
        unitInstance.isRevealed = isVisible; 
        unitInstance.isIdentified = isVisible; // 가시성 설정

        RegisterUnit(cellLocation, unitInstance); 

        // 스프라이트 설정
        SpriteRenderer sr = newUnitObject.GetComponent<SpriteRenderer>();
        Sprite spriteToUse = (card is UnitCard u) ? u.unitSprite : (card is BaseCard b ? b.unitSprite : null);
        
        if (sr != null && spriteToUse != null)
        {
            sr.sprite = spriteToUse;
        }

        return unitInstance;
    }
}
