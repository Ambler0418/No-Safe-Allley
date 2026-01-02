using UnityEngine;

[CreateAssetMenu(fileName = "New Buff All Ally Passive", menuName = "Skills/Passive/Buff All Ally")]
public class BuffAllAllyPassive : PassiveSkill
{
    public Enums.StatusType statusType;
    public int value;
    public int duration = 1;
    public Enums.Faction targetFaction = Enums.Faction.IronFrame;
    public bool useFactionCheck = false;

    public override void OnTurnStart(UnitInstance owner)
    {
        base.OnTurnStart(owner);

        GameManager.Player currentPlayer = owner.owner;

        foreach (var unit in GameManager.Instance.unitRegistry.Values)
        {
            if (unit.owner == currentPlayer)
            {
                // 진영 체크
                if (useFactionCheck && unit.Faction != targetFaction) continue;

                StatusEffect buff = new StatusEffect(statusType, value, duration);
                unit.AddStatus(buff);
            }
        }
        Debug.Log($"[Passive] {owner.sourceCardData.cardName}: 아군에게 {statusType} ({value}) 부여.");
    }
}
