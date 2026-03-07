using System;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
public enum StatusType
{
    Poison,
    Burn,
    Discharge
}

[System.Serializable]
public class StatusInstance
{
    public StatusType type;

    public int stacks;
    public int maxStacks;

    public int damagePerStack;

    public float decayDuration;     // Time to lose ONE stack
    public float remainingTime;
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
    public float _fallSpeed;

    [Header("Essence")]
    public int _essenceMinAmountToSpawn;
    public int _essenceMaxAmountToSpawn;

    List<SpriteRenderer> _spritesRenderer = new List<SpriteRenderer>();

    bool pendingDeath;

    public Action _onDeath;
    public Action _onDeathByPaddle;

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

        _onDeathByPaddle += HandleDeathByPaddle;
    }
    private void OnDestroy()
    {
        _onDeath -= HandleDeath;

        _onDeathByPaddle -= HandleDeathByPaddle;
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

            // DOT tick
            _tickTimer += dt;
            if (_tickTimer >= 1f)
            {
                _tickTimer -= 1f;
                OnDamage(status.stacks * status.damagePerStack);
            }

            // Stack decay timer
            status.remainingTime -= dt;

            if (status.remainingTime <= 0f)
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
                    status.remainingTime = status.decayDuration;
                }
            }
        }

        foreach (var key in toRemove)
        {
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
        _statuses.Clear();
        RemoveAllModifiers();

        abilityManager.NotifyBrickDestroyed(this);
        _brickGenerator.OnBrickDestroyed();
        _brickPool.RemoveActiveBrick(this.gameObject);
        SpawnEssence();
        Instantiate(_destroyParticleEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickDestroy, transform.position);

        pendingDeath = false;
        gameObject.SetActive(false);
    }
    void HandleDeathByPaddle()
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
    void SpawnEssence()
    {
        int essencetoSpawn = UnityEngine.Random.Range(_essenceMinAmountToSpawn, _essenceMaxAmountToSpawn);
        for (int i = 0; i < essencetoSpawn; i++)
        {
            GameObject essence = _essencePool.GetEssence();
            essence.transform.position = transform.position;
        }
    }

    public void ApplyStatus(StatusType type,
    int stacksToAdd,
    int damagePerStack,
    float decayDuration,
    int maxStacks)
    {
        //check if status already exist
        if (_statuses.Count > 0)
        {
            foreach (StatusType st in _statuses.Keys)
            {
                if (type == st)
                {
                    _statuses[st].stacks += stacksToAdd;
                    if (_statuses[st].stacks > _statuses[st].maxStacks)
                        _statuses[st].stacks = _statuses[st].maxStacks;

                    print(gameObject.name + " Stack = " + _statuses[st].stacks);
                    return;
                }
            }
        }

        StatusInstance status = new StatusInstance
        {
            type = type,
            stacks = 1,
            maxStacks = maxStacks,
            damagePerStack = damagePerStack,
            decayDuration = decayDuration,
        };

        _statuses.Add(type, status);

        // Increase stacks (cap at max)
        status.stacks = Mathf.Min(
            status.stacks + stacksToAdd,
            status.maxStacks
        );

        // Refresh decay timer on every hit
        status.remainingTime = status.decayDuration;

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
