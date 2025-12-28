using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Assign in Inspector")]
    public UICard uiCardPrefab;
    public Transform handPanelTransform;

    [Header("Deck Composition")]
    public CardData reconCard;
    public CardData barrierCard;
    public CardData auroraCard;
    public CardData energyRefillCard; // EnergyFill -> EnergyRefill로 가정
    public CardData boomCard;
    public CardData randomCard;

    private List<CardData> gameDeck = new List<CardData>();

    private List<CardData> gameDeckBackup = new List<CardData>();

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
        }
    }

    void Start()
    {
        BuildDeck();
        ShuffleDeck();
        DrawCards(5);
    }

    // 덱을 구성하는 함수
    void BuildDeck()
    {
        gameDeck.Clear();

        for(int i = 0; i < 3; i++)
        {
            gameDeck.Add(reconCard);
            gameDeck.Add(barrierCard);
            gameDeck.Add(auroraCard);
            gameDeck.Add(energyRefillCard);
            gameDeck.Add(boomCard);
        }

        gameDeckBackup = gameDeck;
        
    }

    // 덱을 섞는 함수 (Fisher-Yates Shuffle)
    void ShuffleDeck()
    {
        for (int i = 0; i < gameDeck.Count; i++)
        {
            CardData temp = gameDeck[i];
            int randomIndex = Random.Range(i, gameDeck.Count);
            gameDeck[i] = gameDeck[randomIndex];
            gameDeck[randomIndex] = temp;
        }
    }

    // 지정된 수만큼 카드를 드로우하는 함수
    public void DrawCards(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (gameDeck.Count > 0)
            {
                // 덱의 맨 위에서 카드를 한 장 뽑음
                CardData drawnCard = gameDeck[0];
                gameDeck.RemoveAt(0);

                // 카드 UI 생성 및 초기화
                UICard newCard = Instantiate(uiCardPrefab, handPanelTransform);
                newCard.Initialize(drawnCard);
            }
            else
            {
                gameDeck = gameDeckBackup;
                ShuffleDeck();
                i--; // 덱이 비었을 때 다시 시도
            }
        }
    }

}
