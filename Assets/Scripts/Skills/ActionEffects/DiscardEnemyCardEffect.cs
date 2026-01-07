using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DiscardEnemyCardEffect", menuName = "Skills/Action Effects/Discard Enemy Card")]
public class DiscardEnemyCardEffect : ActionEffect
{
    public override bool Apply(UnitInstance caster, Vector3Int targetLocation)
    {
        // 1. 적의 패(enemyHand) 데이터 확인
        // 현재 GameManager에서 enemyHand를 관리하고 있습니다.
        List<CardData> enemyHand = GameManager.Instance.enemyHand;

        if (enemyHand == null || enemyHand.Count == 0)
        {
            Debug.LogWarning("[Discard] 적의 패가 비어있어 버릴 카드가 없습니다.");
            return false;
        }

        // 2. SearchUIManager를 사용하여 적의 패를 보여주고 1장 선택하게 함
        if (SearchUIManager.Instance != null)
        {
            Debug.Log($"[Discard] 적의 패({enemyHand.Count}장)를 확인합니다.");
            
            // 패널을 열어 적의 핸드 카드를 보여주고, 클릭 시 해당 카드를 버리는 콜백 등록
            SearchUIManager.Instance.OpenSearchPanel(enemyHand, (selectedCard) => {
                if (GameManager.Instance.enemyHand.Contains(selectedCard))
                {
                    GameManager.Instance.enemyHand.Remove(selectedCard);
                    Debug.Log($"[Discard] 적의 카드 '{selectedCard.cardName}'을(를) 버렸습니다. 남은 패: {GameManager.Instance.enemyHand.Count}");
                }
            });
            
            return true;
        }
        else
        {
            Debug.LogError("[Discard] SearchUIManager 인스턴스를 찾을 수 없습니다. UI 연동을 확인하세요.");
            return false;
        }
    }
}