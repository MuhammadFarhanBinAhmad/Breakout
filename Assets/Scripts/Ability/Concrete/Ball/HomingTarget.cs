using UnityEngine;

public class HomingTarget : ABSAbility
{
    public float _increaseHomingValue;
    private void Start()
    {
        _ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
    }
    public override void ModifyHit(HitContext ctx)
    {
        var val = GetStat(UPGRADETARGET.VALUECHANGE).GetValue(UPGRADETARGET.VALUECHANGE);

        if(_ball == null)
        {
            _ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<Ball>();
            ModifyHit(ctx);
        }
        else
            _ball.SetHomingValue(val);


    }
}
