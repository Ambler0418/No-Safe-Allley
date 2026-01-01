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
    private HealthBar healthBar;

    private Canvas HealthBarCanvas;
    public int maxHealth

    {
        get
        {
            if (sourceCardData is UnitCard unitData)
            {
                return unitData.maxHealth;
            }
            else if (sourceCardData is BaseCard baseData)
            {
                return baseData.maxHealth;
            }
            else
            {
                Debug.LogError($"[UnitInstance] 알 수 없는 카드 타입에서 maxHealth를 가져오려 함: {sourceCardData.cardName}");
                return 1; // 안전 값
            }
        }
    }

    // 유닛의 공격력 (UnitCard에서 가져옴)
    public int Attack => (sourceCardData as UnitCard)?.attack ?? 0;
    // 유닛의 방어력 (UnitCard에서 가져옴)
    public int Defense => (sourceCardData as UnitCard)?.defense ?? 0;

    // 카드의 종류(UnitCard, BaseCard)에 관계없이 활성화된 스킬을 가져오는 프로퍼티
    public SkillEffect ActiveSkill
    {
        get
        {
            if (sourceCardData is UnitCard unitData)
            {
                return unitData.activeSkill;
            }
            if (sourceCardData is BaseCard baseData)
            {
                return baseData.activeSkill;
            }
            return null;
        }
    }


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
            if (HealthBarCanvas != null)
            {
                HealthBarCanvas.enabled = _isVisible;
            }
        }
    }

    void Awake()
    {
        // 컴포넌트 참조 캐싱
        spriteRenderer = GetComponent<SpriteRenderer>();
        healthBar = GetComponentInChildren<HealthBar>();
        HealthBarCanvas = GetComponentInChildren<Canvas>();
        if (healthBar == null)
        {
            Debug.LogError(this.gameObject.name + "에서 HealthBar 컴포넌트를 찾을 수 없습니다! 프리팹 설정을 확인하세요.");
        }
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
        healthBar.updateHealthBar(currentHealth, maxHealth);
        IsVisible = _isVisible;
    }

    // 예시: 데미지를 입는 함수
    public void TakeDamage(int damage)
    {
        // 데미지를 받으면 자신의 모습을 드러냅니다.
        IsVisible = true;

        currentHealth -= damage;
        Debug.Log($"{sourceCardData.cardName}이 {damage} 데미지를 입었습니다. 남은 체력: {currentHealth}");
        healthBar.updateHealthBar(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        Debug.Log($"{sourceCardData.cardName}이(가) {amount}만큼 회복했습니다. 현재 체력: {currentHealth}");
        healthBar.updateHealthBar(currentHealth, maxHealth);
    }

    // 예시: 파괴되는 함수
    private void Die()
    {
        Debug.Log($"{sourceCardData.cardName}이 파괴되었습니다.");
        GameManager.Instance.DeregisterUnit(this.location); // 레지스트리에서 자신을 제거

        // 파괴된 것이 유닛 카드인 경우에만 플레이어에게 데미지를 줍니다.
        if (sourceCardData is UnitCard unitCard)
        {
            // 피해량을 카드의 성급(rarity)으로 전달합니다.
            int damageToPlayer = (int)unitCard.rarity;
            GameManager.Instance.ReducePlayerHealth(owner, damageToPlayer);
        }
        // 거점(BaseCard)이 파괴될 경우에는 플레이어 데미지가 없습니다.

        Destroy(gameObject);
    }


}
