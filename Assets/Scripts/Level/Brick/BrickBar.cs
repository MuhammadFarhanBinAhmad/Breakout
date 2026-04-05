using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
public enum StatusType
{
    NULL,
    POISON,
    BURN,
    DISCHARGE,
    STUN
}

[System.Serializable]
public class StatusInstance
{
    public StatusType type;

    public int stacks;
    public int maxStacks;

    public int damagePerStack;

    public float effectDuration;
    public float remainingEffectTime;
}
public class BrickBar : MonoBehaviour
{

    Dictionary<StatusType, StatusInstance> _statuses = new Dictionary<StatusType, StatusInstance>();
    List<StatusType> toRemove = new List<StatusType>();

    public List<BrickModifierBase> _modifiers = new List<BrickModifierBase>();

    BrickPool _brickPool;
    EssencePool _essencePool;

    BrickGenerator _brickGenerator;
    TowerManager _towerManager;
    AbilityManager abilityManager;
    //UIManager

    SpriteRenderer _spriteRenderer;

    public GameObject _destroyParticleEffect;

    [Header("BrickStats")]
    internal int _startingHealth;
    public int _health;
    public int _shield;
    public float _tickTimer;
    float _startFallSpeed;
    public float _fallSpeed;

    [Header("Essence")]
    public int _essenceMinAmountToSpawn;
    public int _essenceMaxAmountToSpawn;

    List<SpriteRenderer> _spritesRenderer = new List<SpriteRenderer>();

    bool pendingDeath;

    public Action _onDeath;
    public Action _onDeathByPaddle;
    public Action _onDeathByTower;

    private void Awake()
    {
        _brickGenerator = FindAnyObjectByType<BrickGenerator>();
        abilityManager = FindAnyObjectByType<AbilityManager>();
        _towerManager = FindAnyObjectByType<TowerManager>();

        foreach (Transform child in transform)
        {
            var sr = child.GetComponent<SpriteRenderer>();
            if (sr != null)
                _spritesRenderer.Add(sr);
        }

        _brickPool = FindAnyObjectByType<BrickPool>();
        _essencePool = FindAnyObjectByType<EssencePool>();

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

        _startFallSpeed = _fallSpeed;
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


        if (pendingDeath)
            _onDeath?.Invoke();

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

            if(status.type == StatusType.STUN)
            {
                _fallSpeed = 0;
            }
            else
            {
                // DOT tick
                _tickTimer += dt;
                if (_tickTimer >= 1f)
                {
                    _tickTimer -= 1f;
                    OnDamage(status.stacks * status.damagePerStack);
                }
            }

            // Stack effect timer
            status.remainingEffectTime -= dt;

            if (status.remainingEffectTime <= 0f)
            {
                status.stacks--;
                print("Stack: " + status.stacks);

                if (status.stacks <= 0)
                {
                    toRemove.Add(kvp.Key);
                }
                else
                {
                    // Restart decay timer for next stack
                    status.remainingEffectTime = status.effectDuration;
                }
            }
        }

        //remove all completed status effect
        foreach (var key in toRemove)
        {
            if (key == StatusType.STUN)
            {
                _fallSpeed = _startFallSpeed;
            }
            _statuses.Remove(key);
        }
    }
    public void OnDamage(int dmg)
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

        print("DAMAGE: " + modified);

        _health -= modified;

        for (int i = 0; i < _modifiers.Count; i++)
            _modifiers[i]?.OnDamageApplied(modified);

        if (_health <= 0)
        {
            pendingDeath = true;
        }
        else
        {
            AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickHit, transform.position);
        }
    }

    void HandleDeath()
    {

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

    public void ApplyStatus(SOStatusEffect _statusEffect)
    {
        //check if status already exist2
        if (_statuses.Count > 0)
        {
            foreach (StatusType st in _statuses.Keys)
            {
                if (_statusEffect._statusType == st)
                {
                    _statuses[st].stacks += _statusEffect._stacksToAdd;
                    if (_statuses[st].stacks > _statuses[st].maxStacks)
                        _statuses[st].stacks = _statuses[st].maxStacks;

                    print(gameObject.name + " Stack = " + _statuses[st].stacks);
                    return;
                }
            }
        }

        StatusInstance status = new StatusInstance
        {
            type = _statusEffect._statusType,
            stacks = 1,
            maxStacks = _statusEffect._maxStacks,
            damagePerStack = _statusEffect._damagePerStack,
            effectDuration = _statusEffect._effectDuration,
        };

        _statuses.Add(_statusEffect._statusType, status);

        // Increase stacks (cap at max)
        status.stacks = Mathf.Min(
            status.stacks + _statusEffect._stacksToAdd,
            status.maxStacks
        );

        // Refresh decay timer on every hit
        status.remainingEffectTime = status.effectDuration;

        print(gameObject.name + " :new status added");
        print(status.stacks + " :stack");

    }
    public void ChangeSpiteColour(Color color)
    {
        for(int i=0; i< _spritesRenderer.Count; i++)
            _spritesRenderer[i].color = color;
    }
    public void SetBrick(SO_BrickHealthStats _stats)
    {
        _startingHealth = _stats._health;
        _health = _stats._health;
        _fallSpeed = _stats._dropSpeed;
        ChangeSpiteColour(_stats._color);
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
    public void Heal(int amount) => _health = Mathf.Min(_health + amount, _startingHealth);

}
