using UnityEngine;

[CreateAssetMenu(fileName = "New Add Placement Action Effect", menuName = "Skills/Action Effects/Add Placement Action")]
public class AddPlacementActionEffect : ActionEffect
{
    public int amount = 1;

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // GameManager의 배치 행동 횟수 조정
        if (GameManager.Instance.currentPhase == GameManager.GamePhase.Placement)
        {
            GameManager.Instance.placementActionsTaken -= amount;
            if (GameManager.Instance.placementActionsTaken < 0) GameManager.Instance.placementActionsTaken = 0;
            
            Debug.Log($"[Tactics] 배치 행동 횟수가 {amount}회 추가되었습니다.");
            return true;
        }
        else
        {
            Debug.LogWarning("배치 단계가 아니므로 배치 행동을 추가할 수 없습니다.");
            // 에너지를 환불해주는 로직이 필요할 수도 있음.
            return false;
        }
    }
}
