using UnityEngine;
using System.Collections.Generic;

public class UnitInstance : MonoBehaviour
{
    // 이 인스턴스가 어떤 카드 데이터에서 왔는지 참조
    public CardData sourceCardData;
    public GameManager.Player owner; // 이 유닛의 소유자

    // 유닛의 소속 (진영)
    public Enums.Faction Faction => sourceCardData.faction;

    // 현재 유닛의 상태
    public int currentHealth;
    public Vector3Int location; // Grid 셀 위치
    public bool hasUsedSkillThisTurn = false; // 이번 턴에 스킬을 사용했는지 여부
    private HealthBar healthBar;

    // --- 상태 이상 관리 ---
    public List<StatusEffect> activeStatuses = new List<StatusEffect>();

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

    // 유닛의 공격력 (기본 공격력 + 버프/디버프)
    public int Attack
    {
        get
        {
            int baseAttack = (sourceCardData as UnitCard)?.attack ?? 0;
            int modifier = 0;
            
            foreach (var status in activeStatuses)
            {
                if (status.type == Enums.StatusType.AttackBuff) modifier += status.value;
                if (status.type == Enums.StatusType.AttackDebuff) modifier -= status.value;
            }

            return Mathf.Max(0, baseAttack + modifier); // 공격력은 0보다 작아질 수 없음
        }
    }

    // 유닛의 방어력 (기본 방어력 + 버프/디버프)
    public int Defense
    {
        get
        {
            int baseDefense = (sourceCardData as UnitCard)?.defense ?? 0;
            int modifier = 0;

            foreach (var status in activeStatuses)
            {
                if (status.type == Enums.StatusType.DefenseBuff) modifier += status.value;
                if (status.type == Enums.StatusType.DefenseDebuff) modifier -= status.value;
            }

            return Mathf.Max(0, baseDefense + modifier);
        }
    }

    // 가하는 피해 배율 (기본 1.0, 최소 0)
    public float DamageDealtMultiplier
    {
        get
        {
            float bonus = 0;
            foreach (var status in activeStatuses)
            {
                if (status.type == Enums.StatusType.DamageDealtBonus) bonus += status.value / 100f;
            }
            return Mathf.Max(0, 1.0f + bonus);
        }
    }

    // 받는 피해 배율 (기본 1.0, 최소 0)
    public float DamageTakenMultiplier
    {
        get
        {
            float bonus = 0;
            foreach (var status in activeStatuses)
            {
                if (status.type == Enums.StatusType.DamageTakenBonus) bonus += status.value / 100f;
            }
            return Mathf.Max(0, 1.0f + bonus);
        }
    }

    /// <summary>
    /// 상태 이상을 고려한 스킬의 최종 에너지 소모량을 계산합니다.
    /// </summary>
    public int GetSkillCost(SkillEffect skill)
    {
        if (skill == null) return 0;

        int reduction = 0;
        foreach (var status in activeStatuses)
        {
            if (status.type == Enums.StatusType.SkillCostReduction)
            {
                reduction += status.value;
            }
        }

        return Mathf.Max(0, skill.energyCost - reduction);
    }

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
                return null; // 거점은 액티브 스킬이 없음
            }
            return null;
        }
    }

    // 조건부 스킬 (2번째 스킬) 가져오기
    public SkillEffect ConditionalSkill
    {
        get
        {
            if (sourceCardData is UnitCard unitData)
            {
                return unitData.conditionalSkill;
            }
            return null;
        }
    }

    /// <summary>
    /// 현재 조건부 스킬을 사용할 수 있는 상태인지 확인합니다.
    /// </summary>
    public bool CanUseConditionalSkill()
    {
        if (sourceCardData is UnitCard unitData)
        {
            string condition = unitData.conditionalSkillCondition;
            if (string.IsNullOrEmpty(condition)) return true; // 조건이 없으면 항상 가능

            // G004: 인접 아군 4명 이상
            if (condition == "AdjacentAllies_4")
            {
                return GetAdjacentAllyCount() >= 4;
            }
            // I004: 사망한 적 유닛 3기 이상
            if (condition == "DeadEnemies_3")
            {
                return GameManager.Instance.deadEnemyCount >= 3;
            }
            // S004: 상대 필드 유닛 2기 이하
            if (condition == "EnemyCount_LE_2")
            {
                return GetTotalEnemyCount() <= 2;
            }
        }
        return false;
    }

    private int GetAdjacentAllyCount()
    {
        int count = 0;
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return 0;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != this && unit.owner == this.owner)
            {
                Vector3 worldPos1 = grid.GetCellCenterWorld(this.location);
                Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                if (Vector3.Distance(worldPos1, worldPos2) < 1.5f * grid.cellSize.x)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private int GetTotalEnemyCount()
    {
        int count = 0;
        GameManager.Player opponent = (owner == GameManager.Player.Player1) ? GameManager.Player.Player2 : GameManager.Player.Player1;
        
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit.owner == opponent) count++;
        }
        return count;
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

    // 체력을 직접 수정하고 UI를 갱신하는 공통 메서드
    public void ModifyHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 0 ~ Max 사이로 제한

        Debug.Log($"{sourceCardData.cardName} 체력 변경: {amount} (현재: {currentHealth})");
        
        if (healthBar != null)
        {
            healthBar.updateHealthBar(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 예시: 데미지를 입는 함수
    public void TakeDamage(int damage)
    {
        // 배리어 체크: 인접한 아군 중 BarrierPassive를 가진 유닛이 있는지 확인
        UnitInstance barrierUnit = GetBarrierProvider();
        if (barrierUnit != null)
        {
            Debug.Log($"{sourceCardData.cardName}이(가) 받을 데미지 {damage}를 {barrierUnit.sourceCardData.cardName}이(가) 대신 받습니다!");
            barrierUnit.TakeDamageDirectly(damage); 
            return; // 원래 타겟은 데미지 없음, 공개 안됨
        }

        TakeDamageDirectly(damage);
    }

    // 배리어 보호 없이 직접 데미지를 받는 내부 함수
    public void TakeDamageDirectly(int damage)
    {
        // 데미지를 받으면 자신의 모습을 드러냅니다.
        IsVisible = true;

        ModifyHealth(-damage); // ModifyHealth 사용
    }

    private UnitInstance GetBarrierProvider()
    {
        // BarrierPassive를 가진 인접 아군 찾기
        Grid grid = GameManager.Instance.gameGrid;
        if (grid == null) return null;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != this && unit.owner == this.owner && unit.currentHealth > 0)
            {
                // 인접 체크
                Vector3 worldPos1 = grid.GetCellCenterWorld(this.location);
                Vector3 worldPos2 = grid.GetCellCenterWorld(unit.location);
                if (Vector3.Distance(worldPos1, worldPos2) < 1.5f * grid.cellSize.x)
                {
                    // 패시브 체크 (타입 직접 참조 대신 문자열 비교로 안전하게 처리)
                    if (unit.sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null && baseCard.passiveSkill.GetType().Name == "BarrierPassive")
                    {
                        return unit;
                    }
                }
            }
        }
        return null;
    }

    public void heal(int amount)
    {
        ModifyHealth(amount); // ModifyHealth 사용
    }

    // --- 상태 이상 관리 메서드 ---

    public void AddStatus(StatusEffect newStatus)
    {
        // 중복 로직은 기획에 따라 다를 수 있음
        activeStatuses.Add(newStatus);
        Debug.Log($"{sourceCardData.cardName}에게 {newStatus.type} 효과 적용 ({newStatus.value}, {newStatus.remainingTurns}턴).");
    }

    // 편의를 위한 오버로드
    public void AddStatus(Enums.StatusType type, int value, int duration, UnitInstance creator = null)
    {
        AddStatus(new StatusEffect(type, value, duration, false, creator));
    }

    public void RemoveStatus(Enums.StatusType type)
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            if (activeStatuses[i].type == type)
            {
                activeStatuses.RemoveAt(i);
            }
        }
    }

    public bool HasStatus(Enums.StatusType type)
    {
        foreach (var status in activeStatuses)
        {
            if (status.type == type) return true;
        }
        return false;
    }

    // 턴 시작 시 호출되어 상태 지속 시간을 감소시키고 만료된 효과 제거
    public void OnTurnStart()
    {
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            if (activeStatuses[i].Tick())
            {
                Debug.Log($"{sourceCardData.cardName}의 {activeStatuses[i].type} 효과가 만료되었습니다.");
                activeStatuses.RemoveAt(i);
            }
        }
        
        // 행동 여부 초기화
        hasUsedSkillThisTurn = false;
        
        // 패시브 스킬 처리 (추후 구현 시 여기에 추가)
        HandlePassiveSkills();
    }

    private void HandlePassiveSkills()
    {
        // 패시브 스킬 시스템 구현 후 연결 예정
        if (sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null)
        {
            baseCard.passiveSkill.OnTurnStart(this);
        }
    }

    // 예시: 파괴되는 함수
    private void Die()
    {
        Debug.Log($"{sourceCardData.cardName}이 파괴되었습니다.");
        GameManager.Instance.DeregisterUnit(this.location); // 레지스트리에서 자신을 제거
        GameManager.Instance.NotifyUnitDeath(this); // 사망 알림

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
