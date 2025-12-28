using UnityEngine;
using UnityEngine.Tilemaps;

public class PlacementManager : MonoBehaviour
{
    // Inspector에 할당
    public Grid gameGrid;             // 필드 전체를 관리하는 단일 Grid
    public GameObject unitPrefab;      // UnitInstance 스크립트가 붙은 유닛 프리팹
    public Tilemap allyTilemap;       // 아군 영역 타일맵 (배치 가능 영역 확인용)

    public static PlacementManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // UICard에서 호출될 핵심 함수
    // PlacementManager.cs의 TryPlaceCard 함수 (일부)

public bool TryPlaceCard(CardData card, Vector3 worldPosition)
{
    // --- 추가된 규칙 적용 ---
    var gm = GameManager.Instance;

    // 1. 현재 단계가 배치 가능한 단계인지 확인 (유닛/거점 카드에만 해당)
    if (card.cardType == Enums.CardType.Unit || card.cardType == Enums.CardType.Base)
    {
        if (gm.currentPhase != GameManager.GamePhase.Preparation && gm.currentPhase != GameManager.GamePhase.Placement)
        {
            Debug.LogWarning($"배치 시도 실패: 현재 단계({gm.currentPhase})에서는 유닛/거점을 배치할 수 없습니다.");
            return false;
        }

        // 2. '배치 단계'이고, 이미 카드를 배치했다면 추가 배치 차단
        if (gm.currentPhase == GameManager.GamePhase.Placement && gm.hasPlacedCardThisTurn)
        {
            Debug.Log("배치 실패: 이번 턴의 배치 단계에서는 이미 유닛/거점을 배치했습니다.");
            return false;
        }
    }
    // --- 규칙 적용 끝 ---

    Vector3Int cellLocation = gameGrid.WorldToCell(worldPosition);
    cellLocation.z = 0;

    if (card.cardType == Enums.CardType.Unit || card.cardType == Enums.CardType.Base)
    {
        // 1. 유닛/거점 카드의 배치 유효성 검사
        if (!IsPlacementValid(cellLocation))
        {
            Debug.Log($"배치 실패: {card.cardName} 카드를 유효하지 않은 위치에 놓았습니다.");
            return false; // 핸드로 복귀
        }
        
        // 2. 유닛 생성 로직
        SpawnUnit(card, cellLocation);

        // --- 추가: 배치 성공 시 플래그 설정 ---
        // '배치 단계'에서만 플래그를 설정하여, '준비 단계'에서는 여러 번 배치 가능하도록 함
        if (gm.currentPhase == GameManager.GamePhase.Placement)
        {
            gm.hasPlacedCardThisTurn = true;
            Debug.Log("배치 단계에서 카드 배치 완료. 플래그를 설정합니다.");
        }
        // --- 설정 끝 ---

        return true; // 카드 파괴
    }
    else if (card.cardType == Enums.CardType.Tactics)
    {
        TacticsCard tacticsCard = card as TacticsCard;
        if (tacticsCard == null || tacticsCard.tacticSkill == null)
        {
            Debug.LogWarning($"사용하려는 전술 카드({card.cardName})에 스킬이 연결되지 않았습니다.");
            return false;
        }

        SkillEffect skillToUse = tacticsCard.tacticSkill;

        // 1. 에너지 확인 및 소모
        if (GameManager.Instance.SpendEnergy(skillToUse.energyCost))
        {
            Debug.Log($"[Tactics Card] '{card.cardName}' 사용. 에너지 {skillToUse.energyCost} 소모.");

            // 2. 스킬 효과 발동
            // 전술 카드는 필드 위 유닛이 시전하는 것이 아니므로 caster는 null,
            // 타겟이 없는 스킬이므로 primaryTarget은 임의의 값(zero)을 넘겨줍니다.
            skillToUse.Execute(null, Vector3Int.zero);

            return true; // 사용 성공 -> 카드 파괴
        }
        else
        {
            Debug.Log($"에너지가 부족하여 '{card.cardName}' 카드를 사용할 수 없습니다.");
            return false; // 사용 실패 -> 카드를 손으로 돌려보냄
        }
    }

    return false;
}

    private bool IsPlacementValid(Vector3Int cell)
    {
        // 1. 타일맵 영역 확인: 아군 영역 타일 위에 드롭되었는지 확인
        if (!allyTilemap.HasTile(cell))
        {
            Debug.Log("배치 실패: 아군 타일 위에 놓아야 합니다.");
            return false;
        }

        // 2. 이미 유닛/거점이 있는지 확인 (추가 로직 필요)

        return true;
    }

    // PlacementManager.cs의 SpawnUnit 함수 전문
private void SpawnUnit(CardData card, Vector3Int cellLocation)
{
    Sprite unitSpriteToUse = null;
    string cardName = card.cardName;

    // 1. 카드를 UnitCard 또는 BaseCard로 캐스팅하여 정보 추출
    UnitCard unitData = card as UnitCard;
    BaseCard baseData = card as BaseCard;

    if (unitData != null)
    {
        // UnitCard 처리 (Recon, Boom 등)
        unitSpriteToUse = unitData.unitSprite;
    }
    else if (baseData != null)
    {
        // 🌟🌟🌟 BaseCard 처리 (Barrier 등) 🌟🌟🌟
        // BaseCard가 UnitCard를 상속하지 않더라도, 
        // PlacementManager는 BaseCard가 필요한 필드를 가지고 있다고 가정하고 처리합니다.
        unitSpriteToUse = baseData.unitSprite; // BaseCard에 unitSprite 필드가 있어야 함 (이전에 추가함)
    }
    else
    {
        // UnitCard도 BaseCard도 아닐 경우 (오류 방지)
        Debug.LogError($"[SpawnUnit] 배치할 수 없는 알 수 없는 CardData 타입입니다: {card.cardName}");
        return; 
    }
    
    // 2. UnitInstance 초기화에 필요한 공통 데이터 추출
    // BaseCard와 UnitCard가 모두 CardData를 상속하고, 
    // UnitInstance는 UnitCard 또는 BaseCard를 받을 수 있어야 합니다.
    
    // *주의*: UnitInstance.Initialize(UnitCard)만 받는다면 문제가 됩니다. 
    // UnitInstance가 CardData를 받고, 내부에서 Unit/BaseCard에 공통된 능력치를 추출하도록 수정해야 합니다.
    
    // 임시방편: 현재 로직을 유지하기 위해, BaseCard도 UnitInstance가 초기화할 수 있다고 가정합니다.
    CardData dataToPass = (unitData != null) ? (CardData)unitData : (CardData)baseData;


    // 3. 셀의 중앙 월드 좌표를 얻어와 프리팹을 생성합니다.
    // gameGrid.GetCellCenterWorld()는 그리드 종류(직사각형, 육각형)에 관계없이 항상 셀의 정확한 중앙 위치를 반환합니다.
    // 기존의 수동 오프셋 계산은 육각형 그리드에서 위치를 잘못 이동시키는 원인이었습니다.
    Vector3 worldPosition = gameGrid.GetCellCenterWorld(cellLocation);
    
    GameObject newUnitObject = Instantiate(unitPrefab, worldPosition, Quaternion.identity);

    // 4. UnitInstance 초기화 (BaseCard 처리를 위해 Initialize 함수 수정이 필요할 수 있습니다.)
    // PlacementManager.cs의 SpawnUnit 함수 (Initialize 호출 부분만 수정)

// ... (중략: UnitCard/BaseCard 캐스팅 및 unitSpriteToUse 결정 로직)

    // 4. UnitInstance 초기화
    UnitInstance unitInstance = newUnitObject.GetComponent<UnitInstance>();

    if (unitInstance != null)
    {
        unitInstance.Initialize(card); // 카드 데이터 초기화
        unitInstance.owner = GameManager.Instance.currentPlayer; // 소유자 설정
        unitInstance.location = cellLocation;

        // 유닛 레지스트리에 등록
        GameManager.Instance.RegisterUnit(cellLocation, unitInstance);

        // 유닛의 초기 가시성 설정
        if (unitInstance.owner != GameManager.Player.Player1)
        {
            unitInstance.IsVisible = false; // 상대(AI, 네트워크) 유닛은 보이지 않게 시작
        }
        else
        {
            unitInstance.IsVisible = true; // 내 유닛은 보이게 시작
        }
        
        // 스프라이트 할당 로직은 unitSpriteToUse를 사용하여 그대로 유지
        SpriteRenderer sr = newUnitObject.GetComponent<SpriteRenderer>();
        if (sr != null && unitSpriteToUse != null)
        {
            sr.sprite = unitSpriteToUse; 
        }
        
        Debug.Log($"[{card.cardType}] '{card.cardName}'를 {cellLocation} 위치에 배치했습니다. (소유자: {unitInstance.owner})");
    }
}
}