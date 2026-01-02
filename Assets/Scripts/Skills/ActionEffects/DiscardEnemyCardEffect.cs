using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Discard Enemy Card Effect", menuName = "Skills/Action Effects/Discard Enemy Card")]
public class DiscardEnemyCardEffect : ActionEffect
{
    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 🌟🌟🌟 주의: 현재 HandManager는 플레이어(나)의 패만 관리합니다. 🌟🌟🌟
        // 적의 패를 관리하고 버리는 시스템은 아직 없습니다.
        // 현재는 로그만 남기고, 향후 적(AI/PVP) 핸드 시스템 구축 시 확장해야 합니다.
        
        Debug.LogWarning("[Discard] 적의 패를 확인하고 버리는 기능은 아직 시스템적으로 구현되지 않았습니다. (UI 및 적 핸드 매니저 필요)");
    }
}
