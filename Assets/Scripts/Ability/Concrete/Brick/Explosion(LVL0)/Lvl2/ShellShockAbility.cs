using UnityEngine;

public class ShellShockAbility : ABSAbility, IExplosionContextModifier
{
    public void ModifyExplosionContext(HitContext hitCtx, ref ExplosionContext explosionCtx)
    {
        explosionCtx._statusEffect = _SOStatusEffect;
    }
}
