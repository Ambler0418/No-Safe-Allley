using UnityEngine;

/// <summary>
/// 특정 게임 이벤트(사망, 피격 등)에 반응하여 발동하는 스킬의 기본 클래스입니다.
/// PassiveSkill을 상속받아 BaseCard에 장착 가능합니다.
/// </summary>
public abstract class CounterSkill : PassiveSkill
{
    [Header("Counter Settings")]
    public bool triggerOnDeath = false;
    public bool triggerOnDamageTaken = false;
    // 추후 triggerOnAttack, triggerOnHeal 등 확장 가능

    // 자식 클래스에서 구체적인 로직 구현
    // 기본적으로 PassiveSkill의 이벤트 메서드들을 오버라이드하여 사용합니다.
}
