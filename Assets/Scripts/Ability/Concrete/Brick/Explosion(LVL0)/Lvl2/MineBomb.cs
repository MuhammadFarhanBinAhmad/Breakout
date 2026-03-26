using UnityEngine;

public class MineBomb : MonoBehaviour
{

    ExplosionPool _explosionPool;

    [SerializeField] Ball _ball;
    [SerializeField] Rigidbody2D _rb;
    [SerializeField] LayerMask _targetLayers;

    int _damage;
    [SerializeField] float _damageMultiplier;
    [SerializeField] float minImpulse;
    [SerializeField] float maxImpulse;

    private void Awake()
    {
        _explosionPool = FindAnyObjectByType<ExplosionPool>();
        _rb = GetComponent<Rigidbody2D>();
        _ball = FindAnyObjectByType<Ball>();
    }
    public void SetStats()
    {
        Vector3 dir = Random.onUnitSphere; // uniform random direction
        float mag = Random.Range(minImpulse, maxImpulse);
        _rb.AddForce(dir * mag, ForceMode2D.Impulse);
        _damage = Mathf.FloorToInt((float)_ball.GetBallBaseDamage() * _damageMultiplier);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & _targetLayers) == 0)
            return;

        // check for brick or ball components (prefer TryGetComponent)
        if (collision.gameObject.TryGetComponent<BrickBar>(out var bb) ||
            collision.gameObject.TryGetComponent<Ball>(out var ball))
        {
            Explode();
        }
    }

    void Explode()
    {
        GameObject explosionGO = _explosionPool.GetExplosion();
        explosionGO.transform.position = transform.position;

        var ed = explosionGO.GetComponent<ExplosionDamage>();
        if (ed != null)
        {
            var ctx = new ExplosionContext
            {
                _damage = _damage,
                _source = gameObject,
                _position = transform.position
            };
            ed.Initialize(ctx);
        }

        gameObject.SetActive(false);
    }
}
