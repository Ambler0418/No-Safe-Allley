using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New Reveal Random Enemy Effect", menuName = "Skills/Action Effects/Reveal Random Enemy")]
public class RevealRandomEnemyEffect : ActionEffect
{
    public int revealCount = 2;
    public bool resetDeadCount = true;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        GameManager.Player opponent = (caster != null && caster.owner == GameManager.Player.Player1) 
                                      ? GameManager.Player.Player2 
                                      : GameManager.Player.Player1;

        // 숨겨진 적 목록 가져오기
        List<UnitInstance> hiddenEnemies = GameManager.Instance.unitRegistry.Values
            .Where(u => u.owner == opponent && !u.IsVisible)
            .ToList();

        if (hiddenEnemies.Count == 0)
        {
            Debug.Log("[Reveal] 정찰할 수 있는 숨겨진 적이 없습니다.");
            return;
        }

        // 랜덤 셔플 및 정찰
        int actualRevealCount = Mathf.Min(revealCount, hiddenEnemies.Count);
        for (int i = 0; i < actualRevealCount; i++)
        {
            int randomIndex = Random.Range(i, hiddenEnemies.Count);
            UnitInstance target = hiddenEnemies[randomIndex];
            hiddenEnemies[randomIndex] = hiddenEnemies[i];
            
            target.IsVisible = true;
            TileEffectManager.Instance.HighlightReconTile(target.location);
            Debug.Log($"[Reveal] 무작위 적 발견: {target.sourceCardData.cardName} at {target.location}");
        }

        if (resetDeadCount)
        {
            GameManager.Instance.ResetDeadEnemyCount();
        }
    }
}
