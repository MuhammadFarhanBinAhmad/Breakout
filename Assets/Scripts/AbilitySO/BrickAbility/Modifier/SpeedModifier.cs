using UnityEngine;

public class SpeedModifier : BrickModifierBase
{
    public float _speedModifier;
    public override void Initialize(BrickBar brick)
    {
        base.Initialize(brick);
        brick._fallSpeed += _speedModifier;
    }
}
