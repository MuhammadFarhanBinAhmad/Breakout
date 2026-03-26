using UnityEngine;

[CreateAssetMenu(fileName = "SOStatusEffect", menuName = "Ability/Status Effect")]
public class SOStatusEffect : ScriptableObject
{
    public StatusType _statusType;
    public int _stacksToAdd,_damagePerStack,_maxStacks;
    public float _effectDuration;
}
