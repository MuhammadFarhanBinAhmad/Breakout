using UnityEngine;

public class ElectricAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var dmg = GetStat(UPGRADETARGET.STATUSDAMAGE).GetValue(UPGRADETARGET.STATUSDAMAGE);
        var delay = GetStat(UPGRADETARGET.EFFECTDURATION).GetValue(UPGRADETARGET.EFFECTDURATION);

        print("Damage: " + dmg);
        ctx._brick.ApplyStatus(
         StatusType.Discharge,
            stacksToAdd: 1,
            damagePerStack: (int)dmg,
            decayDuration: delay,
            maxStacks: _SOAbilityEffect._maxStack
        );
    }
}
