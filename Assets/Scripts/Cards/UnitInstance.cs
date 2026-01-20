using UnityEngine;
using System.Collections.Generic;
using System.Collections; // IEnumerator를 위해 추가

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

    // --- 피격 효과 설정 ---
    [Header("Damage Feedback Settings")]
    [SerializeField] private GameObject _damageNumberPrefab; // 데미지 숫자 프리팹
    [SerializeField] private Vector3 _damageNumberSpawnOffset = new Vector3(0, 0.5f, 0); // 데미지 숫자 생성 위치 오프셋
    [SerializeField] private float _damageFeedbackDuration = 0.2f; // 피격 효과 지속 시간
    [SerializeField] private float _shakeMagnitude = 0.1f;         // 흔들림 강도
    [SerializeField] private float _shakeSpeed = 50f;              // 흔들림 속도

    [Header("Selection Feedback")]
    [SerializeField] private float _selectionScaleMultiplier = 1.2f; // 선택 시 커지는 배율

    private Vector3 _originalScale; // 원래 크기
    private Color _originalColor; // SpriteRenderer의 원래 색상
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
                return GetAdjacentGovernmentAllyCount() >= 4;
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

    /// <summary>
    /// 인접한 아군 중 '정부' 진영 유닛의 수를 반환합니다. (G004 조건부 스킬용)
    /// </summary>
    private int GetAdjacentGovernmentAllyCount()
    {
        int count = 0;
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != this && unit.owner == this.owner && unit.Faction == Enums.Faction.Government)
            {
                if (IsAdjacent(this.location, unit.location))
                {
                    count++;
                }
            }
        }
        return count;
    }

    private bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        if (pos1 == pos2) return false;
        if (pos1.y % 2 == 0) // 짝수 행
        {
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;
            if (dy == 1) return dx == -1 || dx == 0;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 0 || dx == -1;
        }
        else // 홀수 행
        {
            int dx = pos2.x - pos1.x;
            int dy = pos2.y - pos1.y;
            if (dy == 1) return dx == 0 || dx == 1;
            if (dy == 0) return dx == 1 || dx == -1;
            if (dy == -1) return dx == 1 || dx == 0;
        }
        return false;
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
    private bool _isRevealed = false; // 기본은 은신
    private bool _isIdentified = false; // 정보 식별 여부

    [Header("Visibility Flags")]
    public bool isTracking = false; // [매의 눈] 상태: 이동 시 즉시 노출됨

    // 유닛의 가시성 프로퍼티
    public bool isRevealed
    {
        get { return _isRevealed; }
        set 
        { 
            if (_isRevealed != value)
            {
                _isRevealed = value;
                Debug.Log($"[Visibility] {sourceCardData.cardName} 위치 노출 상태: {_isRevealed}");
            }
        }
    }

    public bool isIdentified
    {
        get { return _isIdentified; }
        set
        {
            if (_isIdentified != value)
            {
                _isIdentified = value;
                Debug.Log($"[Visibility] {sourceCardData.cardName} 정보 식별 상태: {_isIdentified}");
            }
            
            // 내 유닛(Player1)이거나, 정보가 식별된 상태면 필드에서 보임
            bool shouldBeVisible = _isIdentified || (owner == GameManager.Player.Player1);
            
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = shouldBeVisible;
            }
            if (HealthBarCanvas != null)
            {
                HealthBarCanvas.enabled = shouldBeVisible;
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
        if (spriteRenderer != null)
        {
            _originalColor = spriteRenderer.color; // 초기 색상 저장
        }
        _originalScale = transform.localScale; // 초기 크기 저장
    }

    // --- 선택 효과 ---
    public void OnSelected()
    {
        transform.localScale = _originalScale * _selectionScaleMultiplier;
    }

    public void OnDeselected()
    {
        transform.localScale = _originalScale;
    }


    // UnitCard와 BaseCard 모두를 초기화하기 위해 CardData를 매개변수로 받습니다.
    public void Initialize(CardData data, GameManager.Player owner)
    {
        sourceCardData = data;
        this.owner = owner; // 소유자 설정
        
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
        
        // 모든 유닛은 처음에는 미식별(은신) 상태로 시작
        isIdentified = false;
        isRevealed = false;

        healthBar.updateHealthBar(currentHealth, maxHealth);
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

    private UnitInstance GetBarrierProvider()
    {
        // 내 주변에 BarrierPassive를 가진 살아있는 아군 거점이 있는지 확인
        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit != this && unit.owner == this.owner && unit.currentHealth > 0)
            {
                Debug.Log($"[Barrier] {sourceCardData.cardName} 주변 유닛 확인: {unit.sourceCardData.cardName}");
                // 육각형 그리드 상에서 인접한지 체크
                if (IsAdjacent(this.location, unit.location))
                {
                    Debug.Log($"[Barrier] {sourceCardData.cardName}과(와) {unit.sourceCardData.cardName}은(는) 인접해 있습니다.");
                    // 패시브 체크: BarrierPassive를 가지고 있는지 확인
                    if (unit.sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null && baseCard.passiveSkill is BarrierPassive)
                    {
                        Debug.Log($"[Barrier] {unit.sourceCardData.cardName}이(가) {sourceCardData.cardName}을(를) 보호합니다!");
                        return unit;
                    }
                }
            }
        }
        Debug.Log($"[Barrier] {sourceCardData.cardName} 주변에 보호자가 없습니다.");
        return null;
    }

    public void TakeDamage(int damage)
    {
        // 1. 배리어 체크: 인접한 아군 중 BarrierPassive를 가진 유닛이 있는지 확인
        // 단, 나 자신이 이미 배리어 유닛(은신처 등)이라면 다른 배리어의 보호를 받지 않음
        bool isAlreadyBarrierUnit = (sourceCardData is BaseCard baseCardSelf && baseCardSelf.passiveSkill is BarrierPassive);
        
        UnitInstance barrierUnit = isAlreadyBarrierUnit ? null : GetBarrierProvider();
        
        if (barrierUnit != null)
        {
            Debug.Log($"[Barrier] {sourceCardData.cardName}이(가) 받을 피해 {damage}를 {barrierUnit.sourceCardData.cardName}이(가) 대신 받습니다! 본인은 은신을 유지합니다.");
            
            // 보호자(은신처)가 대신 피해를 입음 -> TakeDamageDirectly에 의해 은신처는 공개됨
            barrierUnit.TakeDamageDirectly(damage); 
            
            // 원래 타겟은 여기서 함수가 종료되므로:
            // - 피해를 입지 않음 (ModifyHealth 호출 안 됨)
            // - 공개되지 않음 (isRevealed = true 호출 안 됨)
            return; 
        }

        // 2. 보호자가 없거나 내가 배리어 유닛이면 본인이 직접 피해를 입음
        TakeDamageDirectly(damage);
    }

    public void heal(int amount)
    {
        ModifyHealth(amount); // ModifyHealth 사용
    }

    // 배리어 보호 없이 직접 데미지를 받는 내부 함수
    public void TakeDamageDirectly(int damage, UnitInstance attacker = null)
    {
        // 데미지를 받으면 자신의 모습을 드러냅니다.
        if (!isRevealed || !isIdentified)
        {
            isRevealed = true;
            isIdentified = true; // 데미지를 입으면 식별됨
            
            // 공개되었으므로 정찰 하이라이트(노란색)가 있다면 제거
            if (TileEffectManager.Instance != null)
            {
                TileEffectManager.Instance.RemoveReconHighlight(location);
            }
        }

        ModifyHealth(-damage); // ModifyHealth 사용

        // --- 카운터/패시브 알림 추가 ---
        List<UnitInstance> units = new List<UnitInstance>(GameManager.Instance.unitRegistry.Values);
        foreach (var unit in units)
        {
            if (unit != null && unit.gameObject.activeInHierarchy)
            {
                if (unit.sourceCardData is BaseCard baseCard && baseCard.passiveSkill != null)
                {
                    baseCard.passiveSkill.OnTakeDamage(unit, damage, attacker);
                }
            }
        }

        // --- 피격 효과 재생 ---
        if (spriteRenderer != null) // SpriteRenderer가 있는 유닛만 효과 재생
        {
            StartCoroutine(ShowDamageFeedback());
        }

        // --- 데미지 숫자 표시 ---
        if (_damageNumberPrefab != null)
        {
            if (HealthBarCanvas != null)
            {
                Vector3 spawnPosition = transform.position + _damageNumberSpawnOffset;
                GameObject numberGO = Instantiate(_damageNumberPrefab, spawnPosition, Quaternion.identity, HealthBarCanvas.transform);
                DamageNumber dn = numberGO.GetComponent<DamageNumber>();
                if(dn != null)
                {
                    dn.SetText(damage);
                }
            }
            else
            {
                Debug.LogWarning("HealthBarCanvas가 없어 데미지 숫자를 표시할 수 없습니다.");
            }
        }
    }

    private IEnumerator ShowDamageFeedback()
    {
        if (spriteRenderer == null) yield break;

        // 원래 색상 저장
        _originalColor = spriteRenderer.color;

        // 빨간색으로 변경
        spriteRenderer.color = Color.red; // 또는 new Color(0.8f, 0.2f, 0.2f, 1f) 등 다크 테마에 어울리는 색상

        // 흔들림 효과
        Vector3 originalPosition = transform.localPosition;
        float timer = 0f;

        while (timer < _damageFeedbackDuration)
        {
            // 좌우로 흔들림
            float xOffset = Mathf.Sin(timer * _shakeSpeed) * _shakeMagnitude;
            transform.localPosition = originalPosition + new Vector3(xOffset, 0f, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        // 원래 위치와 색상으로 복구
        transform.localPosition = originalPosition;
        spriteRenderer.color = _originalColor;
    }

    // --- 상태 이상 관리 메서드 ---

    public void AddStatus(StatusEffect newStatus)
    {
        // 중복 로직은 기획에 따라 다를 수 있음
        activeStatuses.Add(newStatus);
        
        // 매의 눈(Tracking) 상태가 추가되면 플래그 즉시 갱신
        if (newStatus.type == Enums.StatusType.Tracking) isTracking = true;

        Debug.Log($"{sourceCardData.cardName}에게 {newStatus.name}({newStatus.type}) 효과 적용 ({newStatus.value}, {newStatus.remainingTurns}턴).");
    }

    // 편의를 위한 오버로드
    public void AddStatus(string name, Enums.StatusType type, int value, int duration, UnitInstance creator = null)
    {
        AddStatus(new StatusEffect(name, type, value, duration, false, creator));
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
        // 매의 눈 플래그 갱신
        if (type == Enums.StatusType.Tracking) isTracking = HasStatus(Enums.StatusType.Tracking);
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
        
        // 상태 정산 후 매의 눈 플래그 동기화
        isTracking = HasStatus(Enums.StatusType.Tracking);
        
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
