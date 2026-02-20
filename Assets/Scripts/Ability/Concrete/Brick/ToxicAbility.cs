using UnityEngine;

public class ToxicAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {
        var _statusDamage = GetStat(UPGRADETARGET.STATUSDAMAGE).GetValue(UPGRADETARGET.STATUSDAMAGE);
        var _effectDuration = GetStat(UPGRADETARGET.EFFECTDURATION).GetValue(UPGRADETARGET.EFFECTDURATION);

        ctx._brick.ApplyStatus(
         StatusType.Poison,
            stacksToAdd: 1,
            damagePerStack: (int)_statusDamage,
            decayDuration: _effectDuration,
            maxStacks: _SOAbilityEffect._maxStack
        );
    }
}
