using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopCardDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI cardNameText;
    public Image artworkImage;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    private ShopItem assignedShopItem;
    private ShopManager shopManager;

    public void Initialize(ShopItem item, ShopManager manager)
    {
        this.assignedShopItem = item;
        this.shopManager = manager;

        // 카드 정보 할당
        if (cardNameText != null) cardNameText.text = item.card.cardName;
        if (artworkImage != null) artworkImage.sprite = item.card.artwork;
        
        // 가격 정보 할당
        if (priceText != null) priceText.text = $"가격: {item.price} G";

        // 구매 버튼에 리스너 할당
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => shopManager.BuyItem(assignedShopItem));
        }
    }
}
