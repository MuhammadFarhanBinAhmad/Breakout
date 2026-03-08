using Unity.Mathematics.Geometry;
using UnityEngine;

public class SharpnelBits : MonoBehaviour
{
    [SerializeField]Ball _ball;
    [SerializeField]Rigidbody2D _rb;

    int _damage;
    [SerializeField] float _damageMultiplier;
    [SerializeField] float _lifetime;
    [SerializeField] float minImpulse;
    [SerializeField] float maxImpulse;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _ball = FindAnyObjectByType<Ball>();
    }
    public void SetStats()
    {
        Vector3 dir = Random.onUnitSphere; // uniform random direction
        float mag = Random.Range(minImpulse, maxImpulse);
        _rb.AddForce(dir * mag, ForceMode2D.Impulse);
        _damage = Mathf.FloorToInt((float)_ball.GetBallBaseDamage() * _damageMultiplier);
        Invoke("KillObject", _lifetime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        BrickBar bb = other.GetComponent<BrickBar>();
        if (bb != null)
        {
            print("Hit brick");
            bb.OnDamage(_damage);
            CancelInvoke();
            KillObject();
        }
    }
    void KillObject() => gameObject.SetActive(false);
}
