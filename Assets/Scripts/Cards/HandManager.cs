using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Assign in Inspector")]
    public UICard uiCardPrefab;
    public Transform handPanelTransform;

    [Header("Fallback Deck")]
    // 플레이어가 저장한 덱이 없을 때 사용할 기본 덱
    [SerializeField] private List<CardData> defaultDeck = new List<CardData>();

    private List<CardData> runtimeDeck = new List<CardData>();
    private List<CardData> runtimeDeckBackup = new List<CardData>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        LoadDeckFromCoreManager();

        // LoadDeckFromCoreManager 호출 후, runtimeDeck에 5장 이상의 카드가 있는지 확인
        if (runtimeDeck.Count >= 5) // <-- 이 부분을 수정합니다.
        {
            ShuffleDeck();
            DrawCards(5);
        }
        else
        {
            Debug.LogError("Deck is too small! No player deck saved with at least 5 cards, and no default deck configured with at least 5 cards. Cannot start battle."); // <-- 에러 메시지 수정
        }
    }

    void LoadDeckFromCoreManager()
    {
        // CoreManager가 있고, 저장된 덱에 카드가 1장 이상 있다면 그 덱을 사용
        if (CoreManager.Instance != null && CoreManager.Instance.currentDeck.Count > 0)
        {
            Debug.Log("Loading deck from CoreManager.");
            runtimeDeck = new List<CardData>(CoreManager.Instance.currentDeck);
        }
        // 그렇지 않다면, Inspector에 설정된 기본 덱을 사용
        else if (defaultDeck.Count > 0)
        {
            Debug.Log("CoreManager deck not found or empty. Using default deck.");
            runtimeDeck = new List<CardData>(defaultDeck);
        }

        // 백업 덱도 함께 생성
        if (runtimeDeck.Count > 0)
        {
            runtimeDeckBackup = new List<CardData>(runtimeDeck);
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < runtimeDeck.Count; i++)
        {
            CardData temp = runtimeDeck[i];
            int randomIndex = Random.Range(i, runtimeDeck.Count);
            runtimeDeck[i] = runtimeDeck[randomIndex];
            runtimeDeck[randomIndex] = temp;
        }
    }

    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (runtimeDeck.Count > 0)
            {
                CardData drawnCard = runtimeDeck[0];
                runtimeDeck.RemoveAt(0);

                UICard newCard = Instantiate(uiCardPrefab, handPanelTransform);
                newCard.Initialize(drawnCard);
            }
            else
            {
                Debug.Log("Deck is empty. Reshuffling from backup.");
                runtimeDeck = new List<CardData>(runtimeDeckBackup);
                ShuffleDeck();
                i--;
            }
        }
    }
}
