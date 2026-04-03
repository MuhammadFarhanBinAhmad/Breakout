using UnityEngine;

public class ElectricAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var dmg = _SOAbilityEffect._statusDamage;
        var delay = _SOAbilityEffect._effectDuration;

        ctx._brick.ApplyStatus(_SOStatusEffect
        );
    }
}
