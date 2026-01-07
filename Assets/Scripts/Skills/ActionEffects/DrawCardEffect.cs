using UnityEngine;

[CreateAssetMenu(fileName = "New Draw Card Effect", menuName = "Skills/Action Effects/Draw Card")]
public class DrawCardEffect : ActionEffect
{
    public int amount = 1;

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // HandManager를 통해 카드 드로우
        if (HandManager.Instance != null)
        {
            HandManager.Instance.DrawCards(amount);
            Debug.Log($"[DrawCard] 카드 {amount}장을 뽑았습니다.");
            // 원래는 '정부 소속 카드 서치'여야 하지만, 시스템 미비로 일반 드로우로 대체됨.
        }
        else
        {
            Debug.LogError("HandManager 인스턴스를 찾을 수 없습니다.");
            return false;
        }
        return true;
    }
}
