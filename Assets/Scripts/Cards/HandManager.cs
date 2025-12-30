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

                UICard newCard = Instantiate(uiCardPrefab, handPanelTransform);
                newCard.Initialize(drawnCard);
                cardsInHand.Add(newCard);
            }
            else
            {
                Debug.Log("Deck is empty. Reshuffling from backup.");
                runtimeDeck = new List<CardData>(runtimeDeckBackup);
                ShuffleDeck();
                i--;
            }
        }
        UpdateHandLayout();
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