using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SearchCardUIItem : MonoBehaviour
{
    public Image artworkImage;
    public TextMeshProUGUI nameText;
    public Button selectButton;

    private CardData cardData;
    private System.Action<CardData> onClickAction;

    public void Setup(CardData data, System.Action<CardData> onClick)
    {
        cardData = data;
        onClickAction = onClick;

        Debug.Log($"[SearchUI] Item Setup: {data.cardName}, Artwork: {(data.artwork != null ? data.artwork.name : "NULL")}");

        if (nameText != null) nameText.text = data.cardName;
        if (artworkImage != null)
        {
            if (data.artwork != null)
            {
                artworkImage.sprite = data.artwork;
                artworkImage.color = Color.white; // 혹시 투명도가 낮을 경우를 대비
            }
            else
            {
                artworkImage.color = Color.gray; // 이미지가 없으면 회색으로 표시
            }
        }
        
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onClickAction?.Invoke(cardData));
        }
    }
}
