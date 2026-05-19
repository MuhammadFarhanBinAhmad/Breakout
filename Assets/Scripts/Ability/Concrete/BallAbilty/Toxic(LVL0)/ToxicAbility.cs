using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class ToxicAbility : ABSAbility
{
    ToxicEffectPool _toxicEffectPool;

    private void Start() => _toxicEffectPool = FindAnyObjectByType<ToxicEffectPool>();

    public override void OnHitResolved(HitContext ctx)
    {
        var statusCtx = new ToxicContext
        {
            _abililty = this,
            _statusType = _SOAbilityEffect._statusType,
        };

        statusCtx._Stats[STATID.STACKS_TO_ADD] = _SOAbilityEffect._stacksToAdd;
        statusCtx._Stats[STATID.MAX_STACKS] = _SOAbilityEffect._maxStacks;
        statusCtx._Stats[STATID.DAMAGE_PER_STACK] = _SOAbilityEffect._damagePerStack;
        statusCtx._Stats[STATID.STACK_LIFETIME] = _SOAbilityEffect._stackLifeTime;
        statusCtx._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE] = _SOAbilityEffect._timeBeforeEffectActivate;
        statusCtx._Stats[STATID.SPEED_MULTIPLIER] = _SOAbilityEffect._speedMultiplier;

        statusCtx._Statsbool[STATID.RESET_STACK_TIMER] = _SOAbilityEffect._resetStackTimer;
        statusCtx._Statsbool[STATID.AFFECTS_SPEED] = _SOAbilityEffect._affectSpeed;

        _abilityManager.ApplyToxicModifiers(statusCtx);

        ctx._brick.ApplyStatus(
            statusCtx
        );

        //GameObject effect = _toxicEffectPool.GetObject();

        //effect.transform.parent = ctx._brick.transform;

    }
}
