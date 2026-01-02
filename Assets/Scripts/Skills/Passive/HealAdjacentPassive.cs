using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Heal Adjacent Passive", menuName = "Skills/Passive/Heal Adjacent")]
public class HealAdjacentPassive : PassiveSkill
{
    public int healAmount;

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);

        // Grid 시스템을 통해 인접 타일 가져오기
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        // 육각형 그리드 기준 인접 좌표 계산 (간소화된 방식 또는 유틸리티 사용)
        // 여기서는 GameManager의 unitRegistry를 순회하며 거리를 잴 수도 있고,
        // 좌표 규칙을 사용할 수도 있습니다. 육각형 좌표계(Axial/Cube)를 가정합니다.
        
        // 간단하게 주변 6개 방향 오프셋 정의 (Odd-R 또는 Even-R 등 좌표계에 따라 다름)
        // Unity Tilemap의 경우 cell bounds를 체크하거나 인접 함수 사용.
        // 가장 확실한 방법: 모든 아군 유닛을 검사하여 거리가 1인 유닛 찾기.

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            // 나 자신은 제외, 같은 편인 경우만
            if (unit != owner && unit.owner == owner.owner)
            {
                if (IsAdjacent(owner.location, unit.location))
                {
                    unit.heal(healAmount);
                    Debug.Log($"[Passive] {owner.sourceCardData.cardName}의 효과로 {unit.sourceCardData.cardName} 체력 {healAmount} 회복.");
                }
            }
        }
    }

    private bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        // 수정: World Position 거리로 판단
        Vector3 worldPos1 = GameManager.Instance.gameGrid.GetCellCenterWorld(pos1);
        Vector3 worldPos2 = GameManager.Instance.gameGrid.GetCellCenterWorld(pos2);
        
        // 육각형 타일 크기에 따라 다르겠지만, 보통 인접 타일 중심간 거리는 일정함.
        // 여기서는 대략적인 값으로 체크.
        return Vector3.Distance(worldPos1, worldPos2) < 1.5f * GameManager.Instance.gameGrid.cellSize.x;
    }
}
