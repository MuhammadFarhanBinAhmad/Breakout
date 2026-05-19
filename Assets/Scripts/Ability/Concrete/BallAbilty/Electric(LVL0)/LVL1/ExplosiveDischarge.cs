using UnityEngine;

public class ExplosiveDischarge : ABSAbility
{
    protected ExplosionPool _explosionPool;
    HitContext _context;
    private void Start()
    {
        _explosionPool = FindAnyObjectByType<ExplosionPool>();
    }
    public override void OnHitResolved(HitContext ctx)
    {

        var statusCtx = new AbilityContext
        {
            _abililty = this,
            _statusType = _SOAbilityEffect._statusType,
        };
        statusCtx._Stats[STATID.STACKS_TO_ADD] = _SOAbilityEffect._stacksToAdd;
        statusCtx._Stats[STATID.MAX_STACKS] = _SOAbilityEffect._maxStacks;
        statusCtx._Stats[STATID.DAMAGE_PER_STACK] = _SOAbilityEffect._damagePerStack;
        statusCtx._Stats[STATID.STACK_LIFETIME] = _SOAbilityEffect._stackLifeTime;
        statusCtx._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE] = _SOAbilityEffect._timeBeforeEffectActivate;
        statusCtx._Statsbool[STATID.RESET_STACK_TIMER] = _SOAbilityEffect._resetStackTimer;
        ctx._brick.ApplyStatus(
            statusCtx
        );
        _context = ctx;



    }

    public override void ActivateAbility()
    {
        if (_explosionPool == null) return;

        GameObject explosionGO = _explosionPool.GetExplosion();
        explosionGO.transform.position = transform.position;
        var ed = explosionGO.GetComponent<ExplosionDamage>();
        if (ed == null) return;

        ExplosionContext ectx = new ExplosionContext
        {
            _source = gameObject,
            _position = _context._brick.transform.position,
            _statusEffect = null
        };
        ectx._Stats[STATID.BASE_DAMAGE] = _context._damageValue;
        ectx._Stats[STATID.SCALE_MULTIPLIER] = 1f;

        // Let other abilities modify the explosion data
        _abilityManager.ApplyExplosionModifiers(_context, ectx);
        ed.Initialize(ectx, true);
    }
}
