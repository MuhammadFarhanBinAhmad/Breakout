using UnityEngine;

public class MegaExplosionAbility : ABSAbility, IExplosionContextModifier
{
    public void ModifyExplosionContext(HitContext hitCtx, ref ExplosionContext explosionCtx)
    {
        explosionCtx._scaleMultiplier = _SOAbilityEffect._scaleSizeMultiplier;
        explosionCtx._damage = Mathf.RoundToInt(hitCtx._baseDamage);
    }
}
