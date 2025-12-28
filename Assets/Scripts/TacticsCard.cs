using UnityEngine;

[CreateAssetMenu(fileName = "NewTacticsCard", menuName = "Card Data/Tactics Card")]
public class TacticsCard : CardData
{
    [Header("Tactics Specific Stats")]
    public int energyCost;             // 사용 시 소모 에너지
    public TacticsEffectType effectType; // (필요하다면 Enum으로 세분화: Draw, Heal, Destroy, EnergyGain 등)
    public int effectValue;            // 효과 값 (예: 드로우 수, 치유량)
}

// 예시 Enum (TacticsCard에 추가하거나 별도의 Enums.cs에 추가)
public enum TacticsEffectType
{
    DrawCard,
    HealUnit,
    GainEnergy,
    // ... 기타 효과
}