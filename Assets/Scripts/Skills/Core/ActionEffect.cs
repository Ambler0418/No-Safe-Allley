using UnityEngine;

public abstract class ActionEffect : ScriptableObject
{
    public abstract void Apply(UnitInstance caster, Vector3Int targetTile);
}
