using UnityEngine;

public abstract class CardData : ScriptableObject
{
    // 모든 카드가 가지는 공통 속성
    [Header("Basic Card Info")]
    public string cardName;               // 카드 이름
    public Enums.CardType cardType;       // 카드 종류 (유닛, 전술, 거점)
    public Enums.Faction faction;         // 카드 소속 (진영)
    [TextArea]
    public string description;            // 카드 설명
    public Sprite artwork;                // 카드 이미지
}