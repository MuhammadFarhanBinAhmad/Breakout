using System;
using System.Collections.Generic;
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

    Dictionary<StatusType, StatusInstance> _statuses =new Dictionary<StatusType, StatusInstance>();
    List<StatusType> toRemove = new List<StatusType>();

    BrickPool _brickPool;
    EssencePool _essencePool;

    BrickGenerator _brickGenerator;
    TowerManager _towerManager;
    AbilityManager abilityManager;
    MoneyManager _moneyManager;
    //UIManager
    MoneyUIManager _moneyUIManager;

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

    bool pendingDeath ;

    public Action _onDeath;
    public Action _onDeathByPaddle;

    private void Awake()
    {
        _brickGenerator = FindAnyObjectByType<BrickGenerator>();
        abilityManager = FindAnyObjectByType<AbilityManager>();
        _towerManager = FindAnyObjectByType<TowerManager>();
        _moneyManager = FindAnyObjectByType<MoneyManager>();
        _moneyUIManager = FindAnyObjectByType<MoneyUIManager>();

        _brickPool = FindAnyObjectByType<BrickPool>();
        _essencePool = FindAnyObjectByType<EssencePool>();

        _onDeath += HandleDeath;
        _onDeath += _moneyUIManager.UpdateMoneyUI;

        _onDeathByPaddle += HandleDeathByPaddle;
    }
    private void OnDestroy()
    {
        _onDeath -= HandleDeath;
        _onDeath -= _moneyUIManager.UpdateMoneyUI;

        _onDeathByPaddle -= HandleDeathByPaddle;
    }

    private void Update()
    {
        transform.Translate(Vector3.down * _fallSpeed * Time.deltaTime);
        if (_health > 0)
            ExecuteStatusEffect();

        if(pendingDeath)
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
        _health -= dmg;
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
        abilityManager.NotifyBrickDestroyed(this);
        _brickGenerator.OnBrickDestroyed();
        _moneyManager.CalculateBrickValue(this);
        _brickPool.RemoveActiveBrick(this.gameObject);
        SpawnEssence();
        Instantiate(_destroyParticleEffect,transform.position, Quaternion.identity);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_brickDestroy, transform.position);

        pendingDeath = false;
        gameObject.SetActive(false);
    }
    void HandleDeathByPaddle()
    {
        _statuses.Clear();
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
        for(int i = 0;i < essencetoSpawn; i++)
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
        if(_statuses.Count > 0)
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
}
