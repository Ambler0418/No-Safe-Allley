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
    public UnitCard boomCardData; // 테스트용으로 생성할 Boom 카드 데이터

    [Header("Player Stats")]
    public int player1Health = 15;
    public int player2Health = 15;
    public int player1Energy = 100;
    public int player2Energy = 100;

    [Header("Combat Tracking")]
    public int deadEnemyCount = 0; // 사망한 적 유닛 수 (I004 스킬용)

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
            // 적 유닛이 사망한 경우 카운트 증가 (플레이어가 Player1이라고 가정)
            if (deadUnit.owner == Player.Player2 && deadUnit.sourceCardData is UnitCard)
            {
                deadEnemyCount++;
                Debug.Log($"적 유닛 사망 카운트 증가: {deadEnemyCount}");
            }
    
            // 모든 유닛의 패시브 스킬에 사망 이벤트 전파        // 컬렉션 변경 오류 방지를 위해 리스트 복사 후 순회
        List<UnitInstance> units = new List<UnitInstance>(unitRegistry.Values);
        foreach (var unit in units)
        {
            if (unit != null && unit.gameObject.activeInHierarchy) // 파괴되지 않은 유닛만
            {
                // UnitInstance에 HandlePassiveUnitDeath 메서드를 추가하여 호출하는 것이 깔끔함
                // 하지만 여기서는 직접 접근 (UnitInstance 수정 최소화)
                if (unit.sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null)
                {
                    baseCard.passiveSkill.OnUnitDied(unit, deadUnit);
                }
            }
        }
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

    // --- 전투 인카운터 설정 ---
    private void SetupEncounter()
    {
        if (currentEncounter == null) return;
        
        Debug.Log($"전투 초기화: {currentEncounter.encounterName}");
        
        foreach (var spawnInfo in currentEncounter.enemies)
        {
            if (spawnInfo.enemyCard != null)
            {
                // 적 유닛 생성 (Player2 소유)
                // 좌표는 데이터에 정의된 대로 (적 진영 기준이라면 변환 필요할 수 있음)
                SpawnEnemyUnitForTest(spawnInfo.enemyCard, spawnInfo.position);
            }
        }
    }

    // --- 테스트용 적 유닛 생성 ---
    private void SetupReconTest()
    {
        if (boomCardData == null)
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
        placementActionsTaken = 0; // 턴 시작 시 배치 행동 횟수 초기화
        DeselectUnit(); // 턴 시작 시 유닛 선택 해제

        // 턴 시작 처리 (상태 이상 갱신, 패시브 발동, 행동력 초기화)
        foreach (var unit in unitRegistry.Values)
        {
            if (unit.owner == currentPlayer)
            {
                unit.OnTurnStart();
            }
        }

        Debug.Log("===== Player 1's Turn =====");
        Debug.Log("Phase: Placement - 카드를 1장 드로우하고 유닛/거점을 배치하세요.");
        HandManager.Instance.DrawCards(1);
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
        if (!unit.IsVisible)
        {
            Debug.Log("공개되지 않은 유닛은 이동할 수 없습니다.");
            return;
        }

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
        
        // 4. 행동 횟수 소모
        placementActionsTaken++;
        
        Debug.Log($"{unitToMove.sourceCardData.cardName}이(가) {originalLocation}에서 {destination}으로 이동했습니다. 남은 행동: {placementActionsPerTurn - placementActionsTaken}");

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
    /// 간단한 적 AI의 행동을 처리하는 코루틴입니다.
    /// </summary>
    private IEnumerator ExecuteEnemyTurn()
    {
        Debug.Log("===== Enemy's Turn Starts =====");

        // 적 유닛 턴 시작 처리 (상태 이상 갱신, 패시브 발동)
        foreach (var unit in unitRegistry.Values)
        {
            if (unit.owner == Player.Player2)
            {
                unit.OnTurnStart();
            }
        }

        // 1초 대기 (플레이어가 상황을 인지할 시간)
        yield return new WaitForSeconds(1.0f);

        // 행동 가능한 모든 적 유닛과 대상이 될 아군 유닛 목록을 만듭니다.
        List<UnitInstance> enemyUnits = unitRegistry.Values.Where(u => u.owner == Player.Player2 && u.currentHealth > 0).ToList();
        List<UnitInstance> playerUnits = unitRegistry.Values.Where(u => u.owner == Player.Player1 && u.currentHealth > 0).ToList();

        // 공격할 적이나 공격받을 아군이 없으면 턴을 즉시 종료합니다.
        if (enemyUnits.Count > 0 && playerUnits.Count > 0)
        {
            // 무작위로 공격자와 대상을 선택합니다.
            UnitInstance attacker = enemyUnits[UnityEngine.Random.Range(0, enemyUnits.Count)];
            UnitInstance target = playerUnits[UnityEngine.Random.Range(0, playerUnits.Count)];

            Debug.Log($"Enemy AI: {attacker.sourceCardData.cardName}이(가) {target.sourceCardData.cardName}을(를) 공격합니다!");

            // 타일 클릭과 동일하게 스킬을 실행합니다.
            UnitCard attackerCard = attacker.sourceCardData as UnitCard;
            if (attackerCard != null && attackerCard.activeSkill != null)
            {
                attackerCard.activeSkill.Execute(attacker, target.location);
            }

            // 결과 확인을 위해 1초 더 대기
            yield return new WaitForSeconds(1.0f);
        }
        else
        {
            Debug.Log("행동할 적 유닛 또는 대상이 없어 적 턴을 스킵합니다.");
        }

        Debug.Log("===== Enemy's Turn Ends =====");
        // 적 턴 종료 처리
        OnEnemyTurnEnd();
    }
}
