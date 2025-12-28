using UnityEngine;

public abstract class ActionEffect : ScriptableObject
{
    // 모든 자식 클래스는 이 함수를 반드시 구현해야 합니다.
    // 타겟이 필요 없는 효과의 경우, 이 함수 안에서 targetTile 매개변수를 무시하면 됩니다.
    public abstract void Apply(UnitInstance caster, Vector3Int targetTile);
}
