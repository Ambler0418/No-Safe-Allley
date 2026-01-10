using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData
{
    // --- 플레이어 기본 정보 ---
    public int gold;
    public int playerCurrentHealth; // 플레이어의 현재 체력 (캠페인 유지용)

    // --- 인벤토리 & 덱 (카드 이름으로 저장) ---
    // 카드 데이터(ScriptableObject) 자체는 저장할 수 없으므로, 카드의 이름(ID)을 저장합니다.
    // 딕셔너리는 JSON 직렬화가 까다로우므로, 리스트 2개로 나누어 저장하거나 별도 클래스를 씁니다.
    // 여기서는 간단하게 "CardName:Quantity" 형태의 리스트나 래퍼 클래스를 사용합니다.
    
    [System.Serializable]
    public class CardEntry
    {
        public string cardName;
        public int quantity;

        public CardEntry(string name, int qty)
        {
            cardName = name;
            quantity = qty;
        }
    }

    public List<CardEntry> collectedCards = new List<CardEntry>(); // 보유 카드 목록
    public List<string> currentDeckCardNames = new List<string>(); // 덱에 포함된 카드 이름들

    // --- 캠페인 진행 상황 ---
    public string currentMapName; // 현재 플레이 중인 맵 이름 (나중에 여러 맵이 생길 경우)
    public Vector3Int playerMapPosition; // 캠페인 맵에서의 플레이어 위치
    public List<Vector3Int> clearedNodeCoordinates = new List<Vector3Int>(); // 클리어한 노드들의 좌표 목록

    // 생성자 (기본값 설정)
    public GameSaveData()
    {
        gold = 100;
        playerCurrentHealth = 20; // 기본 체력
        collectedCards = new List<CardEntry>();
        currentDeckCardNames = new List<string>();
        clearedNodeCoordinates = new List<Vector3Int>();
        // 기본 위치는 맵 데이터에 따라 달라지므로 여기서는 0,0,0 등 임시값
        playerMapPosition = new Vector3Int(0, -2, 0); 
    }
}
