using UnityEngine;

[CreateAssetMenu(fileName = "New Adjacent Scaling Damage Effect", menuName = "Skills/Action Effects/Adjacent Scaling Damage")]
public class AdjacentScalingDamageEffect : DealDamageEffect
{
    [Header("Scaling")]
    public float bonusPerAdjacentAlly; // 인접 아군 1명당 추가 계수 (예: 0.15)
    public Enums.Faction targetFaction = Enums.Faction.Government;
    public bool useFactionCheck = false;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        Debug.Log($"[AdjacentScalingDamage] Apply 시작. Caster: {caster.sourceCardData.cardName}, TargetTile: {targetTile}");

        // 전술 카드로 데미지를 주려고 할 때 caster가 null인 경우 처리
        if (caster == null)
        {
            Debug.LogError("[AdjacentScalingDamageEffect] 시전자(caster)가 없습니다.");
            return;
        }

        // 1. 인접 아군 수 계산
        int adjacentAllies = 0;
        Grid grid = GameManager.Instance.gameGrid;
        
        if (grid == null)
        {
            Debug.LogError("[AdjacentScalingDamageEffect] GameManager의 gameGrid가 Null입니다! 거리 계산을 할 수 없습니다.");
        }
        else
        {
            foreach (var unit in GameManager.Instance.unitRegistry.Values)
            {
                if (unit != caster && unit.owner == caster.owner)
                {
                    // 진영 체크
                    if (useFactionCheck && unit.Faction != targetFaction) continue;

                    // 거리 1.5 이하를 인접으로 간주
                    Vector3 worldPos1 = grid.GetCellCenterWorld(caster.location);
                    Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                    if (Vector3.Distance(worldPos1, worldPos2) < 1.5f * grid.cellSize.x)
                    {
                        adjacentAllies++;
                    }
                }
            }
        }

        // 2. 계수 보정
        float originalCoefficient = attackCoefficient;
        attackCoefficient += (adjacentAllies * bonusPerAdjacentAlly);
        
        Debug.Log($"[Scaling Damage] 인접 아군 {adjacentAllies}명 발견. 최종 계수: {attackCoefficient}. base.Apply 호출합니다.");

        // 3. 부모 클래스의 Apply 호출 (실제 데미지 적용 및 로그 출력)
        base.Apply(caster, targetTile);

        // 4. 계수 원상 복구
        attackCoefficient = originalCoefficient;
    }
}
