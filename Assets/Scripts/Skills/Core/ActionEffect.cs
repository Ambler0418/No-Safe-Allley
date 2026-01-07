using UnityEngine;

public abstract class ActionEffect : ScriptableObject
{
    // 모든 자식 클래스는 이 함수를 반드시 구현해야 합니다.
    // 타겟이 필요 없는 효과의 경우, 이 함수 안에서 targetTile 매개변수를 무시하면 됩니다.
    // 성공적으로 적용되었으면 true, 실패했으면(조건 불만족 등) false를 반환합니다.
    public abstract bool Apply(UnitInstance caster, Vector3Int targetTile);
}
