using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    /// <summary>
    /// 월드맵 노드의 종류를 정의합니다.
    /// </summary>
    public enum NodeType
    {
        Battle,     // 일반 전투
        Shop,       // 상점 (카드/아이템 구매)
        Event,      // 무작위 이벤트 (텍스트 어드벤처 등)
        Boss,       // 보스 전투
        Empty       // 아무 일도 없는 노드 (시작 지점 등)
    }

    /// <summary>
    /// 맵 상의 개별 노드에 대한 정의입니다.
    /// 에디터에서 설정하기 위한 데이터 구조입니다.
    /// </summary>
    [System.Serializable]
    public class MapNodeDefinition
    {
        [Tooltip("육각형 그리드 좌표 (Axial/Offset 등 프로젝트 기준에 따름)")]
        public Vector3Int coordinate;
        
        [Tooltip("노드의 타입")]
        public NodeType type;
        
        [Tooltip("노드 이름 (UI 표시용)")]
        public string nodeName;

        [Tooltip("전투 난이도나 상점 티어 등을 설정 (0 = 기본)")]
        public int tier;
        
        [Tooltip("이 노드가 Battle/Boss 타입일 경우 사용될 전투 데이터")]
        public BattleEncounter battleEncounter;

        [Tooltip("이 노드가 Event 타입일 경우 사용될 대화 이벤트 데이터")]
        public DialogueEventData dialogueEvent;

        [Tooltip("이 노드 완료 시 지급될 보상 데이터")]
        public RewardData nodeReward;
    }

    /// <summary>
    /// 단일 전투에서 적이 어디에 배치될지 정의합니다.
    /// </summary>
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public Vector3Int position; // 적 배치 좌표 (상대 진영 기준)
        public UnitCard enemyCard;  // 등장할 적 유닛 카드 데이터
    }
}
