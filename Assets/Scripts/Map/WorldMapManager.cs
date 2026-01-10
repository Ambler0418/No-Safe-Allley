using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;

namespace Map
{
    public class WorldMapManager : MonoBehaviour
    {
        public static WorldMapManager Instance { get; private set; }

        [Header("Settings")]
        public Grid mapGrid; // 씬에 존재하는 Grid 컴포넌트
        public CampaignMapData mapData;
        
        [Header("Prefabs & Visuals")]
        public GameObject nodePrefab; // MapNodeVisual이 붙어있는 프리팹
        public GameObject playerTokenPrefab; // 플레이어(트럭) 마커
        public Camera mapCamera; // 플레이어를 따라다닐 카메라
        
        // 노드 타입별 아이콘
        public Sprite battleIcon;
        public Sprite shopIcon;
        public Sprite eventIcon;
        public Sprite bossIcon;
        public Sprite emptyIcon;

        [Header("Runtime State")]
        public Vector3Int playerPosition;
        
        private Dictionary<Vector3Int, MapNodeVisual> spawnedNodes = new Dictionary<Vector3Int, MapNodeVisual>();
        private GameObject playerToken;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (mapCamera == null) mapCamera = Camera.main;

            if (mapData != null)
            {
                // 전투에서 복귀했고 승리했다면, 해당 노드를 클리어 목록에 추가
                if (CoreManager.Instance != null && CoreManager.Instance.isReturningFromBattle)
                {
                    if (CoreManager.Instance.lastBattleResult)
                    {
                        CoreManager.Instance.clearedNodes.Add(CoreManager.Instance.lastVisitedNodeCoordinate);
                        Debug.Log($"전투 승리! 노드 {CoreManager.Instance.lastVisitedNodeCoordinate} 클리어 처리됨.");

                        // 보상 UI 호출
                        if (MapRewardManager.Instance != null)
                        {
                            MapRewardManager.Instance.CheckAndShowReward();
                        }
                    }
                    else
                    {
                         Debug.Log("전투 패배... 노드 상태 유지.");
                         // 패배 시 보상 없음
                         CoreManager.Instance.pendingReward = null; 
                    }
                    CoreManager.Instance.isReturningFromBattle = false; // 처리 완료
                }

                GenerateMap();

            }
        }

        /// <summary>
        /// 맵 데이터를 기반으로 노드들을 생성합니다.
        /// </summary>
        public void GenerateMap()
        {
            // 기존 노드 정리
            foreach (var node in spawnedNodes.Values)
            {
                if (node != null) Destroy(node.gameObject);
            }
            spawnedNodes.Clear();

            // 기존 플레이어 토큰이 있다면 위치만 리셋하거나, 새로 생성
            // 여기서는 깔끔하게 재생성 로직을 따름 (플레이어 데이터 유지 필요 시 수정)
            
            // 노드 생성
            foreach (var nodeDef in mapData.nodes)
            {
                Vector3 worldPos = mapGrid.GetCellCenterWorld(nodeDef.coordinate);
                GameObject nodeObj = Instantiate(nodePrefab, worldPos, Quaternion.identity, transform);
                
                MapNodeVisual visual = nodeObj.GetComponent<MapNodeVisual>();
                if (visual != null)
                {
                    Sprite icon = GetIconForType(nodeDef.type);
                    visual.Initialize(nodeDef, icon);
                    spawnedNodes[nodeDef.coordinate] = visual;
                }
            }

            // 플레이어 초기화
            if (playerToken == null) // 플레이어가 아직 없을 때만 생성
            {
                Vector3Int spawnPos = mapData.startPosition;
                
                // 저장된 위치가 있다면 그곳을 사용
                if (CoreManager.Instance != null && CoreManager.Instance.lastVisitedNodeCoordinate != Vector3Int.zero)
                {
                    spawnPos = CoreManager.Instance.lastVisitedNodeCoordinate;
                }
                
                SpawnPlayerToken(spawnPos);
            }
        }

        /// <summary>
        /// 새로운 맵 데이터를 로드하고 맵을 재생성합니다.
        /// </summary>
        public void LoadMap(CampaignMapData newMapData)
        {
            if (newMapData == null) return;

            Debug.Log($"맵 변경: {newMapData.mapName}");
            mapData = newMapData;
            GenerateMap();
        }

        private void SpawnPlayerToken(Vector3Int startPos)
        {
            playerPosition = startPos;
            Vector3 worldPos = mapGrid.GetCellCenterWorld(startPos);
            worldPos.z = -1f; // 플레이어가 노드보다 위에 보이도록 Z축 조정

            if (playerToken == null && playerTokenPrefab != null)
            {
                playerToken = Instantiate(playerTokenPrefab, worldPos, Quaternion.identity);
            }
            else if (playerToken != null)
            {
                playerToken.transform.position = worldPos;
            }

            CenterCameraOnPlayer();
            UpdateFogOfWar();
        }

        /// <summary>
        /// 카메라를 플레이어 위치로 즉시 이동시킵니다.
        /// </summary>
        private void CenterCameraOnPlayer()
        {
            if (mapCamera == null) return;
            
            Vector3 targetPos = mapGrid.GetCellCenterWorld(playerPosition);
            targetPos.z = mapCamera.transform.position.z; // 카메라의 기존 Z축(높이) 유지
            mapCamera.transform.position = targetPos;
        }

        /// <summary>
        /// 노드 타입에 맞는 아이콘을 반환합니다.
        /// </summary>
        private Sprite GetIconForType(NodeType type)
        {
            switch (type)
            {
                case NodeType.Battle: return battleIcon;
                case NodeType.Shop: return shopIcon;
                case NodeType.Event: return eventIcon;
                case NodeType.Boss: return bossIcon;
                case NodeType.Empty: return emptyIcon;
                default: return battleIcon;
            }
        }

        /// <summary>
        /// 플레이어가 특정 노드로 이동을 시도합니다. (MapNodeVisual에서 호출)
        /// </summary>
        public void TryMoveToNode(MapNodeVisual targetNode)
        {
            Vector3Int targetCoord = targetNode.nodeData.coordinate;

            // 1. 현재 위치인지 확인
            if (targetCoord == playerPosition) return;

            // 2. 인접한 노드인지 확인
            if (IsNeighbor(playerPosition, targetCoord))
            {
                MovePlayer(targetCoord);
            }
            else
            {
                Debug.Log("너무 멉니다! 인접한 칸으로만 이동할 수 있습니다.");
            }
        }

        /// <summary>
        /// 플레이어 이동 처리 및 이벤트 발생
        /// </summary>
        private void MovePlayer(Vector3Int newPos)
        {
            playerPosition = newPos;
            
            // 시각적 이동 (트윈 애니메이션 등을 추가할 수 있음)
            if (playerToken != null)
            {
                Vector3 targetPos = mapGrid.GetCellCenterWorld(newPos);
                targetPos.z = -1f; // 플레이어 Z축 유지
                playerToken.transform.position = targetPos;
            }

            CenterCameraOnPlayer();
            // 안개 업데이트
            UpdateFogOfWar();

            // 노드 도착 이벤트 처리
            HandleNodeArrival(spawnedNodes[newPos].nodeData);
        }

        /// <summary>
        /// 전장의 안개 업데이트: 현재 위치와 인접한 곳만 보여줍니다.
        /// </summary>
        private void UpdateFogOfWar()
        {
            // 모든 노드를 먼저 숨김 처리 (또는 어둡게)
            foreach (var kvp in spawnedNodes)
            {
                kvp.Value.UpdateVisibility(false, false, false);
            }

            // 현재 위치 보여주기
            if (spawnedNodes.ContainsKey(playerPosition))
            {
                spawnedNodes[playerPosition].UpdateVisibility(true, true, true);
            }

            // 인접 노드 보여주기
            List<Vector3Int> neighbors = GetNeighbors(playerPosition);
            foreach (var neighbor in neighbors)
            {
                if (spawnedNodes.ContainsKey(neighbor))
                {
                    spawnedNodes[neighbor].UpdateVisibility(true, false, false);
                }
            }
        }

        /// <summary>
        /// 노드 도착 시 로직 (씬 전환 등)
        /// </summary>
        private void HandleNodeArrival(MapNodeDefinition nodeData)
        {
            Debug.Log($"도착: {nodeData.type} 노드");

            // 이미 클리어한 노드인지 확인 (Battle, Boss, Event만 해당)
            if (CoreManager.Instance != null && CoreManager.Instance.clearedNodes.Contains(nodeData.coordinate))
            {
                if (nodeData.type == NodeType.Battle || nodeData.type == NodeType.Boss || nodeData.type == NodeType.Event)
                {
                    Debug.Log("이미 클리어한 노드이므로 이벤트를 실행하지 않습니다.");
                    return; 
                }
            }

            if (nodeData.type == NodeType.Battle || nodeData.type == NodeType.Boss)
            {
                if (nodeData.battleEncounter != null)
                {
                    Debug.Log($"전투 시작! Encounter: {nodeData.battleEncounter.encounterName}");
                    
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.currentEncounter = nodeData.battleEncounter;
                        GameManager.Instance.currentReward = nodeData.nodeReward; // (삭제 예정 - CoreManager 사용 시)
                    }
                    
                    // CoreManager에 현재 노드 좌표 및 보상 정보 저장
                    if (CoreManager.Instance != null)
                    {
                        CoreManager.Instance.lastVisitedNodeCoordinate = nodeData.coordinate;
                        CoreManager.Instance.isReturningFromBattle = false; 
                        CoreManager.Instance.pendingReward = nodeData.nodeReward; // 보상 등록
                        CoreManager.Instance.SaveGameData(); 
                    }

                    SceneManager.LoadScene("Battle"); 
                }
                else
                {
                    Debug.LogError("전투 노드인데 BattleEncounter 데이터가 없습니다!");
                }
            }
            else if (nodeData.type == NodeType.Shop)
            {
                // Shop 씬 또는 UI 오픈
                // 상점은 클리어 처리하지 않음 (반복 이용 가능)
            }
            else if (nodeData.type == NodeType.Event)
            {
                if (nodeData.dialogueEvent != null)
                {
                    Debug.Log($"대화 이벤트 시작: {nodeData.dialogueEvent.eventTitle}");
                    if (DialogueManager.Instance != null)
                    {
                        DialogueManager.Instance.StartDialogue(nodeData.dialogueEvent, () => 
                        {
                            Debug.Log("대화 이벤트가 종료되었습니다.");
                            
                            // 대화 성공적으로 종료 시 클리어 처리 및 저장
                            if (CoreManager.Instance != null)
                            {
                                CoreManager.Instance.clearedNodes.Add(nodeData.coordinate);
                                CoreManager.Instance.SaveGameData();
                                Debug.Log($"이벤트 노드 {nodeData.coordinate} 클리어 처리됨.");
                                
                                // (선택) 맵 갱신이 필요하다면 GenerateMap() 호출
                                // GenerateMap(); 
                            }
                        });
                    }
                    else
                    {
                        Debug.LogError("DialogueManager가 씬에 없습니다!");
                    }
                }
                else
                {
                    Debug.LogWarning("Event 노드이지만 연결된 DialogueEventData가 없습니다.");
                }
            }
            else if (nodeData.type == NodeType.Empty)
            {
                // 아무 일도 일어나지 않음 (클리어 처리 안 함)
                Debug.Log("빈 노드(또는 시작 지점)에 도착했습니다.");
            }
        }

        /// <summary>
        /// 메인 메뉴로 돌아갑니다.
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("메인 메뉴로 돌아갑니다.");
            // 여기서 필요한 저장 로직 수행 (SaveCampaignState();)
            SceneManager.LoadScene("MainMenu");
        }

        // --- Hex Grid Helper Methods ---

        /// <summary>
        /// 두 육각형 좌표가 인접해 있는지 확인 (거리 1)
        /// Unity의 Hex Grid 좌표계(Offset) 기준에 따라 다를 수 있으나,
        /// 보통 Cube 좌표계로 변환하여 거리를 계산하거나 Axial 거리 공식을 사용합니다.
        /// 여기서는 간단히 거리가 1인 경우를 찾습니다.
        /// </summary>
        private bool IsNeighbor(Vector3Int a, Vector3Int b)
        {
            return GetHexDistance(a, b) == 1;
        }

        /// <summary>
        /// 육각형 그리드에서의 거리 계산 (Offset Coordinates -> Cube Coordinates 변환 사용)
        /// Unity CellLayout이 Hexagon일 때 기준 (Odd-R or Even-R 등 Grid 설정 확인 필요)
        /// 기본적으로 Unity는 홀수 행(Odd Row) 또는 짝수 행(Even Row) 방식을 씁니다.
        /// </summary>
        private int GetHexDistance(Vector3Int a, Vector3Int b)
        {
            // Unity Grid 컴포넌트의 설정에 따라 다를 수 있습니다.
            // 가장 확실한 방법은 Grid 컴포넌트의 좌표계를 따르는 것이지만, 
            // 여기서는 일반적인 Offset -> Cube 변환 공식을 사용합니다.
            // (Unity 기본 Hex Top-Pointy 기준, Odd-Row라고 가정)
            
            // 만약 Grid가 없으면 거리 계산이 부정확할 수 있으므로, 단순 인접성 체크로 대체 가능
            
            // 간단 버전: 좌표 차이의 절댓값 합과 관련이 있음.
            // 정확한 구현을 위해 Cube 좌표로 변환
            Vector3Int ac = OffsetToCube(a);
            Vector3Int bc = OffsetToCube(b);

            return (Mathf.Abs(ac.x - bc.x) + Mathf.Abs(ac.y - bc.y) + Mathf.Abs(ac.z - bc.z)) / 2;
        }

        // Unity 기본 Hexagon (Pointy Top)에서의 Offset to Cube (Odd-Row 가정)
        // 만약 Flat Top이거나 Even-Row라면 수정 필요.
        // 프로젝트의 Grid 설정을 확인해야 하나, 일단 Unity Default(Odd-Row)로 작성.
        private Vector3Int OffsetToCube(Vector3Int hex)
        {
            var q = hex.x - (hex.y - (hex.y & 1)) / 2;
            var r = hex.y;
            return new Vector3Int(q, r, -q - r);
        }

        private List<Vector3Int> GetNeighbors(Vector3Int center)
        {
            List<Vector3Int> neighbors = new List<Vector3Int>();
            
            // 육각형 인접 6방향 체크
            // (구체적인 오프셋은 Grid 설정에 따라 다름, 가장 확실한 건 거리 1인 모든 좌표 검색)
            // 여기서는 단순하게 주변 좌표를 훑어서 거리 1인 것을 찾습니다.
            // 범위: x(-1~1), y(-1~1) 
            
            // Unity Tilemap 좌표계 특성상 인접 좌표 패턴이 정해져 있습니다.
            // Y가 짝수일 때와 홀수일 때 X 오프셋이 다릅니다 (Pointy Top 기준).
            
            int y = center.y;
            bool isOddRow = (y % 2 != 0); // 홀수 행 여부 (음수 처리 주의)
            if (y < 0) isOddRow = (y % 2 != 0); // C# 나머지 연산자는 음수 보존

            // Pointy Top Hex Neighbors Offsets
            Vector2Int[] offsets;
            
            if (isOddRow)
            {
                offsets = new Vector2Int[] {
                    new Vector2Int(0, 1), new Vector2Int(1, 1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(0, -1), new Vector2Int(1, -1)
                };
            }
            else // Even Row
            {
                offsets = new Vector2Int[] {
                    new Vector2Int(-1, 1), new Vector2Int(0, 1),
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(-1, -1), new Vector2Int(0, -1)
                };
            }

            foreach (var offset in offsets)
            {
                neighbors.Add(center + (Vector3Int)offset);
            }

            return neighbors;
        }
    }
}
