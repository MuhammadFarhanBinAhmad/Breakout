using System;
using Unity.Mathematics;
using UnityEngine;

public class Consistency : ABSAbility
{

    public int _currentCombo,_comboThreshold;
    int _baseDamageIncrease;

    public override void ModifyHit(HitContext ctx)
    {
        ctx._baseDamage += (int)_baseDamageIncrease;
    }
    public override void OnBallDestroy(Ball ball)
    {
        _currentCombo = 0;
        _baseDamageIncrease = 0;
    }
    public override void OnBrickDestroy(BrickBar bar)
    {
        _currentCombo++;
        if (_currentCombo >= _comboThreshold)
        {
            _baseDamageIncrease += (int)_SOAbilityEffect._baseDamageValue;
            _currentCombo = 0;
        }
    }
}
