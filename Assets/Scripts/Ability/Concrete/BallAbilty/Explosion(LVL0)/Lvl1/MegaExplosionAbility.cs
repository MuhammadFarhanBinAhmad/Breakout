using UnityEngine;

public class MegaExplosionAbility : ABSAbility, IExplosionContextModifier
{
    public void ModifyExplosionContext(HitContext hitCtx, ref ExplosionContext explosionCtx)
    {
        explosionCtx._scaleMultiplier = _SOAbilityEffect._explosionSizeMultiplier;
        explosionCtx._damage = (int)(explosionCtx._damage * _SOAbilityEffect._explosionDamageMultiplier);
    }
}
