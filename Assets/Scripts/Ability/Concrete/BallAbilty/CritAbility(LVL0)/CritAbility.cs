using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

public class CritAbility : ABSAbility
{

    public override void ModifyHit(HitContext ctx)
    {

        AbilityContext cctx = new AbilityContext { };
        cctx._Stats[STATID.BASE_DAMAGE] = _SOAbilityEffect._baseDamageValue;
        cctx._Stats[STATID.CRIT_CHANCE] = _SOAbilityEffect._baseCritChance;
        cctx._Stats[STATID.CRIT_MULTIPLIER] = _SOAbilityEffect._critMultiplier;

        _abilityManager.ApplyCriticalModifiers(ctx, cctx);

        bool isCrit =  RNGService.RollCrit(cctx._Stats[STATID.CRIT_CHANCE], _SOAbilityEffect._bonusPerFail);

        if (isCrit)
        {
            ctx._isCrit = true;
            ctx._damageValue = ctx._damageValue + Mathf.CeilToInt(
                cctx._Stats[STATID.BASE_DAMAGE] * cctx._Stats[STATID.CRIT_MULTIPLIER]
            );
        }

        return; // only one crit owner

    }
}
