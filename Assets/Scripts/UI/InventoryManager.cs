using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform collectionContentParent;
    public Transform deckContentParent;
    public GameObject cardUIPrefab;
    public TextMeshProUGUI deckCountText;

    [Header("Deck Building Settings")]
    public int maxDeckSize = 30;

    // 현재 편집 중인 덱 (임시 덱)
    private List<CardData> currentEditingDeck = new List<CardData>();
    // 덱에 넣고 남은 카드들의 수량을 추적하는 임시 사전
    private Dictionary<CardData, int> availableCardCounts = new Dictionary<CardData, int>();

    void Start()
    {
        InitializeAvailableCards();
        RefreshUI();
    }

    // `CoreManager`에서 컬렉션과 덱 정보를 가져와 `availableCardCounts`를 초기화합니다.
    void InitializeAvailableCards()
    {
        if (CoreManager.Instance == null)
        {
            Debug.LogError("CoreManager.Instance is null!");
            return;
        }

        // 1. 전체 컬렉션을 기반으로 사용 가능한 카드 수량을 복사합니다.
        availableCardCounts = new Dictionary<CardData, int>(CoreManager.Instance.playerCardCollection);

        // 2. 저장된 덱 정보를 불러옵니다.
        currentEditingDeck = new List<CardData>(CoreManager.Instance.currentDeck);

        // 3. 이미 덱에 포함된 카드들의 수량을 `availableCardCounts`에서 빼줍니다.
        foreach (CardData cardInDeck in currentEditingDeck)
        {
            if (availableCardCounts.ContainsKey(cardInDeck))
            {
                availableCardCounts[cardInDeck]--;
            }
        }
    }

    // UI 전체를 새로고침합니다.
    void RefreshUI()
    {
        // --- 1. 컬렉션 뷰 새로고침 ---
        foreach (Transform child in collectionContentParent)
        {
            Destroy(child.gameObject);
        }

        // `CoreManager`의 전체 카드 목록을 순회하며 UI 생성
        foreach (var entry in CoreManager.Instance.playerCardCollection)
        {
            CardData card = entry.Key;
            int remainingQuantity = availableCardCounts.ContainsKey(card) ? availableCardCounts[card] : 0;

            GameObject cardUI_GO = Instantiate(cardUIPrefab, collectionContentParent);
            InventoryCardDisplay display = cardUI_GO.GetComponent<InventoryCardDisplay>();
            if (display != null)
            {
                display.Initialize(card, remainingQuantity, this, true);
            }
        }

        // --- 2. 덱 뷰 새로고침 ---
        foreach (Transform child in deckContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (CardData card in currentEditingDeck)
        {
            GameObject cardUI_GO = Instantiate(cardUIPrefab, deckContentParent);
            InventoryCardDisplay display = cardUI_GO.GetComponent<InventoryCardDisplay>();
            if (display != null)
            {
                display.Initialize(card, 1, this, false);
            }
        }

        // --- 3. 덱 카운트 텍스트 업데이트 ---
        deckCountText.text = $"{currentEditingDeck.Count} / {maxDeckSize}";
    }

    // 임시 덱에 카드를 추가합니다.
    public void AddCardToDeck(CardData cardToAdd)
    {
        // 덱이 꽉 찼는지 확인
        if (currentEditingDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning("Deck is full!");
            return;
        }

        // 추가할 수 있는 수량이 남았는지 확인
        if (availableCardCounts.ContainsKey(cardToAdd) && availableCardCounts[cardToAdd] > 0)
        {
            currentEditingDeck.Add(cardToAdd);
            availableCardCounts[cardToAdd]--; // 사용 가능 수량 1 감소
            RefreshUI(); // UI 새로고침
        }
        else
        {
            Debug.LogWarning($"No more copies of '{cardToAdd.cardName}' available to add.");
        }
    }

    // 임시 덱에서 카드를 제거합니다.
    public void RemoveCardFromDeck(CardData cardToRemove)
    {
        if (currentEditingDeck.Remove(cardToRemove))
        {
            availableCardCounts[cardToRemove]++; // 사용 가능 수량 1 증가
            RefreshUI(); // UI 새로고침
        }
    }

    public void SaveDeck()
    {
        if (currentEditingDeck.Count < 5)
        {
            Debug.LogWarning($"Deck must contain at least 5 cards. Current: {currentEditingDeck.Count}");
            return;
        }

        if (CoreManager.Instance != null)
        {
            CoreManager.Instance.currentDeck = new List<CardData>(currentEditingDeck);
            Debug.Log("Deck saved successfully!");
        }
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}