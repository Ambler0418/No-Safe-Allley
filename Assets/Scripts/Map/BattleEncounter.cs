using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    /// <summary>
    /// 전투 스테이지 하나의 구성을 저장하는 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEncounter", menuName = "Map/Battle Encounter")]
    public class BattleEncounter : ScriptableObject
    {
        public string encounterName;
        [TextArea] public string description; // 전투 설명 (선택 사항)
        
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
        
        [Header("Enemy Deck")]
        public List<CardData> initialHand = new List<CardData>(); // 초기 핸드 지정
        public List<CardData> enemyDeck = new List<CardData>();

        // 추가 가능: 보상 정보, 배경 이미지 등
    }
}
