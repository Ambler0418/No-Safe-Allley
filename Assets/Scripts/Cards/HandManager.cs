using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    [Header("Assign in Inspector")]
    public UICard uiCardPrefab;
    public Transform handPanelTransform;

    [Header("Fallback Deck")]
    [SerializeField] private List<CardData> defaultDeck = new List<CardData>();

    private List<CardData> runtimeDeck = new List<CardData>();
    private List<CardData> runtimeDeckBackup = new List<CardData>();

    [Header("Expanded Layout Settings")]
    [Range(0f, 1500f)] public float expandedArcRadius = 600f; 
    [Range(0f, 180f)] public float expandedMaxArcAngle = 90f;    
    [Range(-200f, 200f)] public float expandedHeightOffset = 50f; 
    [Range(-50f, 50f)] public float expandedFanOffset = -20f; 

    [Header("Retracted Layout Settings")]
    [Range(0f, 1500f)] public float retractedArcRadius = 800f;
    [Range(0f, 180f)] public float retractedMaxArcAngle = 40f;
    [Range(-200f, 200f)] public float retractedHeightOffset = -80f;
    [Range(-50f, 50f)] public float retractedFanOffset = -30f;

    [Header("Hover Effects")]
    [Range(1f, 1.5f)] public float hoverScaleMultiplier = 1.2f;
    [Range(0f, 100f)] public float hoverHeightBonus = 30f;

    [Header("Animation")]
    [Range(1f, 20f)] public float layoutTransitionSpeed = 8f;

    // 카드 상태 관리
    private List<UICard> cardsInHand = new List<UICard>();
    private UICard hoveredCard = null;
    private UICard draggedCard = null;
    public bool IsDragging => draggedCard != null; // 드래그 중 여부 확인용 프로퍼티
    private bool isHandExpanded = false;
    private float transitionProgress = 0f; // 0 = retracted, 1 = expanded

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    void Start()
    {
        LoadDeckFromCoreManager();

        if (runtimeDeck.Count >= 5)
        {
            ShuffleDeck();
            DrawCards(5);
        }
        else
        {
            Debug.LogError("Deck is too small! Not enough cards to start battle.");
        }
    }

    void Update()
    {
        // 목표값(0 또는 1)을 향해 부드럽게 transitionProgress 값을 변경
        float targetProgress = isHandExpanded ? 1f : 0f;
        transitionProgress = Mathf.Lerp(transitionProgress, targetProgress, Time.deltaTime * layoutTransitionSpeed);
        
        UpdateHandLayout();
    }

    // --- 카드 덱 관리 ---
    void LoadDeckFromCoreManager()
    {
        if (CoreManager.Instance != null && CoreManager.Instance.currentDeck.Count > 0)
            runtimeDeck = new List<CardData>(CoreManager.Instance.currentDeck);
        else if (defaultDeck.Count > 0)
            runtimeDeck = new List<CardData>(defaultDeck);

        if (runtimeDeck.Count > 0)
            runtimeDeckBackup = new List<CardData>(runtimeDeck);
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

                AddCardToHand(drawnCard);
            }
            else
            {
                Debug.Log("Deck is empty. Reshuffling from backup.");
                
                if (runtimeDeckBackup.Count == 0)
                {
                    Debug.LogError("Cannot reshuffle: Backup deck is also empty! Stopping draw.");
                    break;
                }

                runtimeDeck = new List<CardData>(runtimeDeckBackup);
                ShuffleDeck();
                i--;
            }
        }
        UpdateHandLayout();
    }

    /// <summary>
    /// 덱에서 특정 진영의 카드를 검색하여 UI를 통해 선택한 뒤 패로 가져옵니다.
    /// </summary>
    public void SearchCardByFaction(Enums.Faction targetFaction)
    {
        Debug.Log($"[Search] 서치 시작. 대상 진영: {targetFaction}, 현재 덱 잔량: {runtimeDeck.Count}");
        
        List<CardData> matchingCards = new List<CardData>();
        foreach (var card in runtimeDeck)
        {
            if (card.faction == targetFaction)
            {
                matchingCards.Add(card);
            }
        }

        Debug.Log($"[Search] 검색 완료. 일치하는 카드: {matchingCards.Count}장");

        if (matchingCards.Count > 0)
        {
            // 서치 UI 오픈
            SearchUIManager.Instance.OpenSearchPanel(matchingCards, (selectedCard) => {
                // 콜백: 플레이어가 선택한 카드 처리
                runtimeDeck.Remove(selectedCard);
                AddCardToHand(selectedCard);
                UpdateHandLayout();
                Debug.Log($"[Search] '{selectedCard.cardName}'을(를) 선택하여 패로 가져왔습니다.");
            });
        }
        else
        {
            Debug.LogWarning($"[Search] {targetFaction} 진영의 카드가 덱에 없습니다.");
        }
    }

    private void AddCardToHand(CardData data)
    {
        UICard newCard = Instantiate(uiCardPrefab, handPanelTransform);
        newCard.Initialize(data);
        cardsInHand.Add(newCard);
    }
    
    public void RemoveCardFromHand(UICard card)
    {
        cardsInHand.Remove(card);
    }

    // --- 외부 이벤트 수신 ---
    public void ExpandHand() => isHandExpanded = true;
    public void RetractHand() => isHandExpanded = false;
    public void OnCardHoverEnter(UICard card) => hoveredCard = card;
    public void OnCardHoverExit(UICard card) { if (hoveredCard == card) hoveredCard = null; }
    public void OnCardDragBegin(UICard card) => draggedCard = card;
    public void OnCardDragEnd(UICard card) { if (draggedCard == card) draggedCard = null; }

    /// <summary>
    /// 덱(초기 덱, 현재 덱, 손패 포함)에 특정 문자열을 이름(파일명 또는 카드명)에 포함하는 카드가 있는지 확인합니다.
    /// 예: "I004" 또는 "네크로필리아"
    /// </summary>
    public bool CheckDeckContainsCard(string searchString)
    {
        // 1. 초기 백업 덱 확인 (게임 시작 시점의 덱)
        if (runtimeDeckBackup != null && runtimeDeckBackup.Count > 0)
        {
            foreach (var card in runtimeDeckBackup)
            {
                if (card != null && (card.name.Contains(searchString) || card.cardName.Contains(searchString))) return true;
            }
            // 백업 덱이 있다면 여기서 결론이 나야 하지만, 중간에 카드가 추가되는 경우를 대비해 아래도 확인 가능
            // 하지만 백업 덱이 '초기 덱'을 의미한다면 여기서 true면 끝.
            return false;
        }

        // 2. 현재 덱 확인 (백업이 아직 안 되었거나 비어있을 경우)
        foreach (var card in runtimeDeck)
        {
            if (card != null && (card.name.Contains(searchString) || card.cardName.Contains(searchString))) return true;
        }

        // 3. 손패 확인
        foreach (var uiCard in cardsInHand)
        {
            if (uiCard.cardData != null && 
               (uiCard.cardData.name.Contains(searchString) || uiCard.cardData.cardName.Contains(searchString)))
                return true;
        }
        
        return false;
    }

    // --- 핵심 레이아웃 로직 ---
    void UpdateHandLayout()
    {
        int numCards = cardsInHand.Count;
        if (numCards == 0) return;

        float centerIndex = (numCards - 1) / 2f;

        // 현재 transitionProgress에 따라 레이아웃 값들을 보간
        float currentArcRadius = Mathf.Lerp(retractedArcRadius, expandedArcRadius, transitionProgress);
        float currentMaxArcAngle = Mathf.Lerp(retractedMaxArcAngle, expandedMaxArcAngle, transitionProgress);
        float currentHeightOffset = Mathf.Lerp(retractedHeightOffset, expandedHeightOffset, transitionProgress);
        float currentFanOffset = Mathf.Lerp(retractedFanOffset, expandedFanOffset, transitionProgress);

        for (int i = 0; i < numCards; i++)
        {
            UICard card = cardsInHand[i];
            if (card == draggedCard) continue;

            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.SetSiblingIndex(i);

            float anglePerCard = numCards > 1 ? currentMaxArcAngle / (numCards - 1) : 0;
            float currentCardAngle = (i - centerIndex) * anglePerCard;

            cardRect.localRotation = Quaternion.Euler(0, 0, -currentCardAngle);

            float xPos = Mathf.Sin(currentCardAngle * Mathf.Deg2Rad) * currentArcRadius + (i - centerIndex) * currentFanOffset;
            float yPos = (Mathf.Cos(currentCardAngle * Mathf.Deg2Rad) - 1) * currentArcRadius + currentHeightOffset;
            
            float scale = 1f;
            if (card == hoveredCard && isHandExpanded)
            {
                scale = hoverScaleMultiplier;
                yPos += hoverHeightBonus;
            }

            cardRect.localScale = Vector3.Lerp(cardRect.localScale, Vector3.one * scale, Time.deltaTime * layoutTransitionSpeed);
            cardRect.anchoredPosition = Vector2.Lerp(cardRect.anchoredPosition, new Vector2(xPos, yPos), Time.deltaTime * layoutTransitionSpeed);
        }
        
        if (hoveredCard != null && isHandExpanded)
        {
            hoveredCard.transform.SetAsLastSibling();
        }
    }
}