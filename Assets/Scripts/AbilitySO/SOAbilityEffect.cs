using System.Collections.Generic;
using UnityEngine;
public enum ABILITYTYPE
{
    NONE,
    BRICK,
    BALL,
    ENVIROMENT
}
public enum BRICKABILITYTYPE
{
    NONE,
    CRIT,
    EXPLOSIVE,
    TOXIC,
    LIGHTNING
}

public enum BALLABILITYTYPE
{
    NONE
}
public enum ENVIROMENTABILITYTYPE
{
    NONE
}

public enum UPGRADETARGET
{
    DAMAGEMULTIPLIER,
    EFFECTDURATION,
    STATUSDAMAGE,
    MAXSTACK,
    AMOUNTTOSPAWN,
    CRITCHANCE,
    CRITMULTIPLIER,
    VALUECHANGE
}


[CreateAssetMenu(menuName = "Ability/Effect")]
public class SOAbilityEffect : ScriptableObject
{
    public string _abilityName;
    public ABILITYTYPE _abilityType;

    public bool _isBallAbility;
    public bool _isBrickAbility;
    public bool _isEnviromentAbility;

    [GroupUnder(nameof(_isBallAbility))]
    public BALLABILITYTYPE _ballAbilityType;

    [GroupUnder(nameof(_isBrickAbility))]
    public BRICKABILITYTYPE _brickAbilityType;

    [GroupUnder(nameof(_isEnviromentAbility))]
    public ENVIROMENTABILITYTYPE _environmentAbilityType;

    [Header("BasicUpgrade")]
    public float _baseDamageMultiplier;


    [Header("Runtime")]
    public GameObject _abilityPrefab;

    public bool _applyStatus;
    public bool _spawnEffect;
    public bool damageModifier;

    [GroupUnder(nameof(_applyStatus))]
    public float _effectDuration;

    [GroupUnder(nameof(_applyStatus))]
    public int _statusDamage;

    [GroupUnder(nameof(_applyStatus))]
    public int _maxStack;



    [GroupUnder(nameof(_spawnEffect))]
    public int _amountToSpawn;

    [GroupUnder(nameof(damageModifier))]
    public float _baseCritChance;

    [GroupUnder(nameof(damageModifier))]
    public float _critMultiplier;

    [GroupUnder(nameof(damageModifier))]
    public float _bonusPerFail;


}
