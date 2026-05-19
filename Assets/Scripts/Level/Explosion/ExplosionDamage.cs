using System;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    AbilityContext _ctx;

    Vector3 _startScale = Vector3.one;
    SOStatusEffect _SOStatusEffect;
    int _damage;
    bool _hasExploded = false;

    public GameObject [] effect ;


    public void Initialize(AbilityContext ctx,bool explodeNow)
    {
        _ctx = ctx;
        transform.position = _ctx._position;
        transform.localScale = _startScale  * _ctx._Stats[STATID.SCALE_MULTIPLIER];
        _damage = (int)ctx._Stats[STATID.BASE_DAMAGE];
        _SOStatusEffect = ctx._statusEffect;
        _hasExploded = false;
        foreach (var item in effect)
        {
            item.transform.localScale = transform.localScale;
        }
        if(explodeNow)
        ExplodeNow();
    }
    public void ExplodeNow()
    {
        if (_hasExploded) return;
        _hasExploded = true;
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onBombExplosion,transform.position);
        GlobalFeedbackManager.Instance.PlayGlobalFeedback();
        Invoke("DisableSelf", .1f);
    }
    void DisableSelf() => gameObject.SetActive(false);
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_ctx == null) return;

        var brick = other.GetComponent<BrickBar>();
        if (brick != null)
        {
            brick.OnDamage(_damage);

            if(_SOStatusEffect != null)
            {
                var statusCtx = new AbilityContext
                {
                };
                statusCtx._Stats[STATID.MAX_STACKS] = _SOStatusEffect._maxStacks;
                statusCtx._Stats[STATID.DAMAGE_PER_STACK] = _SOStatusEffect._damagePerStack;
                statusCtx._Stats[STATID.STACK_LIFETIME] = _SOStatusEffect._stackLifeTime;
                statusCtx._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE] = _SOStatusEffect._timeBeforeEffectActivate;
                brick.ApplyStatus(statusCtx);

            }
        }
    }
}
