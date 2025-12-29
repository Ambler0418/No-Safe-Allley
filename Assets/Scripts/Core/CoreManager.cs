using UnityEngine;
using System.Collections.Generic; // Dictionary와 List를 사용하기 위해 추가!
    
public class CoreManager : MonoBehaviour
{
    public static CoreManager Instance { get; private set; }

    [Header("Player Data")]
    public int playerGold = 100; // Re-added playerGold

    [Header("Player Deck Data")]
    // 플레이어가 소유한 모든 카드의 목록과 개수를 저장합니다. (예: "병사 카드": 3개)
    public Dictionary<CardData, int> playerCardCollection = new Dictionary<CardData, int>();
 
    // 플레이어가 현재 구성한 덱 목록입니다. (최대 30장)
    public List<CardData> currentDeck = new List<CardData>();
 
    // --- 테스트용 시작 카드 데이터 (Inspector에서 설정) ---
    [Header("Test Settings")]
    [SerializeField] private List<CardData> startingCards;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 테스트를 위해 시작 카드를 컬렉션에 추가합니다.
            AddStartingCardsForTest();
        }
        else
        {
            Destroy(gameObject);
        }
    }
   // 테스트용 함수: Inspector에서 설정한 시작 카드들을 컬렉션에 추가합니다.
    private void AddStartingCardsForTest()
    {
        foreach (CardData card in startingCards)
        {
            // 이미 컬렉션에 있는 카드면 개수만 1 증가
            if (playerCardCollection.ContainsKey(card))
            {
                playerCardCollection[card]++;
            }
            // 없는 카드면 새로 추가
            else
            {
                playerCardCollection.Add(card, 1);
            }
        }
    }
}