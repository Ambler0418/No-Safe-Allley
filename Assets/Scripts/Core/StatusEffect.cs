using System;

[Serializable]
public class StatusEffect
{
    public Enums.StatusType type;
    public int value;          // 버프/디버프 수치 (예: 공격력 +2면 2)
    public int remainingTurns; // 남은 턴 수
    public bool isPermanent;   // 영구 지속 여부
    public UnitInstance creator; // 효과를 부여한 시전자 (추가)

    public StatusEffect(Enums.StatusType type, int value, int duration, bool isPermanent = false, UnitInstance creator = null)
    {
        this.type = type;
        this.value = value;
        this.remainingTurns = duration;
        this.isPermanent = isPermanent;
        this.creator = creator;
    }

    // 턴 감소 (true 반환 시 효과 만료)
    public bool Tick()
    {
        if (isPermanent) return false;

        remainingTurns--;
        return remainingTurns <= 0;
    }
}
