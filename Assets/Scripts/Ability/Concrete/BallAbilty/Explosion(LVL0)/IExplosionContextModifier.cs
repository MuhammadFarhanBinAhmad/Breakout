using UnityEngine;

public interface IExplosionContextModifier
{
    void ModifyExplosionContext(HitContext hitCtx, ref ExplosionContext explosionCtx);

}
