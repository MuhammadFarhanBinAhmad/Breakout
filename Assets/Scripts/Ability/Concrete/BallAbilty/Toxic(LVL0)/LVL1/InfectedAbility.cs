using UnityEngine;

public class InfectedAbility : ABSAbility, IToxicContextModifier
{
    public void ModifyToxicContext( AbilityContext toxicContext)
    {
        toxicContext._Statsbool[STATID.AFFECTS_SPEED] = _SOAbilityEffect._affectSpeed;
        toxicContext._Stats[STATID.SPEED_MULTIPLIER] = _SOAbilityEffect._speedMultiplier;
        toxicContext._Stats[STATID.DAMAGE_PER_STACK] *= _SOAbilityEffect._baseDamageMultiplier; 
    }
}
