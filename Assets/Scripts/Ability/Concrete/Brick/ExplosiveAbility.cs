using UnityEngine;

public class ExplosiveAbility : ABSAbility
{

    ExplosionPool _explosionPool;

    private void Start()
    {
        _explosionPool = FindAnyObjectByType<ExplosionPool>();
    }

    public override void OnHit(HitContext ctx)
    {
        GameObject explosion = _explosionPool.GetExplosion();
        explosion.transform.position = ctx._brick.transform.position;
    }

}
