using UnityEngine;

// 이 카드를 Unity 메뉴에서 생성할 수 있도록 설정합니다.
[CreateAssetMenu(fileName = "NewUnitCard", menuName = "Card Data/Unit Card")]
public class UnitCard : CardData // CardData를 상속받음
{
    [Header("Unit Placement Image")]
    public Sprite unitSprite; //
    
    [Header("Unit Specific Stats")]
    public Enums.UnitClass unitClass;  // 유닛 분류 (Scout, Assault, Logistics)
    public int maxHealth;
    public int attack;
    public int defense;

    [Header("Skill")]
    // 이 카드가 사용할 스킬을 ScriptableObject 애셋으로 연결
    public SkillEffect activeSkill;
    
    // 상태 변수 (currentHealth, location, revealed 등)는 여기서 제거합니다.
    // 이 값들은 인스턴스(필드 위의 유닛)가 관리합니다.
}