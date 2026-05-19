using System.Collections.Generic;
using UnityEngine;
public enum ABILITYTYPE
{
    BALL,
    PASSIVE,
    ITEM
}


[CreateAssetMenu(menuName = "Ability/Ability Effect")]
public class SOAbilityEffect : ScriptableObject
{
    public string _abilityName;

    public ABILITYTYPE _abilityType;
    public STATUSTYPE _statusType;

    [Header("Runtime")]
    public GameObject _abilityPrefab;
    public List<SOAbilityEffect> _abilitiesChild;
    public SOAbilityEffect _abilityToRemove;

    public bool _genericEffect;
    public bool _applyStatus;
    public bool _spawnEffect;
    public bool _critEffect;
    public bool _explosionEffect;
    //-----------------Generic-----------------//
    [GroupUnder(nameof(_genericEffect))]
    public float _baseValue;//Value of abiltity effect to change. Be use to replace, add,minus, etc(eg.thershold, combo, etc.)
    [GroupUnder(nameof(_genericEffect))]
    public float _baseDamageValue;
    [GroupUnder(nameof(_genericEffect))]
    public float _baseDamageMultiplier;//Value of abiltity effect to change. Be use to replace, add,minus,etc. Is multiplier(eg.thershold, base damage, etc.)
    [GroupUnder(nameof(_genericEffect))]
    public float _speedMultiplier;
    [GroupUnder(nameof(_genericEffect))]
    public float _bonusPerFail;
    [GroupUnder(nameof(_genericEffect))]
    public float _scaleSizeMultiplier;
    //-----------------Toxic-----------------//
    [GroupUnder(nameof(_applyStatus))]
    public int _maxStacks;
    [GroupUnder(nameof(_applyStatus))]
    public int _stacksToAdd;
    [GroupUnder(nameof(_applyStatus))]
    public int _damagePerStack;
    [GroupUnder(nameof(_applyStatus))]
    public float _stackLifeTime;
    [GroupUnder(nameof(_applyStatus))]
    public float _timeBeforeEffectActivate;
    [GroupUnder(nameof(_applyStatus))]
    public bool _resetStackTimer;
    [GroupUnder(nameof(_applyStatus))]
    public bool _affectSpeed;
    //-----------------Spawn-----------------//
    [GroupUnder(nameof(_spawnEffect))]
    public int _amountToSpawn;
    [GroupUnder(nameof(_spawnEffect))]
    public GameObject _itemToSpawn;
    //-----------------Crit-----------------//
    [GroupUnder(nameof(_critEffect))]
    public float _baseCritChance;
    [GroupUnder(nameof(_critEffect))]
    public float _critMultiplier;
    [GroupUnder(nameof(_critEffect))]
    public int _layerToDestroy;
    //-----------------Explosive-----------------//
    [GroupUnder(nameof(_explosionEffect))]
    public float _explosionDamageMultiplier;
    [GroupUnder(nameof(_explosionEffect))]
    public float _explosionSizeMultiplier;
}
