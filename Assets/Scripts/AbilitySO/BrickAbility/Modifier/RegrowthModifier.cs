using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class RegrowthModifier : BrickModifierBase
{
    public float _healInterval;
    public int _healAmount;

    float _timer;
    float _life;

    public override void Initialize(BrickBar brick)
    {
        base.Initialize(brick);
        _timer = 0;
        
    }

    public override void Tick(float dt)
    {
        _timer += dt;

        if (_timer > _healInterval)
        {
            if (_brick._health < _brick._startingHealth)
            {
                _brick._health += _healAmount;
            }
        }
    }

}
