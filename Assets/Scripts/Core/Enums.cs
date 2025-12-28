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

    // 전술 카드 효과 종류 (향후 ActionEffect 구현 시 참고)
    public enum TacticsEffectType
    {
        DrawCard,
        HealUnit,
        GainEnergy,
        // ... 기타 효과
    }
}