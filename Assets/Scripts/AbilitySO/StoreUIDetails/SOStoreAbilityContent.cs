using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Ability/Store Ability Content")]
public class SOStoreAbilityContent : ScriptableObject
{
    public string abilityID;
    public string ability_Name;
    [TextArea] public string ability_Description;
    public int ability_Level;
    public SOAbilityEffect ability_ToSpawn;
    public Sprite icon;
    public bool _availableToPurchaseAtStart;

    [Header("Requirements")]
    public List<string> requiredAbilityIDs = new List<string>();
}
