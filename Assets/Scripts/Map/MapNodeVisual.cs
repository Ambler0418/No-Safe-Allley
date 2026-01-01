using UnityEngine;

namespace Map
{
    public class MapNodeVisual : MonoBehaviour
    {
        [Header("References")]
        public SpriteRenderer iconRenderer;
        public SpriteRenderer fogRenderer; // 안개를 표현할 스프라이트 (없으면 alpha 조절)

        [Header("Data")]
        public MapNodeDefinition nodeData;

        private bool _isUnlocked = false; // 이동 가능한 인접 노드인가?

        /// <summary>
        /// 초기화 함수
        /// </summary>
        public void Initialize(MapNodeDefinition data, Sprite icon)
        {
            nodeData = data;
            if (iconRenderer != null)
            {
                iconRenderer.sprite = icon;
            }
            // 이름 등 추가 설정
            this.name = $"Node_{data.coordinate}_{data.type}";
        }

        /// <summary>
        /// 시야 상태 업데이트 (전장의 안개)
        /// </summary>
        /// <param name="isVisible">현재 위치거나 인접해서 보이는가?</param>
        /// <param name="isVisited">이미 방문한 곳인가?</param>
        /// <param name="isCurrent">현재 플레이어가 위치한 곳인가?</param>
        public void UpdateVisibility(bool isVisible, bool isVisited, bool isCurrent)
        {
            _isUnlocked = isVisible; // 보이는 곳이면 이동 가능하다고 가정 (추가 로직 가능)

            if (isVisible)
            {
                // 보임: 안개 걷힘
                if (fogRenderer != null) fogRenderer.gameObject.SetActive(false);
                iconRenderer.color = Color.white; // 원래 색

                if (isCurrent)
                {
                    // 현재 위치 표시 (예: 하이라이트)
                    iconRenderer.color = Color.yellow; 
                }
            }
            else
            {
                // 안 보임: 안개에 가려짐
                if (fogRenderer != null) 
                {
                    fogRenderer.gameObject.SetActive(true);
                }
                else 
                {
                    // 안개 스프라이트가 없으면 투명도/색상으로 처리
                    iconRenderer.color = new Color(0.2f, 0.2f, 0.2f, 1f); // 어둡게 처리
                }
            }
        }

        /// <summary>
        /// 노드 클릭 시 이동 시도
        /// </summary>
        private void OnMouseDown()
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; // UI 클릭 중이면 무시
            }

            if (_isUnlocked)
            {
                // 매니저에게 이동 요청
                WorldMapManager.Instance.TryMoveToNode(this);
            }
            else
            {
                Debug.Log("이동할 수 없는 지역입니다. (안개 속이거나 연결되지 않음)");
            }
        }
    }
}
