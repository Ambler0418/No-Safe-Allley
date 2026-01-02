using UnityEngine;      

[CreateAssetMenu(fileName = "New Recon Tile Effect", menuName = "Skills/Action Effects/Recon Tile")]
public class ReconTileEffect : ActionEffect
{
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 시전자 소유자 확인 (전술 카드인 경우 현재 플레이어)
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        // 해당 타일에 적 유닛이 있고, 그 유닛이 현재 보이지 않는 상태라면
        if (targetUnit != null && targetUnit.owner != casterOwner && !targetUnit.IsVisible)
        {
            // 모습을 보이게 하고 타일을 강조표시합니다.
            TileEffectManager.Instance.HighlightReconTile(targetTile);
            Debug.Log($"정찰 성공! {targetTile} 위치에서 숨어있던 적 유닛({targetUnit.sourceCardData.cardName})을 발견했습니다.");
        }
        else
        {
            // 타겟 타일에 유닛이 없거나, 아군이거나, 이미 보이는 유닛일 경우
            Debug.Log($"정찰 실패: {targetTile} 위치에 숨어있는 적 유닛이 없습니다.");
        }
    }
}