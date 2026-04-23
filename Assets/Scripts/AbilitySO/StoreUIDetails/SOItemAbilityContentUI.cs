using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AbilityUI/Item Ability Content")]
public class SOItemAbilityContentUI : ScriptableObject
{
    public string ability_Name;
    [TextArea] public string ability_Description;
    public ITEMRARITY _itemRarity;
    public SOAbilityEffect ability_ToSpawn;
    public Sprite icon;
}
