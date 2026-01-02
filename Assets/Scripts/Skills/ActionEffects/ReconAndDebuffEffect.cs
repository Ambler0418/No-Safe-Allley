using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recon And Debuff Effect", menuName = "Skills/Action Effects/Recon And Debuff")]
public class ReconAndDebuffEffect : ActionEffect
{
    public int range = 1;
    public Enums.StatusType statusType;
    public int value; // 버프/디버프 수치 (추가)
    public int duration = 1;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return;

        // 시전자 소유자 확인
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        // 범위 탐색 (사각형 범위 순회)
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int checkPos = new Vector3Int(targetTile.x + x, targetTile.y + y, 0);
                Vector3 centerWorld = grid.GetCellCenterWorld(targetTile);
                Vector3 checkWorld = grid.GetCellCenterWorld(checkPos);

                if (Vector3.Distance(centerWorld, checkWorld) <= range * grid.cellSize.x * 1.5f)
                {
                    TileEffectManager.Instance.HighlightReconTile(checkPos);
                    
                    UnitInstance targetUnit = GameManager.Instance.GetUnitAt(checkPos);
                    if (targetUnit != null && targetUnit.owner != casterOwner)
                    {
                        // 발견!
                        targetUnit.IsVisible = true;
                        
                        // 디버프 부여 (입력받은 value 사용)
                        StatusEffect debuff = new StatusEffect(statusType, value, duration);
                        targetUnit.AddStatus(debuff);
                        Debug.Log($"[Recon] {targetUnit.sourceCardData.cardName} 발견 및 {statusType}({value}) 상태 부여.");
                    }
                }
            }
        }
    }
}
