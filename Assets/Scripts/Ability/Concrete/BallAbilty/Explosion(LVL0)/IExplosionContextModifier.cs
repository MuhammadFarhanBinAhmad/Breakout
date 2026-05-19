using UnityEngine;

public interface IExplosionContextModifier
{
    void ModifyExplosionContext(HitContext hitCtx, ExplosionContext explosionCtx);

}
