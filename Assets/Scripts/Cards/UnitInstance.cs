using UnityEngine;

public class UnitInstance : MonoBehaviour
{
    // 이 인스턴스가 어떤 카드 데이터에서 왔는지 참조
    public CardData sourceCardData;
    public GameManager.Player owner; // 이 유닛의 소유자

    // 현재 유닛의 상태
    public int currentHealth;
    public Vector3Int location; // Grid 셀 위치
    public bool hasUsedSkillThisTurn = false; // 이번 턴에 스킬을 사용했는지 여부

    // 내부 컴포넌트 및 상태
    private SpriteRenderer spriteRenderer;
    private bool _isVisible = true;

    // 유닛의 가시성 프로퍼티
    public bool IsVisible
    {
        get { return _isVisible; }
        set
        {
            _isVisible = value;
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = _isVisible;
            }
        }
    }

    void Awake()
    {
        // 컴포넌트 참조 캐싱
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // UnitCard와 BaseCard 모두를 초기화하기 위해 CardData를 매개변수로 받습니다.
    public void Initialize(CardData data)
    {
        sourceCardData = data;
        
        // 데이터 타입에 따라 초기 체력 설정
        if (data is UnitCard unitData)
        {
            // UnitCard의 초기화
            currentHealth = unitData.maxHealth;
            Debug.Log($"UnitInstance 초기화 완료: {unitData.cardName} (체력: {currentHealth})");
        }
        else if (data is BaseCard baseData)
        {
            // BaseCard의 초기화
            currentHealth = baseData.maxHealth; 
            Debug.Log($"UnitInstance 초기화 완료: {baseData.cardName} (거점 체력: {currentHealth})");
        }
        else
        {
            Debug.LogError($"[UnitInstance] 알 수 없는 카드 타입으로 초기화 시도: {data.cardName}");
            currentHealth = 1; // 안전 값
        }
    }

    // 예시: 데미지를 입는 함수
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{sourceCardData.cardName}이 {damage} 데미지를 입었습니다. 남은 체력: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 예시: 파괴되는 함수
    private void Die()
    {
        Debug.Log($"{sourceCardData.cardName}이 파괴되었습니다.");
        GameManager.Instance.DeregisterUnit(this.location); // 레지스트리에서 자신을 제거
        Destroy(gameObject);
    }

    // 마우스 클릭 시 호출되는 Unity 이벤트 함수
    void OnMouseDown()
    {
        // '행동' 단계가 아니면 아무것도 하지 않음
        if (GameManager.Instance.currentPhase != GameManager.GamePhase.Action) return;
        
        // 현재 턴의 플레이어가 유닛의 소유자가 아니면 아무것도 하지 않음
        if (this.owner != GameManager.Instance.currentPlayer)
        {
            Debug.Log($"상대방의 유닛({sourceCardData.cardName})은 선택할 수 없습니다.");
            return;
        }

        // 모든 조건을 통과하면 GameManager에 선택 요청
        GameManager.Instance.SelectUnit(this);
    }
}
