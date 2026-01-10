using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;

namespace Map
{
    public class MapRewardManager : MonoBehaviour
    {
        public static MapRewardManager Instance { get; private set; }

        [Header("UI References")]
        public GameObject rewardPanel; // 보상 팝업 전체 패널
        public Transform cardContainer; // 카드들이 생성될 부모 Transform
        public GameObject rewardCardPrefab; // UICard 프리팹
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI goldText;
        public Button confirmButton;

        [Header("State")]
        private RewardData currentRewardData;
        private int currentChoiceIndex = 0; // 현재 진행 중인 선택 단계
        private CardData selectedCard;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (rewardPanel != null) rewardPanel.SetActive(false);
            if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        /// <summary>
        /// CoreManager에 대기 중인 보상이 있다면 UI를 표시합니다.
        /// </summary>
        public void CheckAndShowReward()
        {
            if (CoreManager.Instance == null || CoreManager.Instance.pendingReward == null)
            {
                Debug.Log("지급할 대기 보상이 없습니다.");
                return;
            }

            currentRewardData = CoreManager.Instance.pendingReward;
            
            Debug.Log("대기 중인 보상이 있어 UI를 표시합니다.");
            ProcessImmediateRewards();
            StartCardSelection();
        }

        private void ProcessImmediateRewards()
        {
            // 골드 지급
            if (currentRewardData.goldReward > 0)
            {
                CoreManager.Instance.playerGold += currentRewardData.goldReward;
                Debug.Log($"{currentRewardData.goldReward} 골드 지급됨.");
            }

            // 체력 회복
            if (currentRewardData.healthReward > 0)
            {
                CoreManager.Instance.playerCurrentHealth += currentRewardData.healthReward;
                Debug.Log($"플레이어 체력 {currentRewardData.healthReward} 회복.");
            }
        }

        private void StartCardSelection()
        {
            currentChoiceIndex = 0;
            rewardPanel.SetActive(true);
            ShowNextChoice();
        }

        private void ShowNextChoice()
        {
            // 모든 선택이 끝났는지 확인
            if (currentChoiceIndex >= currentRewardData.cardChoices.Count)
            {
                FinishRewardProcess();
                return;
            }

            // UI 초기화
            selectedCard = null;
            if (confirmButton != null) confirmButton.interactable = false;
            
            if (titleText != null) titleText.text = "보상 선택";
            if (goldText != null) 
            {
                goldText.text = $"선택 {currentChoiceIndex + 1} / {currentRewardData.cardChoices.Count}";
            }

            // 현재 단계의 선택지 가져오기
            var currentChoiceGroup = currentRewardData.cardChoices[currentChoiceIndex];
            GenerateCardOptions(currentChoiceGroup.options);
        }

        private void GenerateCardOptions(List<CardData> options)
        {
            foreach (Transform child in cardContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (CardData card in options)
            {
                if (card == null) continue;

                GameObject cardObj = Instantiate(rewardCardPrefab, cardContainer);
                UICard uiCard = cardObj.GetComponent<UICard>();
                if (uiCard != null) uiCard.Initialize(card);

                Button btn = cardObj.GetComponent<Button>();
                if (btn == null) btn = cardObj.AddComponent<Button>();
                
                CardData capturedCard = card;
                btn.onClick.AddListener(() => OnCardSelected(capturedCard, cardObj));
            }
        }

        private void OnCardSelected(CardData card, GameObject selectedObj)
        {
            selectedCard = card;
            foreach (Transform child in cardContainer)
            {
                child.localScale = Vector3.one; 
            }
            selectedObj.transform.localScale = Vector3.one * 1.1f;
            if (confirmButton != null) confirmButton.interactable = true;
        }

        private void OnConfirmButtonClicked()
        {
            if (selectedCard != null)
            {
                // 카드 지급
                if (CoreManager.Instance.playerCardCollection.ContainsKey(selectedCard))
                    CoreManager.Instance.playerCardCollection[selectedCard]++;
                else
                    CoreManager.Instance.playerCardCollection.Add(selectedCard, 1);
                
                Debug.Log($"보상 카드 선택 완료: {selectedCard.cardName}");
            }

            // 다음 선택으로 이동
            currentChoiceIndex++;
            ShowNextChoice();
        }

        private void FinishRewardProcess()
        {
            CoreManager.Instance.pendingReward = null; 
            CoreManager.Instance.SaveGameData();
            rewardPanel.SetActive(false);
            Debug.Log("모든 보상 수령 완료 및 저장됨.");
        }
    }
}