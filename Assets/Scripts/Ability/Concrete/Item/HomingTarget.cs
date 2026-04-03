using UnityEngine;

public class HomingTarget : ABSAbility
{
    private void Start()
    {
        _ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
    }
    public override void ModifyHit(HitContext ctx)
    {
        var val = _SOAbilityEffect._baseValue;

        if(_ball == null)
        {
            _ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
            ModifyHit(ctx);
        }
        else
            _ball.SetHomingValue(val);


    }
}
