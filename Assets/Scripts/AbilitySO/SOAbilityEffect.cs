using System.Collections.Generic;
using UnityEngine;
public enum ABILITYTYPE
{
    BALL,
    PASSIVE,
    ITEM
}
public enum BRICKABILITYTYPE
{
    NONE,
    CRIT,
    EXPLOSIVE,
    TOXIC,
    LIGHTNING
}


[CreateAssetMenu(menuName = "Ability/Ability Effect")]
public class SOAbilityEffect : ScriptableObject
{
    public string _abilityName;

    public ABILITYTYPE _abilityType;


    [Header("Runtime")]
    public GameObject _abilityPrefab;
    public List<SOAbilityEffect> _abilitiesChild;
    public SOAbilityEffect _abilityToRemove;

    public bool _genericEffect;
    public bool _applyStatus;
    public bool _spawnEffect;
    public bool _critEffect;

    [GroupUnder(nameof(_genericEffect))]
    public float _baseValue;//Value of abiltity effect to change. Be use to replace, add,minus, etc(eg.thershold, combo, etc.)
    [GroupUnder(nameof(_genericEffect))]
    public float _baseDamageValue;
    [GroupUnder(nameof(_genericEffect))]
    public float _baseDamageMultiplier;//Value of abiltity effect to change. Be use to replace, add,minus,etc. Is multiplier(eg.thershold, base damage, etc.)
    [GroupUnder(nameof(_genericEffect))]
    public float _bonusPerFail;
    [GroupUnder(nameof(_genericEffect))]
    public float _scaleSizeMultiplier;
    [GroupUnder(nameof(_applyStatus))]
    public float _effectDuration;
    [GroupUnder(nameof(_applyStatus))]
    public int _statusDamage;
    [GroupUnder(nameof(_applyStatus))]
    public int _maxStack;
    [GroupUnder(nameof(_spawnEffect))]
    public int _amountToSpawn;
    [GroupUnder(nameof(_critEffect))]
    public float _baseCritChance;
    [GroupUnder(nameof(_critEffect))]
    public float _critMultiplier;
}
