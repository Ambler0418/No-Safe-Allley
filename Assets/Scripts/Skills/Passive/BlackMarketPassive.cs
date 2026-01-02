using UnityEngine;

[CreateAssetMenu(fileName = "New Black Market Passive", menuName = "Skills/Passive/Black Market")]
public class BlackMarketPassive : PassiveSkill
{
    public int energyCost = 1;
    public int drawAmount = 1;

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);

        // 에너지 소모 시도
        if (GameManager.Instance.SpendEnergy(energyCost))
        {
            if (HandManager.Instance != null)
            {
                HandManager.Instance.DrawCards(drawAmount);
                Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 에너지 {energyCost} 소모하여 카드 {drawAmount}장 드로우.");
            }
        }
        else
        {
            Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 에너지가 부족하여 효과 발동 실패.");
        }
    }
}
