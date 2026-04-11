using System;
using Unity.VisualScripting;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{
    public event Action<ExplosionContext> OnExploded;
    ExplosionContext _ctx;

    Vector3 _startScale;
    SOStatusEffect _statusEffect;
    int _damage;
    bool _hasExploded = false;

    [SerializeField] float _camShakeStrength;

    private void Start()
    {
        _startScale = new Vector3(1,1,1);
    }
    public void Initialize(ExplosionContext ctx)
    {
        _ctx = ctx;
        transform.position = _ctx._position;
        transform.localScale = _startScale  * _ctx._scaleMultiplier;
        _damage = ctx._damage;
        _statusEffect = ctx._statusEffect;
        _hasExploded = false;
        ExplodeNow();
    }

    public void ExplodeNow()
    {
        if (_hasExploded) return;
        _hasExploded = true;
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onBombExplosion,transform.position);
        GlobalFeedbackManager.Instance.PlayGlobalFeedback();
        OnExploded?.Invoke(_ctx);
        //ResetScale();
    }

    public void ResetScale() => transform.localScale = _startScale;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_ctx == null) return;

        var brick = other.GetComponent<BrickBar>();
        if (brick != null)
        {
            brick.OnDamage(_damage);

            if(_statusEffect != null)
            brick.ApplyStatus(_statusEffect);
        }
    }
}
