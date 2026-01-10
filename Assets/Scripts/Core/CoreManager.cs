using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Linq 추가

public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }

    [Header("Runtime Data")]
    // 실제 게임 로직에서 사용하는 데이터 구조 (딕셔너리 등)
    public int playerGold = 100;
    public Dictionary<CardData, int> playerCardCollection = new Dictionary<CardData, int>();
    public List<CardData> currentDeck = new List<CardData>();
    
    // 캠페인 상태
    public Vector3Int lastVisitedNodeCoordinate; // 마지막으로 진입한 노드 좌표
    public bool lastBattleResult; // 마지막 전투 승리 여부
    public bool isReturningFromBattle = false; // 전투에서 복귀했는지 여부 체크
    // 클리어한 노드 좌표 목록 (중복 방지를 위해 HashSet 사용)
    public HashSet<Vector3Int> clearedNodes = new HashSet<Vector3Int>();

    // 대기 중인 보상 데이터 (맵으로 복귀 후 지급)
    public RewardData pendingReward;
    
    // 플레이어 현재 체력 (캠페인 유지용)
    public int playerCurrentHealth = 20;

    [Header("Save Data")]
    public GameSaveData saveData; // 직렬화용 데이터 컨테이너

    // 카드 데이터베이스 (저장된 이름으로 실제 ScriptableObject를 찾기 위함)
    // 실제로는 Resources.Load나 Addressables를 써야 하지만, 지금은 간단히 인스펙터나 전체 로드로 해결
    // 여기서는 "모든 카드를 리스트로 들고 있거나 Resources 폴더에서 찾는 방식"을 가정
    private Dictionary<string, CardData> cardDatabase = new Dictionary<string, CardData>();

    [Header("Test Settings")]
    [SerializeField] private List<CardData> startingCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            BuildCardDatabase(); // 카드 DB 구축
            LoadGameData();      // 게임 데이터 로드
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 모든 카드 리소스를 로드하여 이름으로 찾을 수 있게 DB 구축
    private void BuildCardDatabase()
    {
        cardDatabase.Clear();
        
        // Resources/Cards 폴더 안의 모든 CardData 로드
        CardData[] allCards = Resources.LoadAll<CardData>("Cards");
        
        foreach (CardData card in allCards)
        {
            // 카드 이름(cardName)을 키로 사용하되, 중복 방지
            if (!string.IsNullOrEmpty(card.cardName))
            {
                if (!cardDatabase.ContainsKey(card.cardName))
                {
                    cardDatabase.Add(card.cardName, card);
                }
            }
            // 에셋 파일 이름도 키로 등록 (혹시 모를 상황 대비)
            if (!cardDatabase.ContainsKey(card.name))
            {
                cardDatabase.Add(card.name, card);
            }
        }
        
        Debug.Log($"카드 데이터베이스 구축 완료: {cardDatabase.Count}개의 항목 등록됨.");
    }

    public void SaveGameData()
    {
        if (saveData == null) saveData = new GameSaveData();

        // 1. 런타임 데이터 -> SaveData로 변환
        saveData.gold = playerGold;
        saveData.playerCurrentHealth = playerCurrentHealth;
        saveData.playerMapPosition = lastVisitedNodeCoordinate; // 마지막 위치 저장

        // 컬렉션 변환
        saveData.collectedCards.Clear();
        foreach (var pair in playerCardCollection)
        {
            saveData.collectedCards.Add(new GameSaveData.CardEntry(pair.Key.cardName, pair.Value));
        }

        // 덱 변환
        saveData.currentDeckCardNames.Clear();
        foreach (var card in currentDeck)
        {
            saveData.currentDeckCardNames.Add(card.cardName);
        }

        // 클리어 노드 변환
        saveData.clearedNodeCoordinates = new List<Vector3Int>(clearedNodes);

        // 2. 파일 저장
        SaveSystem.SaveGame(saveData);
    }

    public void LoadGameData()
    {
        GameSaveData loadedData = SaveSystem.LoadGame();

        if (loadedData != null)
        {
            saveData = loadedData;

            // 1. SaveData -> 런타임 데이터로 복원
            playerGold = saveData.gold;
            playerCurrentHealth = saveData.playerCurrentHealth;
            lastVisitedNodeCoordinate = saveData.playerMapPosition;
            
            // 클리어 노드 복원
            clearedNodes = new HashSet<Vector3Int>(saveData.clearedNodeCoordinates);

            // 컬렉션 복원
            playerCardCollection.Clear();
            foreach (var entry in saveData.collectedCards)
            {
                CardData card = FindCardByName(entry.cardName);
                if (card != null)
                {
                    if (playerCardCollection.ContainsKey(card))
                    {
                        playerCardCollection[card] += entry.quantity; // 중복 시 수량 합산
                    }
                    else
                    {
                        playerCardCollection.Add(card, entry.quantity);
                    }
                }
            }

            // 덱 복원
            currentDeck.Clear();
            foreach (var cardName in saveData.currentDeckCardNames)
            {
                CardData card = FindCardByName(cardName);
                if (card != null)
                {
                    currentDeck.Add(card);
                }
            }
        }
        else
        {
            // 저장된 데이터가 없으면 새 게임 초기화
            Debug.Log("새 게임 초기화 중...");
            InitializeNewGame();
        }
    }

    private void InitializeNewGame()
    {
        playerGold = 100;
        playerCurrentHealth = 20;
        playerCardCollection.Clear();
        currentDeck.Clear();
        clearedNodes.Clear();
        
        // 초기 카드 지급
        AddStartingCardsForTest();
        
        // 초기 덱 설정 (컬렉션에 있는 카드로 덱 채우기 예시)
        // ...
        
        SaveGameData(); // 초기 상태 저장
    }

    private CardData FindCardByName(string cardName)
    {
        if (cardDatabase.ContainsKey(cardName))
        {
            return cardDatabase[cardName];
        }
        
        // DB에 없으면 Resources에서 로드 시도 (폴더 구조가 잡혀있다는 가정 하에)
        // CardData card = Resources.Load<CardData>($"Cards/{cardName}"); // 경로 예시
        // if (card != null) {
        //    cardDatabase.Add(cardName, card);
        //    return card;
        // }

        Debug.LogWarning($"카드를 찾을 수 없습니다: {cardName}. (DB 등록 필요)");
        return null;
    }

    // 테스트용 함수: Inspector에서 설정한 시작 카드들을 컬렉션에 추가합니다.
    private void AddStartingCardsForTest()
    {
        foreach (CardData card in startingCards)
        {
            // DB에도 등록 (중요)
            if(!cardDatabase.ContainsKey(card.cardName)) cardDatabase.Add(card.cardName, card);

            if (playerCardCollection.ContainsKey(card))
            {
                playerCardCollection[card]++;
            }
            else
            {
                playerCardCollection.Add(card, 1);
            }
        }
    }
}