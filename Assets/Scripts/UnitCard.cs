using UnityEngine;

// 이 카드를 Unity 메뉴에서 생성할 수 있도록 설정합니다.
[CreateAssetMenu(fileName = "NewUnitCard", menuName = "Card Data/Unit Card")]
public class UnitCard : CardData // CardData를 상속받음
{
    [Header("Unit Placement Image")]
    public Sprite unitSprite; //
    
    [Header("Unit Specific Stats")]
    public Enums.UnitClass unitClass;  // 유닛 분류 (Scout, Assault, Logistics)
    public int maxHealth;              // 최대 체력 (원본)
    public int attack;                 // 공격력 (원본)
    public int defense;                // 방어력 (원본)

    [Header("Skill")]
    [TextArea]
    public string skillDescription;     // 스킬 설명
    public int skillEnergyCost;        // 스킬 사용 비용
    
    // 상태 변수 (currentHealth, location, revealed 등)는 여기서 제거합니다.
    // 이 값들은 인스턴스(필드 위의 유닛)가 관리합니다.
}