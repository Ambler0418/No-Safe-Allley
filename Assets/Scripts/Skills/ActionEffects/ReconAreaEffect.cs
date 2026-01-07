using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Recon Area Effect", menuName = "Skills/Action Effects/Recon Area")]
public class ReconAreaEffect : ActionEffect
{
    [Header("Deprecated (Controlled by AreaPattern)")]
    public int baseRange = 1;
    public int bonusConditionThreshold = 3;
    public int bonusRange = 1;
    public bool useFactionCheck = false;
    public Enums.Faction targetFaction = Enums.Faction.Government;

    // 이제 AreaPattern이 범위를 결정하므로, 이 효과는 '단일 타일'에 대해서만 동작해야 합니다.
    // SkillEffect가 AreaPattern에서 받은 모든 타일에 대해 이 Apply를 각각 호출해줍니다.
    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 시전자 소유자 확인
        GameManager.Player casterOwner = (caster != null) ? caster.owner : GameManager.Instance.currentPlayer;

        UnitInstance targetUnit = GameManager.Instance.GetUnitAt(targetTile);
        
        // 적 유닛이 있고, 숨겨진 상태라면
        if (targetUnit != null && targetUnit.owner != casterOwner && !targetUnit.isRevealed)
        {
            // 영구적으로 표시 (적 발견)
            TileEffectManager.Instance.HighlightReconTile(targetTile);
            targetUnit.isRevealed = true; // 유닛 발견!
            Debug.Log($"[Recon] {targetTile}에서 적 발견! ({targetUnit.sourceCardData.cardName})");
        }
        else
        {
            // 적이 없거나 이미 보이는 곳이면 잠깐 반짝임 (빈 땅 정찰 피드백)
            // 선택한 칸(Center)도 이제 정상적으로 이 로직을 탑니다.
            TileEffectManager.Instance.FlashReconTile(targetTile, 0.5f);
        }

        return true;
    }
}
