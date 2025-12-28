using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 드래그 관련 3가지 인터페이스 구현
public class UICard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

    // 1. 드래그 시작 시
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 원래 위치와 부모 저장
        originalPosition = rectTransform.localPosition;
        originalParent = transform.parent;

        // 2. 부모를 최상위 캔버스로 변경
        //    - HandManager 같은 LayoutGroup의 제어에서 벗어나 자유롭게 움직이게 하기 위함
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        
        // 3. 드래그 중에는 다른 UI와 상호작용하지 않도록 설정
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    // 2. 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        // 가장 간단하고 직접적인 방법으로 위치를 설정합니다.
        // 카드의 피벗(Pivot)이 마우스 커서를 따라갑니다.
        rectTransform.position = eventData.position;
        
        // 만약 이 방법으로도 중심이 맞지 않는다면, 피벗 자체의 문제입니다.
        // 그 경우, Unity 에디터에서 UICard 프리팹의 RectTransform > Pivot 값을 (0.5, 0.5)로 설정해야 합니다.
    }

    // 3. 드래그 종료 시
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true; // Raycast 다시 활성화

        // 드래그가 끝난 스크린 좌표를 월드 좌표로 변환
        // Camera.main이 씬의 메인 카메라를 참조한다고 가정
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        bool placementSuccess = false;

        // 2단계에서 구현한 PlacementManager 호출
        if (PlacementManager.Instance != null)
        {
            placementSuccess = PlacementManager.Instance.TryPlaceCard(cardData, mouseWorldPosition);
        }
        else
        {
            // 아직 PlacementManager를 씬에 생성하지 않았다면 경고
            Debug.LogError("PlacementManager.Instance가 씬에 없습니다. 2단계를 진행해주세요.");
        }

        if (placementSuccess)
        {
            // 배치(유닛) 또는 사용(전술)에 성공했다면 카드 파괴
            Destroy(gameObject); 
        }
        else
        {
            // 실패했다면 원래 부모와 위치로 복귀
            transform.SetParent(originalParent);
            rectTransform.localPosition = originalPosition;
        }
    }
}