using UnityEngine;
using System.Collections.Generic;

namespace Map
{
    /// <summary>
    /// 전체 캠페인 맵의 배치를 저장하는 스크립터블 오브젝트입니다.
    /// 개발자가 에디터에서 미리 생성해둔 맵 데이터를 로드할 때 사용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCampaignMap", menuName = "Map/Campaign Map Data")]
    public class CampaignMapData : ScriptableObject
    {
        public string mapName;
        
        [Tooltip("플레이어의 시작 지점 좌표")]
        public Vector3Int startPosition;

        [Tooltip("맵에 존재하는 모든 노드 리스트")]
        public List<MapNodeDefinition> nodes = new List<MapNodeDefinition>();
    }
}
