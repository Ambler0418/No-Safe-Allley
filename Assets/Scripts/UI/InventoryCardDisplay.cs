using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCardDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI cardNameText;
    public Image artworkImage;
    public TextMeshProUGUI quantityText; // 보유 수량을 표시할 텍스트
    public Button cardButton; // 카드 전체를 감싸는 버튼

    private CardData assignedCardData;
    private InventoryManager inventoryManager;

    // isCollectionCard: true면 컬렉션 카드, false면 덱 카드
    public void Initialize(CardData cardData, int quantity, InventoryManager manager, bool isCollectionCard)
    {
        this.assignedCardData = cardData;
        this.inventoryManager = manager;

        // UI 요소에 카드 정보 할당
        if (cardNameText != null) cardNameText.text = cardData.cardName;
        if (artworkImage != null) artworkImage.sprite = cardData.artwork;
        if (quantityText != null) quantityText.text = $"x{quantity}";
        // isCollectionCard가 false인 경우 (덱에 있는 카드) 수량 텍스트를 숨깁니다.
        // 또는 다른 방식으로 표시할 수 있습니다 (예: 덱에서는 수량을 표시하지 않음)
        if (!isCollectionCard && quantityText != null) quantityText.gameObject.SetActive(false);


        // 버튼 클릭 시의 동작 설정
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners(); // 기존 리스너 제거

            if (isCollectionCard)
            {
                // 컬렉션 카드 클릭 시: 덱에 추가
                cardButton.onClick.AddListener(() => inventoryManager.AddCardToDeck(assignedCardData));
            }
            else
            {
                // 덱 카드 클릭 시: 덱에서 제거
                cardButton.onClick.AddListener(() => inventoryManager.RemoveCardFromDeck(assignedCardData));
            }
        }
    }
}