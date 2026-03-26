using Unity.VisualScripting;
using UnityEngine;

public class ExplosiveAbility : ABSAbility
{

    protected ExplosionPool _explosionPool;

    private void Start()
    {
        _explosionPool = FindAnyObjectByType<ExplosionPool>();
    }

    public override void OnHit(HitContext ctx)
    {
        if (_explosionPool == null) return;

        GameObject explosionGO = _explosionPool.GetExplosion();
        explosionGO.transform.position = transform.position;

        var ed = explosionGO.GetComponent<ExplosionDamage>();
        if (ed == null) return;

        ExplosionContext ectx = new ExplosionContext
        {
            _damage = ctx._baseDamage,
            _source = gameObject,
            _position = ctx._brick.transform.position,
            _scaleMultiplier = 1f,
            _statusEffect = null
        };

        // Let other abilities modify the explosion data
        _abilityManager.ApplyExplosionModifiers(ctx, ref ectx);

        ed.Initialize(ectx);
    }
}
