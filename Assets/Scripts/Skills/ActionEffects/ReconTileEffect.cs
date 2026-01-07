using UnityEngine;      

[CreateAssetMenu(fileName = "New Recon Tile Effect", menuName = "Skills/Action Effects/Recon Tile")]
public class ReconTileEffect : ActionEffect
{
    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 1. 해당 타일의 유닛 확인
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 2. 시전자 소유자 확인 (전술 카드인 경우 현재 플레이어)
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        // 3. 정찰 로직 수행
        // 해당 타일에 적 유닛이 있고, 그 유닛이 현재 보이지 않는 상태라면
        if (targetUnit != null && targetUnit.owner != casterOwner && !targetUnit.isRevealed)
        {
            // 모습을 보이게 하고 타일을 영구 강조표시합니다.
            // (TileEffectManager.HighlightReconTile 내부에서 permanentlyHighlightedTiles에 추가됨)
            TileEffectManager.Instance.HighlightReconTile(targetTile);
            
            targetUnit.isRevealed = true; // 위치 공개
            // targetUnit.isIdentified = true; // 규칙상 공격받아야 정보가 공개되므로 여기서는 위치만(isRevealed) 공개
            
            Debug.Log($"정찰 발견: {targetTile} 위치에서 숨어있던 적({targetUnit.sourceCardData.cardName}) 발견!");
        }
        else
        {
            // 적이 없거나, 아군이거나, 이미 보이는 유닛일 경우 -> 잠깐 반짝임 (정찰 피드백)
            TileEffectManager.Instance.FlashReconTile(targetTile, 0.5f);
        }

        return true;
    }
}