using System;

[Serializable]
public class StatusEffect
{
    public string name;        // 표시될 이름 (예: [소화], [화상])
    public Enums.StatusType type;
    public int value;          // 버프/디버프 수치
    public int remainingTurns; // 남은 턴 수
    public bool isPermanent;   // 영구 지속 여부
    public UnitInstance creator; // 효과를 부여한 시전자
    public bool justApplied = true; // 생성된 직후 턴 감소 방지용

    public StatusEffect(string name, Enums.StatusType type, int value, int duration, bool isPermanent = false, UnitInstance creator = null)
    {
        this.name = name;
        this.type = type;
        this.value = value;
        this.remainingTurns = duration;
        this.isPermanent = isPermanent;
        this.creator = creator;
        this.justApplied = true;
    }

    // 턴 감소 (true 반환 시 효과 만료)
    public bool Tick()
    {
        if (isPermanent) return false;

        if (justApplied)
        {
            justApplied = false;
            return false;
        }

        remainingTurns--;
        return remainingTurns <= 0;
    }
}
