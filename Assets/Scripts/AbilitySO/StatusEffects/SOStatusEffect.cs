using UnityEngine;

[CreateAssetMenu(fileName = "SOStatusEffect", menuName = "Ability/Status Effect")]
public class SOStatusEffect : ScriptableObject
{
    public int _stacksToAdd,_damagePerStack,_maxStacks;
    public float _stackLifeTime, _timeBeforeEffectActivate;
    //_effectStackDuration - How long a stack last
    //_timeBeforeEffect - effect rate. How long it take before effect damage enemy
}
