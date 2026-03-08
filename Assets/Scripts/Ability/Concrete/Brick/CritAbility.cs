using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;

public class CritAbility : ABSAbility
{
    RNGService.PRDState _prdState = new RNGService.PRDState();
    private RNGService _rng => RNGService.Instance;
    public SOAbilityEffect test;



    public override void ModifyHit(HitContext ctx)
    {
        //Increase base damage

        var critChance = _SOAbilityEffect._baseCritChance;
        var critMultiplier = _SOAbilityEffect._critMultiplier;

        print("Step 1.1: Check for crit");
        bool isCrit = RollCrit(critChance, _SOAbilityEffect._bonusPerFail);

        if (isCrit)
        {
            print("Modfy crit value");
            ctx._isCrit = true;
            ctx._finaleDamage = Mathf.CeilToInt(
                ctx._baseDamage * critMultiplier
            );

            print("baseDmg: " + ctx._baseDamage + " _finaleDamage: " + ctx._finaleDamage);
        }

        base.ModifyHit(ctx);
        return; // only one crit owner

    }

    public bool RollCrit(float chance, float bonusPerFail)
    {
        return RNGService.Instance.RollPRD(chance, bonusPerFail, _prdState);
    }

}
