using UnityEngine;

[CreateAssetMenu(fileName = "New Energy Refill Effect", menuName = "Skills/Action Effects/Energy Refill")]
public class EnergyRefillEffect : ActionEffect
{
    [Header("Refill Amount")]
    public int amount; // 회복할 에너지 양

    // 이 Apply 함수가 SkillEffect에 의해 실제로 호출됩니다.
    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        // 중요: 이 효과는 스킬 범위 내 '모든 타일'에 대해 한 번씩 실행됩니다.
        // 에너지를 한 번만 회복시키려면, 이 효과를 사용하는 스킬의 AreaPattern을
        // SingleTileArea로 설정하고 시전자 자신을 타겟으로 하는 것이 좋습니다.
        
        // GameManager에 에너지 회복 요청 (누가 사용했는지는 GameManager가 스스로 판단)
        GameManager.Instance.AddEnergy(amount);
        return true;
    }
}