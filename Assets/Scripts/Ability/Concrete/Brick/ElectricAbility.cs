using UnityEngine;

public class ElectricAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var dmg = _SOAbilityEffect._statusDamage;
        var delay = _SOAbilityEffect._effectDuration;

        ctx._brick.ApplyStatus(
         StatusType.Discharge,
            stacksToAdd: 1,
            damagePerStack: (int)dmg,
            decayDuration: delay,
            maxStacks: _SOAbilityEffect._maxStack
        );
    }
}
