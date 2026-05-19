using UnityEngine;

public class CritBleedingAbility : ABSAbility
{


    public override void OnHit(HitContext ctx)
    {
        //Increase base damage
        var critChance = _SOAbilityEffect._baseCritChance;
        var critMultiplier = _SOAbilityEffect._critMultiplier;
        bool isCrit = RNGService.RollCrit(critChance, _SOAbilityEffect._bonusPerFail);

        if (isCrit)
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

            ctx._brick.ApplyStatus(
                statusCtx
            );
        }

        return; // only one crit owner

    }

}
