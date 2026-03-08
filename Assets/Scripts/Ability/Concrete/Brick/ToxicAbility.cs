using UnityEngine;

public class ToxicAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var _statusDamage = _SOAbilityEffect._statusDamage;
        var _effectDuration = _SOAbilityEffect._effectDuration;

        ctx._brick.ApplyStatus(
         StatusType.Poison,
            stacksToAdd: 1,
            damagePerStack: (int)_statusDamage,
            decayDuration: _effectDuration,
            maxStacks: _SOAbilityEffect._maxStack
        );
    }
}
