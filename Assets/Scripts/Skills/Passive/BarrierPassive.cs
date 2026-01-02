using UnityEngine;

[CreateAssetMenu(fileName = "New Barrier Passive", menuName = "Skills/Passive/Barrier")]
public class BarrierPassive : PassiveSkill
{
    // 이 패시브는 UnitInstance.TakeDamage에서 직접 체크하여 로직을 수행하므로
    // 여기에는 별도 로직이 없어도 됩니다. 
    // 마커 역할 (HasPassive<BarrierPassive>())
}
