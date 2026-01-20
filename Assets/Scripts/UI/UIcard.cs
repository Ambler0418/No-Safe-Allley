using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 드래그 및 호버 관련 인터페이스 구현
public class UICard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    // 카드 데이터 (UnitCard, TacticsCard, BaseCard 모두 처리 가능)
    public CardData cardData; 
    
    // UI 컴포넌트 참조
    private Image cardImage;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition; // 드래그 시작 시 원래 위치 저장
    private Transform originalParent; // 드래그 시작 시 원래 부모 오브젝트 저장

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardImage = GetComponent<Image>();
        
        if (canvasGroup == null)
        {
            Debug.LogError("UICard requires a CanvasGroup component on the same GameObject!");
        }
    }

    // 외부에서 호출되어 카드를 초기화하는 함수
    public void Initialize(CardData data)
    {
        cardData = data;
        
        // 1. 카드 이미지 설정
        if (cardImage != null && data.artwork != null)
        {
            cardImage.sprite = data.artwork;
        }
        
        // 2. 기타 시각적 요소 초기화 (필요하다면)
    }

    // --- 드래그 이벤트 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        HandManager.Instance.OnCardDragBegin(this); // 핸드 매니저에게 드래그 시작을 알림
        HandManager.Instance.OnCardHoverExit(this); // 호버링 효과 즉시 해제
        
        originalPosition = rectTransform.localPosition;
        originalParent = transform.parent;
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
        rectTransform.localRotation = Quaternion.identity; // 드래그 중에는 각도를 0으로 설정

        // --- 배치 미리보기 (Ghost Unit) 로직 ---
        if (PlacementManager.Instance != null)
        {
            // [중요] 2D에서 ScreenToWorldPoint 사용 시 z값을 카메라와의 거리로 설정해야 함
            Vector3 screenPos = Input.mousePosition;
            screenPos.z = -Camera.main.transform.position.z; // 카메라가 -10에 있다면 10으로 설정
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(screenPos);
            
            // Debug.Log($"[Drag Debug] MouseWorld: {mouseWorldPosition}");

            bool isPreviewShowing = PlacementManager.Instance.UpdatePlacementPreview(cardData, mouseWorldPosition);
            
            // 미리보기가 보이면 카드 UI는 숨김 (투명하게), 아니면 보임
            canvasGroup.alpha = isPreviewShowing ? 0f : 1f;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        HandManager.Instance.OnCardDragEnd(this); // 핸드 매니저에게 드래그 종료를 알림
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f; // 드래그 종료 시 투명도 복구

        // 미리보기 종료
        if (PlacementManager.Instance != null)
        {
            PlacementManager.Instance.HidePreview();
        }

        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        bool placementSuccess = false;
        if (PlacementManager.Instance != null)
        {
            placementSuccess = PlacementManager.Instance.TryPlaceCard(cardData, mouseWorldPosition);
        }
        else
        {
            Debug.LogError("PlacementManager.Instance가 씬에 없습니다.");
        }

        if (placementSuccess)
        {
            // HandManager의 논리 리스트에서 자신을 제거
            HandManager.Instance.RemoveCardFromHand(this);
            Destroy(gameObject); 
        }
        else
        {
            transform.SetParent(originalParent);
            rectTransform.localPosition = originalPosition;
        }
    }

    // --- 호버 이벤트 ---
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 드래그 중이 아닐 때만 호버 효과 적용
        if (!eventData.dragging)
        {
            HandManager.Instance.OnCardHoverEnter(this);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HandManager.Instance.OnCardHoverExit(this);
    }
}