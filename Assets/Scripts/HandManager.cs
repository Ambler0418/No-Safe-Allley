using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
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

    void Start()
    {
        BuildDeck();
        ShuffleDeck();
        DrawInitialHand(5);
    }

    // 덱을 구성하는 함수
    void BuildDeck()
    {
        gameDeck.Clear();

        // 지정된 카드 에셋들을 덱에 추가
        gameDeck.Add(reconCard);
        gameDeck.Add(barrierCard);
        gameDeck.Add(auroraCard);
        gameDeck.Add(energyRefillCard);
        gameDeck.Add(boomCard);

        for (int i = 0; i < 10; i++)
        {
            gameDeck.Add(randomCard);
        }
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
    void DrawInitialHand(int count)
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
        }
    }
}
