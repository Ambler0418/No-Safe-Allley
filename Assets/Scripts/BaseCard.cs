using UnityEngine;

// UnitCard와 BaseCard는 필드에 배치되므로 별도의 Sprite를 가집니다.
[CreateAssetMenu(fileName = "New Base Card", menuName = "Card Data/Base Card")]
public class BaseCard : CardData // CardData를 상속받음
{
    [Header("Base Specific Stats")]
    public int maxHealth;
    public int energyCost;
    
    // 🌟🌟🌟 추가: 필드 위에 배치될 거점의 이미지 🌟🌟🌟
    [Header("Base Placement Image")]
    public Sprite unitSprite; 
}