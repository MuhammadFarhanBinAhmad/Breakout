
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum EASINGTYPE
{
    LINEAR,
    QUADRACTICOUT,
    QUARTICOUT
}

[System.Serializable]
public class AbilityStatRuntime
{

    public SOAbilityStatUpgrade definition;
    public int level;


    public void LevelUp()
    {
        if (definition != null && definition.hasMax && level >= definition.maxLevel)
        {
            Debug.LogWarning($"{definition.name} already at max level ({definition.maxLevel}).");
            return;
        }
        level++;


    }

    // ---------- Public API ----------

    public float GetValue(UPGRADETARGET target)
    {
        var entry = GetEntry(target, out int index);
        if (entry == null || index < 0) return 0f;

        return CalculateValue(
        entry.initialBaseValue,
        entry.initialMaxDelta,
        level,
        entry
    );
    }

    public int NextCost => GetCost(level);

    // ---------- Internal helpers ----------
    float ApplyEasing(float t, EASINGTYPE easing)
    {
        switch (easing)
        {
            case EASINGTYPE.LINEAR:
                return t;

            case EASINGTYPE.QUADRACTICOUT:
                return 1f - (1f - t) * (1f - t);

            case EASINGTYPE.QUARTICOUT:
                return 1f - Mathf.Pow(1f - t, 4);

            default:
                return t;
        }
    }

    UpgradeTarget GetEntry(UPGRADETARGET target, out int index)
    {
        for (int i = 0; i < definition.statKey.Count; i++)
        {
            if (definition.statKey[i]._statkey == target)
            {
                index = i;
                return definition.statKey[i];
            }
        }

        Debug.LogError($"Stat {target} not found in {definition.name}");
        index = -1;
        return null;
    }

    float CalculateValue(
        float baseValue,         // value stored on the SO at index
        float maxDelta,          // value stored on the SO at index
        int lvl,
        UpgradeTarget entry      // contains easing, subtract, growth settings
    )
    {
        // level 0 -> return baseValue (no growth applied)
        if (lvl <= 0) return baseValue;

        // --- scale baseValue and maxDelta according to the per-entry growth config ---
        float scaledBase = baseValue;
        float scaledMaxDelta = maxDelta;

        if (entry != null)
        {
            if (entry.growthMode == GROWTHMODE.ADDITIVE)
            {
                if(entry._subtract)
                {
                    scaledBase = baseValue - entry.baseValueMultiplierPerLevel * lvl;
                    scaledMaxDelta = maxDelta - entry.maxDeltaMultiplierPerLevel * lvl;
                }
                else
                {
                    // additive: add fixed amount per level
                    scaledBase = baseValue + entry.baseValueMultiplierPerLevel * lvl;
                    scaledMaxDelta = maxDelta + entry.maxDeltaMultiplierPerLevel * lvl;
                }

            }
            else // MULTIPLICATIVE
            {
                // multiplicative: treat perLevel as fractional percent (0.1 = +10% per level)
                scaledBase = baseValue * Mathf.Pow(1f + entry.baseValueMultiplierPerLevel, lvl);
                scaledMaxDelta = maxDelta * Mathf.Pow(1f + entry.maxDeltaMultiplierPerLevel, lvl);
            }
        }

        // progression parameter t (same diminishing-return formula you had)
        float t = (float)lvl / (lvl + Mathf.Max(0.0001f, definition.alpha));
        // easing uses the entry's easing type
        EASINGTYPE easing = entry != null ? entry._easingType : EASINGTYPE.QUARTICOUT;
        float eased = ApplyEasing(t, easing);

        float delta = scaledMaxDelta * eased;
        bool subtract = entry != null && entry._subtract;
        return subtract ? scaledBase - delta : scaledBase + delta;
    }

    int GetCost(int lvl)
    {
        return Mathf.RoundToInt(
            definition.baseCost * Mathf.Pow(definition.costGrowth, lvl)
        );
    }
}

public abstract class ABSAbility : MonoBehaviour
{
    protected Ball _ball;
    [SerializeField] protected AbilityManager _abilityManager;

    public SOAbilityEffect _SOAbilityEffect;
    public List<ABSAddOnAbility> _ABSAddOnAbilityList = new();

    [Header("Upgradable Stats (Runtime)")]
    [SerializeField]protected List<AbilityStatRuntime> _AbilityStatRuntime = new();
    
    protected virtual void Awake()
    {
        ValidateUniqueStats();
    }
    private void OnEnable()
    {
        _ball = FindAnyObjectByType<Ball>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
    }

    protected AbilityStatRuntime GetStat(UPGRADETARGET key)
    {
        foreach (var stat in _AbilityStatRuntime)
        {
            foreach (var entry in stat.definition.statKey)
            {
                if (entry._statkey == key)
                    return stat;
            }
        }

        Debug.LogError($"{name} missing stat for key {key}");
        return null;
    }
    public bool TryUpgradeStat(int index)
    {
        if (index < 0 || index >= _AbilityStatRuntime.Count)
        {
            Debug.LogWarning($"Invalid stat index {index}");
            return false;
        }

        var stat = _AbilityStatRuntime[index];
        stat.LevelUp();
        print(stat.definition._brickAbilityParent);
        _abilityManager.UpgradeAbilityTypeLevel(stat.definition._brickAbilityParent);
        
        //foreach (var key in stat.definition.statKey)
        //{
        //    float value = stat.GetValueToAdd(key);
        //    Debug.Log(
        //        $"{name} upgraded {key} " +
        //        $"Å® Level {stat.level}, Value {value}"
        //    );
        //}

        return true;
    }

    public IReadOnlyList<AbilityStatRuntime> Stats => _AbilityStatRuntime;

    // Called once when added
    public virtual void OnAdded(AbilityManager manager) { }

    public void AddAddOnAbility(SOAbilityEffect _effect)
    {
        print("add addon ability");
        if (_effect == null)
            return;

        GameObject go = Instantiate(_effect._abilityPrefab, transform);
        go.transform.SetParent(transform, true);

        var addon = go.GetComponent<ABSAddOnAbility>();
        if (addon != null)
        {
            _ABSAddOnAbilityList.Add(addon);
        }
    }

    // Brick-related hooks
    // Modify phase: abilities & their addons can modify the context.All damage calculation to be done here
    public virtual void ModifyHit(HitContext ctx)
    {
        ctx._finaleDamage += (int)((float)(ctx._baseDamage) * _SOAbilityEffect._baseDamageMultiplier);

        // default: allow addons to modify hit
        foreach (var addon in _ABSAddOnAbilityList)
            addon.OnModifyHit(ctx);

    }

    // Resolve/execute phase: the concrete ability may roll/apply damage,
    // and addons can react via OnHitResolved (called after resolve).
    public virtual void OnHit(HitContext ctx)
    {
        // default just call addons to react
        foreach (var addon in _ABSAddOnAbilityList)
            addon.OnHit(ctx);
    }
    public virtual void OnHitResolved(HitContext ctx)
    {
        foreach (var addon in _ABSAddOnAbilityList)
            addon.OnHitResolved(ctx);
    }
    public virtual void OnBrickDestroy(BrickBar bar) { }
    public virtual void OnBallDestroy(Ball ball) {
    }

    // Time-based hook
    public virtual void OnTick(float deltaTime) { }
    protected void ValidateUniqueStats()
    {
        var usedKeys = new HashSet<UPGRADETARGET>();

        foreach (var stat in _AbilityStatRuntime)
        {
            var def = stat.definition;
            if (def == null) continue;

            if (def.statKey == null || def.statKey.Count == 0)
            {
                Debug.LogError($"{def.name} has no UpgradeTarget entries.");
                continue;
            }

            foreach (var entry in def.statKey)
            {
                if (!usedKeys.Add(entry._statkey))
                {
                    Debug.LogError($"{name} has duplicate upgrade target {entry._statkey}");
                }
            }
        }
    }
}
