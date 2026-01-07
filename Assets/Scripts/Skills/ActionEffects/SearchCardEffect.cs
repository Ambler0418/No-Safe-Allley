using UnityEngine;

[CreateAssetMenu(fileName = "New Search Card Effect", menuName = "Skills/Action Effects/Search Card")]
public class SearchCardEffect : ActionEffect
{
    public Enums.Faction targetFaction;

    public override bool Apply(UnitInstance caster, Vector3Int targetTile)
    {
        if (HandManager.Instance != null)
        {
            HandManager.Instance.SearchCardByFaction(targetFaction);
            return true;
        }
        else
        {
            Debug.LogError("HandManager Instance not found.");
            return false;
        }
    }
}
