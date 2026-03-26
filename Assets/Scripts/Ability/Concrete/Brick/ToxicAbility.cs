using UnityEngine;

public class ToxicAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var _statusDamage = _SOAbilityEffect._statusDamage;
        var _effectDuration = _SOAbilityEffect._effectDuration;

        ctx._brick.ApplyStatus(
            _SOStatusEffect
        );
    }
}
