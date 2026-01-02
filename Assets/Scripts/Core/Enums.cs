public class Enums
{
    // 카드 종류
    public enum CardType
    {
        Unit,       // 유닛
        Tactics,    // 전술
        Base        // 거점
    }

    // 유닛 클래스 분류
    public enum UnitClass
    {
        None,       // 거점이나 전술 카드를 위해
    Scout,      // 정찰병
    Assault,    // 돌격병
    Logistics   // 보급병
}

    
    // 카드 등급 (별 등급)
    public enum CardRarity
    {
        OneStar = 1,
        TwoStar = 2, // 최대 10장 제한
        ThreeStar = 3  // 최대 5장 제한
    }

    public enum Faction
    {
        Government,
        SmogBandits,
        IronFrame,
        FlameOrder
    }

    // 전술 카드 효과 종류 (향후 ActionEffect 구현 시 참고)
    public enum TacticsEffectType
    {
        DrawCard,
        HealUnit,
        GainEnergy,
        // ... 기타 효과
    }

    // 상태 이상 및 버프 종류
    public enum StatusType
    {
        None,
        AttackBuff,     // 공격력 증가
        DefenseBuff,    // 방어력 증가
        AttackDebuff,   // 공격력 감소
        DefenseDebuff,  // 방어력 감소
        Stun,           // 행동 불가
        Shield,         // 보호막 (데미지 흡수)
        ReconImmunity,  // 정찰 면역 (위치 발각 안됨)
        Tracking,       // 위치 추적 (이동해도 발각됨)
        SkillCostReduction, // 스킬 에너지 비용 감소
        DamageDealtBonus,   // 가하는 피해 증가 (퍼센트)
        DamageTakenBonus,    // 받는 피해 증가 (퍼센트)
        Provoked,           // 도발당함 (특정 대상만 공격 가능)
        Infected            // 감염됨 (I004용 마커 또는 특수효과)
    }
}