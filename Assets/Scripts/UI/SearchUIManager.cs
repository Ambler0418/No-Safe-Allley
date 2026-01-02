using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class SearchUIManager : MonoBehaviour
{
    public static SearchUIManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject searchPanel;      // 서치 전체 패널
    public Transform contentArea;       // 카드 아이템들이 배치될 곳 (Grid Layout Group 권장)
    public GameObject cardItemPrefab;   // 서치용 간단한 카드 프리팹

    private System.Action<CardData> onCardSelected; // 선택 시 실행될 콜백

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;

        if (searchPanel != null) searchPanel.SetActive(false);
    }

    /// <summary>
    /// 서치 UI를 엽니다.
    /// </summary>
    /// <param name="cards">보여줄 카드 리스트</param>
    /// <param name="callback">카드를 선택했을 때 실행할 함수</param>
    public void OpenSearchPanel(List<CardData> cards, System.Action<CardData> callback)
    {
        if (searchPanel == null)
        {
            Debug.LogError("[SearchUI] searchPanel이 할당되지 않았습니다!");
            return;
        }

        Debug.Log($"[SearchUI] 패널 오픈 시도. 대상 카드 수: {cards.Count}");
        onCardSelected = callback;
        searchPanel.SetActive(true);

        // 기존 아이템 청소
        foreach (Transform child in contentArea)
        {
            Destroy(child.gameObject);
        }

        // 카드 아이템 생성
        int instantiatedCount = 0;
        foreach (var card in cards)
        {
            GameObject itemObj = Instantiate(cardItemPrefab, contentArea);
            var itemScript = itemObj.GetComponent<SearchCardUIItem>();
            
            if (itemScript != null)
            {
                itemScript.Setup(card, OnItemClicked);
                instantiatedCount++;
            }
            else
            {
                Debug.LogError($"[SearchUI] 생성된 아이템({itemObj.name})에서 SearchCardUIItem 스크립트를 찾을 수 없습니다! 프리팹 설정을 확인하세요.");
            }
        }
        Debug.Log($"[SearchUI] 아이템 생성 완료. 성공: {instantiatedCount} / 전체: {cards.Count}");
    }

    private void OnItemClicked(CardData selectedData)
    {
        onCardSelected?.Invoke(selectedData);
        ClosePanel();
    }

    public void ClosePanel()
    {
        if (searchPanel != null) searchPanel.SetActive(false);
    }
}
