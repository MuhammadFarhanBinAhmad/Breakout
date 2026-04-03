using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class ClusterBomb : MonoBehaviour
{

    ExplosionPool _explosionPool;

    [SerializeField] Rigidbody2D _rb;

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
    public void SetStats(int dmg)
    {
        Vector3 dir = UnityEngine.Random.onUnitSphere; // uniform random direction
        float mag = UnityEngine.Random.Range(minImpulse, maxImpulse);
        _rb.AddForce(dir * mag, ForceMode2D.Impulse);
        _explosionDamage = Mathf.FloorToInt((float)dmg * _damageMultiplier);
        StartCoroutine(StartExplodeTimer());
    }

    IEnumerator StartExplodeTimer()
    {
        yield return new WaitForSeconds(_timeTillExplode);
        InvokeOnExplode();
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
                _source = null,
                _position = transform.position,
            };
            ed.Initialize(ctx);
        }

        GlobalGameplayEventManager.OnClusterBombExplode?.Invoke(new ExplosionContext
        {
            _position = transform.position,
            _damage = _explosionDamage,
            _source = null,
        });
        print("hit");
        this.gameObject.SetActive(false);

    }
    public Transform GetTransformPosition() => transform;
}
