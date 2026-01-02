using UnityEngine;

[CreateAssetMenu(fileName = "New Search Card Effect", menuName = "Skills/Action Effects/Search Card")]
public class SearchCardEffect : ActionEffect
{
    public Enums.Faction targetFaction;

    public override void Apply(UnitInstance caster, Vector3Int targetTile)
    {
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SearchCardByFaction(targetFaction);
        }
        else
        {
            Debug.LogError("HandManager Instance not found.");
        }
    }
}
