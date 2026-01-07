using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Sacrifice Counter", menuName = "Skills/Counter/Sacrifice Ritual")]
public class SacrificeCounter : CounterSkill
{
    [Header("Effect Settings")]
    public int healAmount = 300;

    public override void OnUnitDied(UnitInstance owner, UnitInstance deadUnit)
    {
        // 1. 발동 조건 체크
        // - 사망한 유닛이 아군이어야 함
        if (deadUnit.owner != owner.owner) return;
        // - 나 자신이 죽은 경우는 제외
        if (deadUnit == owner) return;

        // "임의의 아군 유닛 사망 시" (거리 제한 없음)
        // 거점(owner) 인접 아군을 회복
        Debug.Log($"[Counter] {owner.sourceCardData.cardName}: 아군 {deadUnit.sourceCardData.cardName} 사망 감지 -> 희생 의식 발동 준비.");
        HealRandomAdjacentAlly(owner);
    }

    private void HealRandomAdjacentAlly(UnitInstance owner)
    {
        var adjacentAllies = new List<UnitInstance>();

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            // 나 자신이 아니고, 아군이며, 살아있는 유닛
            if (unit != owner && unit.owner == owner.owner && unit.currentHealth > 0)
            {
                if (IsAdjacent(owner.location, unit.location))
                {
                    adjacentAllies.Add(unit);
                }
            }
        }

        if (adjacentAllies.Count > 0)
        {
            // 랜덤 1명 선택
            UnitInstance target = adjacentAllies[Random.Range(0, adjacentAllies.Count)];
            target.heal(healAmount);
            Debug.Log($"[Sacrifice] {owner.sourceCardData.cardName}의 희생 의식 발동! {target.sourceCardData.cardName}에게 {healAmount} 회복.");
        }
        else
        {
            Debug.Log($"[Sacrifice] {owner.sourceCardData.cardName} 주변에 회복할 아군이 없습니다.");
        }
    }

    // 육각형 인접 체크 (Utility로 빼면 좋으나 일단 유지)
    private bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        if (pos1 == pos2) return false;
        if (pos1.y % 2 == 0) // 짝수 행
        {
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;
            if (dy == 1) return dx == -1 || dx == 0;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 0 || dx == -1;
        }
        else // 홀수 행
        {
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;
            if (dy == 1) return dx == 0 || dx == 1;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 1 || dx == 0;
        }
        return false;
    }
}