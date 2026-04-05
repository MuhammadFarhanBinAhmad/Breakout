using UnityEngine;

public class HalfAndHalfExplosion : ABSAbility, IExplosionContextModifier
{
    public void ModifyExplosionContext(HitContext hitCtx, ref ExplosionContext explosionCtx)
    {
        explosionCtx._scaleMultiplier = _SOAbilityEffect._explosionSizeMultiplier ;
        explosionCtx._damage = (int)(explosionCtx._damage / _SOAbilityEffect._explosionDamageMultiplier);
    }
}
