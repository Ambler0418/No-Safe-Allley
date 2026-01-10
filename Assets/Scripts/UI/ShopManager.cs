using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

// ShopManager.cs 파일 안에 함께 정의합니다.
[System.Serializable]
public class ShopItem
{
    public CardData card; // 판매할 카드
    public int price;     // 판매 가격
}

public class ShopManager : MonoBehaviour
{
    [Header("Shop Settings")]
    public List<ShopItem> itemsForSale; // Inspector에서 판매 목록을 설정합니다.

    [Header("UI References")]
    public Transform shopContentParent;
    public GameObject shopCardUIPrefab;
    public TextMeshProUGUI playerGoldText;

    void Start()
    {
        UpdateGoldDisplay();
        PopulateShop();
    }

    // 플레이어의 골드 UI를 업데이트합니다.
    void UpdateGoldDisplay()
    {
        if (CoreManager.Instance != null)
        {
            playerGoldText.text = $"골드: {CoreManager.Instance.playerGold}";
        }
    }

    // 판매 목록에 있는 아이템들을 UI로 생성합니다.
    void PopulateShop()
    {
        foreach (Transform child in shopContentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (ShopItem item in itemsForSale)
        {
            GameObject cardUI_GO = Instantiate(shopCardUIPrefab, shopContentParent);
            ShopCardDisplay display = cardUI_GO.GetComponent<ShopCardDisplay>();
            if (display != null)
            {
                display.Initialize(item, this);
            }
        }
    }

    // 카드 구매 로직
    public void BuyItem(ShopItem itemToBuy)
    {
        if (CoreManager.Instance == null)
        {
            Debug.LogError("CoreManager is missing!");
            return;
        }

        // 1. 골드가 충분한지 확인
        if (CoreManager.Instance.playerGold >= itemToBuy.price)
        {
            // 2. 골드 차감
            CoreManager.Instance.playerGold -= itemToBuy.price;

            // 3. 카드 컬렉션에 추가
            if (CoreManager.Instance.playerCardCollection.ContainsKey(itemToBuy.card))
            {
                CoreManager.Instance.playerCardCollection[itemToBuy.card]++; // 이미 있으면 수량 증가
            }
            else
            {
                CoreManager.Instance.playerCardCollection.Add(itemToBuy.card, 1); // 없으면 새로 추가
            }

            Debug.Log($"{itemToBuy.card.cardName} 구매 성공!");

            // 4. 골드 UI 업데이트 및 데이터 저장
            UpdateGoldDisplay();
            CoreManager.Instance.SaveGameData(); // 파일에 즉시 저장
            
            // TODO: 구매 성공 시 사운드, 시각 효과 등 피드백 추가
        }
        else
        {
            Debug.LogWarning("골드가 부족합니다!");
            // TODO: 골드 부족 시 피드백 추가
        }
    }

    // 메인 메뉴로 돌아갑니다.
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
