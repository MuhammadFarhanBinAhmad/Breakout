using System;
using Unity.Mathematics;
using UnityEngine;

public class Consistency : ABSAbility
{

    public int _currentCombo;
    int _baseDamageIncrease;

    public override void ModifyHit(HitContext ctx)
    {
        _baseDamageIncrease = (int)GetStat(UPGRADETARGET.VALUECHANGE).GetValue(UPGRADETARGET.VALUECHANGE);
        var threshold = GetStat(UPGRADETARGET.VALUECHANGE).GetValue(UPGRADETARGET.VALUECHANGE);
        print("threshold" + threshold);
        ctx._baseDamage += (int)_baseDamageIncrease * Math.Max(1,_currentCombo / (int)threshold);
        print(ctx._baseDamage);
    }
    public override void OnBallDestroy(Ball ball)
    {
        _currentCombo = 0;
    }
    public override void OnBrickDestroy(BrickBar bar)
    {
        _currentCombo++;

    }
}
