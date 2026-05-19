using System;
using System.Collections.Generic;

using UnityEngine;

[System.Flags]
public enum STATUSTYPE
{
    NONE = 0,
    EXPLOSION = 1 << 0,
    DISCHARGE = 1 << 1,
    CRIT = 1 << 2,
    TOXIC = 1 << 3,
}
public enum DeathCause
{
    NORMAL,
    PADDLE,
    TOWER
}
[System.Serializable]
public class StatusInstance
{
    public ABSAbility _ability;

    public STATUSTYPE type;

    public int stacks;
    public int stackToAdd;
    public int maxStacks;

    public int damagePerStack;

    public float remainingStackTime,stackLifeTime;
    public float remainingEffectTime, timeBeforeEffect;

    public bool resetStackLifeTimeUponHit;
    public GameObject spawnPrefab;

    public bool affectsSpeed;
    public float speedMultiplier;
}
public class BrickBar : MonoBehaviour
{
    BrickUI _brickUI;

    Dictionary<STATUSTYPE, StatusInstance> _statuses = new Dictionary<STATUSTYPE, StatusInstance>();
    List<STATUSTYPE> toRemove = new List<STATUSTYPE>();

    public List<BrickModifierBase> _modifiers = new List<BrickModifierBase>();
    List <GameObject> _abilityEffects = new List<GameObject>();

    public SO_BrickHealthStats []_brickHealthStats = new SO_BrickHealthStats[5];
    
    BrickPool _brickPool;
    EssencePool _essencePool;

    BrickGenerator _brickGenerator;
    TowerManager _towerManager;
    AbilityManager abilityManager;
    //UIManager

    SpriteRenderer _spriteRenderer;

    public GameObject _destroyParticleEffect;

    [Header("BrickStats")]
    public int _startingHealth;
    public int _health;
    public int _layer;
    public int _shield;
    public float _tickTimer;
    public float _baseFallSpeed;
    public float _fallSpeed;
    bool _speedDirty;


    [Header("BrickDamageEffect")]
    [SerializeField] SOLerpAnimationEffect _onDamageAnimEffect;
    [SerializeField] SOLerpAnimationEffect _onDeathAnimEffect;
    [SerializeField] ParticleSystem _damageParticle;
    AnimationCurveEffect _AnimCurveEffect;


    [Header("Essence")]
    public int _essenceMinAmountToSpawn;
    public int _essenceMaxAmountToSpawn;


    bool pendingDeath;
    DeathCause pendingDeathCause;

    public Action _onDeath;
    public Action _onDeathByPaddle;
    public Action _onDeathByTower;

    private void Awake()
    {
        _brickGenerator = FindAnyObjectByType<BrickGenerator>();
        abilityManager = FindAnyObjectByType<AbilityManager>();
        _towerManager = FindAnyObjectByType<TowerManager>();


        _brickPool = FindAnyObjectByType<BrickPool>();
        _essencePool = FindAnyObjectByType<EssencePool>();

        _brickUI = GetComponent<BrickUI>();

        _AnimCurveEffect = GetComponent<AnimationCurveEffect>();

        _onDeath += HandleDeath;
        _onDeath += SpawnEssence;
        _onDeath += _brickGenerator.OnBrickDestroyed;
        _onDeath += _statuses.Clear;
        _onDeath += RemoveAllModifiers;


        _onDeathByPaddle += HandleDeathByPaddle;
        _onDeathByPaddle += _statuses.Clear;
        _onDeathByPaddle += RemoveAllModifiers;
        _onDeathByPaddle += _brickGenerator.OnBrickDestroyed;

        _onDeathByTower += HandleDeathWithotSpawningEssence;
        _onDeathByTower += _statuses.Clear;
        _onDeathByTower += RemoveAllModifiers;
        _onDeathByTower += _brickGenerator.OnBrickDestroyed;
    }
    private void OnDestroy()
    {
        _onDeath -= HandleDeath;
        _onDeath -= SpawnEssence;
        _onDeath -= _brickGenerator.OnBrickDestroyed;
        _onDeath -= _statuses.Clear;
        _onDeath -= RemoveAllModifiers;

        _onDeathByPaddle -= HandleDeathByPaddle;
        _onDeathByPaddle -= _statuses.Clear;
        _onDeathByPaddle -= RemoveAllModifiers;
        _onDeathByPaddle -= _brickGenerator.OnBrickDestroyed;

        _onDeathByTower -= HandleDeathWithotSpawningEssence;
        _onDeathByTower -= _statuses.Clear;
        _onDeathByTower -= RemoveAllModifiers;
        _onDeathByTower -= _brickGenerator.OnBrickDestroyed;

    }

    private void Update()
    {
        float dt = Time.deltaTime;
        transform.Translate(Vector3.down * _fallSpeed * Time.deltaTime);

        if (_health > 0)
            ExecuteStatusEffect();

        TickModifiers(dt);

        if (_speedDirty)
        {
            RecalculateSpeed();
            _speedDirty = false;
        }

        if (pendingDeath)
            ResolveDeath();
    }
    void ResolveDeath()
    {
        switch (pendingDeathCause)
        {
            case DeathCause.NORMAL:
                {
                    _onDeath?.Invoke();
                    break;
                }
            case DeathCause.TOWER:
                {
                    _onDeathByTower?.Invoke();
                    break;
                }
            case DeathCause.PADDLE:
                {
                    _onDeathByPaddle?.Invoke();
                    break;
                }
        }

    }

    void ExecuteStatusEffect()
    {
        float dt = Time.deltaTime;
        toRemove.Clear();

        if (pendingDeath)
            return;

        foreach (var kvp in _statuses)
        {
            var status = kvp.Value;
            //Damage effect timer
            //if (status.type == STATUSTYPE.STUN)
            //{
            //    _fallSpeed = 0;
            //}
            //else
            //{
            //    // DOT tick
            //    status.remainingEffectTime -= dt;
            //    if (status.remainingEffectTime <= 0)
            //    {
            //        status.remainingEffectTime = status.timeBeforeEffect;
            //        OnDamage(status.stacks * status.damagePerStack); //total stack * stack/dmg
            //    }
            //}

            // Stack timer
            status.remainingStackTime -= dt;
            if (status.remainingStackTime <= 0f)
            {
                status.stacks--;
                if (status.affectsSpeed)
                    MarkSpeedDirty();

                if (status.stacks <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
                else
                {
                    // Restart decay timer for next stack
                    status.remainingStackTime = status.stackLifeTime;
                }

                status._ability.ActivateAbility();
            }
            if(status.stacks >0)
            {
                //Effect timer
                status.remainingEffectTime -= dt;
                if (status.remainingEffectTime <= 0)
                {
                    status.remainingEffectTime = status.timeBeforeEffect;
                    print("TotalDmg via stack" + status.stacks * status.damagePerStack);
                    OnDamage(status.stacks * status.damagePerStack); //total stack * stack/dmg
                }
            }
            
        }

        //remove all completed status effect
        foreach (var key in toRemove)
        {
            //if (key == STATUSTYPE.STUN)
            //{
            //    _fallSpeed = _startFallSpeed;
            //}
            _statuses.Remove(key);
        }
    }
    public void OnDamage(int dmg,DeathCause deathcause = DeathCause.NORMAL)
    {
        int modified = dmg;
        for (int i = 0; i < _modifiers.Count; i++)
        {
            if (_modifiers[i] != null)
                modified = _modifiers[i].ModifyIncomingDamage(modified);
        }

        //For Hit shield effect
        if (modified <= 0)
            return;


        _health -= modified;
        for (int i = 0; i < _modifiers.Count; i++)
            _modifiers[i]?.OnDamageApplied(modified);

        UpdateBrickAfterDamage(deathcause);
    }
    public void OnDamageLayer(int amount, DeathCause deathcause = DeathCause.NORMAL)
    {
        _layer -= amount;
        if (_layer > 0)
        {
            SetBrick(_brickHealthStats[_layer]);
        }
        else
        {
            pendingDeathCause = deathcause;
            pendingDeath = true;
        }
    }

    void UpdateBrickAfterDamage(DeathCause deathcause = DeathCause.NORMAL)
    {
        if (_health <= 0)
        {
            _AnimCurveEffect.PlayEffect(_onDeathAnimEffect, this.gameObject);
            if (_layer >0 )
            {
                _layer--;
                SetBrick(_brickHealthStats[_layer]);
            }
            else
            {
                pendingDeathCause = deathcause;
                pendingDeath = true;
            }
        }
        else
        {
            _AnimCurveEffect.PlayEffect(_onDamageAnimEffect, this.gameObject);
            _damageParticle.Play();
        }
        _brickUI.SetLayerHealthFillAmount(_startingHealth,_health);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickHit, transform.position);
    }

    void HandleDeath()
    {
       GlobalFeedbackManager.Instance.SetSizeCapForBrickDestroy();
       GlobalFeedbackManager.Instance.PlayGlobalFeedback?.Invoke();

        abilityManager.NotifyBrickDestroyed(this);
        _brickPool.RemoveActiveBrick(this.gameObject);
        Instantiate(_destroyParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickDestroy, transform.position);

        pendingDeath = false;
        gameObject.SetActive(false);
    }
    void HandleDeathWithotSpawningEssence()
    {
        _statuses.Clear();
        RemoveAllModifiers();
        abilityManager.NotifyBrickDestroyed(this);
        _brickGenerator.OnBrickDestroyed();
        _brickPool.RemoveActiveBrick(this.gameObject);
        Instantiate(_destroyParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickDestroy, transform.position);
        pendingDeath = false;
        gameObject.SetActive(false);
    }
    void HandleDeathByPaddle()
    {
        abilityManager.NotifyBrickDestroyed(this);
        _brickPool.RemoveActiveBrick(this.gameObject);
        Instantiate(_destroyParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickDestroy, transform.position);

        pendingDeath = false;
        gameObject.SetActive(false);
    }
    void SpawnEssence()
    {
        int essencetoSpawn = UnityEngine.Random.Range(_essenceMinAmountToSpawn, _essenceMaxAmountToSpawn);
        for (int i = 0; i < essencetoSpawn; i++)
        {
            GameObject essence = _essencePool.GetEssence();
            essence.transform.position = transform.position;
        }
    }

    public void ApplyStatus(AbilityContext _statusEffect)
    {
        //check if status already exist
        if (_statuses.TryGetValue(_statusEffect._statusType, out StatusInstance existing))
        {

            if (existing.stacks >= (int)_statusEffect._Stats[STATID.MAX_STACKS])
                existing.stacks = (int)_statusEffect._Stats[STATID.MAX_STACKS];
            else
                existing.stacks += (int)_statusEffect._Stats[STATID.STACKS_TO_ADD];

            if(existing.resetStackLifeTimeUponHit)
            {
                existing.remainingStackTime = existing.stackLifeTime;
            }

            if (existing.affectsSpeed)
                MarkSpeedDirty();


            return;
        }
        else
        {
            StatusInstance sinst = new StatusInstance
            {
                _ability = _statusEffect._abililty,
                type = _statusEffect._statusType,
                stacks = (int)_statusEffect._Stats[STATID.STACKS_TO_ADD],
                maxStacks = (int)_statusEffect._Stats[STATID.MAX_STACKS],
                damagePerStack = (int)_statusEffect._Stats[STATID.DAMAGE_PER_STACK],
                stackLifeTime = (int)_statusEffect._Stats[STATID.STACK_LIFETIME],
                remainingStackTime = (int)_statusEffect._Stats[STATID.STACK_LIFETIME],
                timeBeforeEffect = (int)_statusEffect._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE],
                remainingEffectTime = (int)_statusEffect._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE],
                resetStackLifeTimeUponHit = _statusEffect._Statsbool[STATID.RESET_STACK_TIMER],
                spawnPrefab = _statusEffect._spawnPrefab,
                affectsSpeed = _statusEffect._Statsbool[STATID.AFFECTS_SPEED],
                speedMultiplier = _statusEffect._Stats[STATID.SPEED_MULTIPLIER]
            };


            _statuses.Add(_statusEffect._statusType, sinst);
            if (sinst.affectsSpeed)
                MarkSpeedDirty();
        }
    }
    public void SetBrick(SO_BrickHealthStats _stats)
    {
        _startingHealth = _stats._health;
        _health = _stats._health;
        _baseFallSpeed = _stats._dropSpeed;
        _fallSpeed = _stats._dropSpeed;
        _layer = _stats._layerNumber;
        _brickUI.SetCurrentLayer(_layer);
        _brickUI.PrepBrickLayerColour(_layer);
    }

    void MarkSpeedDirty() => _speedDirty = true;


    public void RecalculateSpeed()
    {
        float speedMultiplier = 0;

        foreach (var kvp in _statuses)
        {
            var status = kvp.Value;

            if (!status.affectsSpeed)
                continue;

            for (int i = 0; i < status.stacks; i++)
            {
                speedMultiplier += status.speedMultiplier;
            }
        }
        _fallSpeed = _baseFallSpeed * (1 - speedMultiplier);
    }
    //MODIFIERS
    public BrickModifierBase AddModifier(BrickModifierBase modifierPrefab)
    {
        if (modifierPrefab == null) return null;
        // instantiate as child of the brick (could come from a pool)
        var instance = Instantiate(modifierPrefab, transform.position, Quaternion.identity);
        instance.transform.SetParent(transform, false);
        instance.Initialize(this);
        _modifiers.Add(instance);
        return instance;
    }
    public void RemoveModifier(BrickModifierBase instance)
    {
        if (instance == null) return;
        if (_modifiers.Contains(instance))
        {
            _modifiers.Remove(instance);
            instance.OnRemove();
        }
    }
    public void RemoveAllModifiers()
    {
        // iterate copy to avoid modifying while iterating
        var copy = new List<BrickModifierBase>(_modifiers);
        foreach (var m in copy)
        {
            RemoveModifier(m);
        }
    }
    void TickModifiers(float dt)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            var m = _modifiers[i];
            if (m != null) m.Tick(dt);
        }
    }
    public int GetHealth() => _health;
}
