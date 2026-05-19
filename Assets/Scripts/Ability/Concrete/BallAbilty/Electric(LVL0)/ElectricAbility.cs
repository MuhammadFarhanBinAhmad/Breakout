using UnityEngine;

public class ElectricAbility : ABSAbility
{
    public override void OnHitResolved(HitContext ctx)
    {

        var statusCtx = new AbilityContext
        {
            _abililty = this,
            _statusType = _SOAbilityEffect._statusType,
        };
        statusCtx._Stats[STATID.STACKS_TO_ADD] = _SOAbilityEffect._stacksToAdd;
        statusCtx._Stats[STATID.MAX_STACKS] = _SOAbilityEffect._maxStacks;
        statusCtx._Stats[STATID.DAMAGE_PER_STACK] = _SOAbilityEffect._damagePerStack;
        statusCtx._Stats[STATID.STACK_LIFETIME] = _SOAbilityEffect._stackLifeTime;
        statusCtx._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE] = _SOAbilityEffect._timeBeforeEffectActivate;
        statusCtx._Statsbool[STATID.RESET_STACK_TIMER] = _SOAbilityEffect._resetStackTimer;

        _abilityManager.ApplyDischargeModifiers(ctx, statusCtx);

        ctx._brick.ApplyStatus(
            statusCtx
        );
    }
}
