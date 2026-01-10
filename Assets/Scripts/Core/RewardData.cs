using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Reward", menuName = "Game Data/Reward Data")]
public class RewardData : ScriptableObject
{
    [Header("Immediate Rewards")]
    public int goldReward = 50;
    public int healthReward = 0; // 플레이어 체력 회복량

    [Header("Card Choices")]
    // 여러 번의 선택 기회 (각 기회마다 지정된 카드들 중 하나 선택)
    public List<CardChoiceGroup> cardChoices = new List<CardChoiceGroup>();

    [System.Serializable]
    public class CardChoiceGroup
    {
        [Tooltip("이 그룹에 있는 카드들 중 하나를 선택할 수 있습니다. 1개면 확정 지급.")]
        public List<CardData> options = new List<CardData>();
    }
}
