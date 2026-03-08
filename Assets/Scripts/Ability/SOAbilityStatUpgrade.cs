using System;
using System.Collections.Generic;
using UnityEngine;

public enum GROWTHMODE
{
    ADDITIVE,
    MULTIPLICATIVE
}

[System.Serializable]
public class UpgradeTarget
{
    public bool _subtract;

    [Header("Initial values (level 0)")]
    public float initialBaseValue;
    public float initialMaxDelta;

    [Header("Per-level growth (optional)")]
    [Tooltip("If GrowthMode is ADDITIVE, this value is added to baseValue each level.\n" +
             "If MULTIPLICATIVE, this is treated as a fractional percent (0.1 = +10% per level).")]
    public float baseValueMultiplierPerLevel;

    [Tooltip("Same semantics as baseValuePerLevel but for maxDelta.")]
    public float maxDeltaMultiplierPerLevel;

    public GROWTHMODE growthMode;
}


[CreateAssetMenu(menuName = "Ability/Stat Upgrade")]
public class SOAbilityStatUpgrade : ScriptableObject
{
    [Header("Identity")]
    public BRICKABILITYTYPE _brickAbilityParent;
    public string displayName;
    [TextArea] public string description;
    public List<UpgradeTarget> statKey = new List<UpgradeTarget>();
    public Sprite icon;

    [Header("Progression settings")]
    public float alpha;  // diminishing returns control

    [Header("Cost Progression")]
    public bool hasMax;
    public int maxLevel;
    public int baseCost;
    public float costGrowth;
}
