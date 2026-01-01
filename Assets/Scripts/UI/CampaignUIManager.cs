using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용
using System.Collections.Generic;
using Map; // WorldMapManager 접근을 위해

namespace UI
{
    public class CampaignUIManager : MonoBehaviour
    {
        [Header("Buttons")]
        public Button backButton; // 메인 메뉴로 돌아가는 버튼

        [Header("Map Selection")]
        public TMP_Dropdown mapSelector; // 맵 선택 드롭다운
        public List<CampaignMapData> availableMaps; // 선택 가능한 맵 데이터 목록

        private void Start()
        {
            // 버튼 이벤트 연결
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackButtonClicked);
            }

            // 맵 선택 드롭다운 초기화
            InitializeMapSelector();
        }

        private void InitializeMapSelector()
        {
            if (mapSelector == null || availableMaps == null || availableMaps.Count == 0) return;

            mapSelector.ClearOptions();
            List<string> options = new List<string>();

            foreach (var map in availableMaps)
            {
                // 맵 데이터에 이름이 없으면 파일 이름이나 기본값 사용
                string mapName = string.IsNullOrEmpty(map.mapName) ? map.name : map.mapName;
                options.Add(mapName);
            }

            mapSelector.AddOptions(options);
            
            // --- 시작 맵 설정 로직 추가 ---
            if (WorldMapManager.Instance != null && WorldMapManager.Instance.mapData != null)
            {
                int startIndex = availableMaps.IndexOf(WorldMapManager.Instance.mapData);
                if (startIndex != -1)
                {
                    mapSelector.value = startIndex;
                    mapSelector.RefreshShownValue();
                }
            }
            // ----------------------------

            // 현재 선택된 맵 변경 시 이벤트 연결
            mapSelector.onValueChanged.AddListener(OnMapSelected);
        }

        private void OnMapSelected(int index)
        {
            if (index >= 0 && index < availableMaps.Count)
            {
                CampaignMapData selectedMap = availableMaps[index];
                if (WorldMapManager.Instance != null)
                {
                    WorldMapManager.Instance.LoadMap(selectedMap);
                }
            }
        }

        private void OnBackButtonClicked()
        {
            // WorldMapManager의 함수 호출
            if (WorldMapManager.Instance != null)
            {
                WorldMapManager.Instance.ReturnToMainMenu();
            }
            else
            {
                Debug.LogError("WorldMapManager가 씬에 없습니다!");
            }
        }
    }
}
