using System;
using UnityEngine;

public class MiniBomb : MonoBehaviour
{
    [SerializeField] Rigidbody2D _rb;
    MiniBombAbility _miniBombAbility;
    ExplosionPool _explosionPool;

    int _explosionDamage;
    [SerializeField] float _damageMultiplier;
    [SerializeField] float minImpulse;
    [SerializeField] float maxImpulse;
    [SerializeField] float _timeTillExplode;

    public Action _OnExplode;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _explosionPool = FindAnyObjectByType<ExplosionPool>();

        _OnExplode += Explode;
    }
    private void OnDestroy()
    {
        _OnExplode -= Explode;
    }

    void CheckIfMiniBombAbilityIsNull()
    {
        if (_miniBombAbility == null)
            _miniBombAbility = FindAnyObjectByType<MiniBombAbility>();
        else
            return;
    }
    public void SetStats(int dmg)
    {
        CheckIfMiniBombAbilityIsNull();

        Vector3 dir = UnityEngine.Random.onUnitSphere; // uniform random direction
        float mag = UnityEngine.Random.Range(minImpulse, maxImpulse);
        _rb.AddForce(dir * mag, ForceMode2D.Impulse);
        _explosionDamage = Mathf.FloorToInt((float)dmg * _damageMultiplier);
        Invoke("InvokeOnExplode", _timeTillExplode);
    }
    public void InvokeOnExplode() => _OnExplode?.Invoke();
    void Explode()
    {
        GameObject exp = _explosionPool.GetSmallExplosion();
        exp.transform.position = transform.position;
        ExplosionDamage ed = exp.GetComponent<ExplosionDamage>();
        //Set context of explosion to pass to explosion object
        if (ed != null)
        {
            ExplosionContext ctx = new ExplosionContext
            {
                _damage = _explosionDamage,
                _source = gameObject,
                _position = transform.position,
            };
            ed.Initialize(ctx);
        }

        if(_explosionDamage > 2)
        {
            _miniBombAbility.SpawnMiniBomb(transform.position, _explosionDamage);
        }

        gameObject.SetActive(false);
    }

    public Transform GetTransformPosition() => transform;
}
