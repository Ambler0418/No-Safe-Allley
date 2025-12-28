using UnityEngine;      
[CreateAssetMenu(fileName = "New Reveal Tile Effect", menuName = "Skills/Action Effects/Reveal Tile")]
public class RevealTileEffect : ActionEffect
{
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        if (targetUnit != null && targetUnit.owner != caster.owner && !targetUnit.IsVisible)
        {
            TileEffectManager.Instance.HighlightReconTile(targetTile);
            Debug.Log($"{targetTile}의 숨은 적 발견!");
        }
    }
}