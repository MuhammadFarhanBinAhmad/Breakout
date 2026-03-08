using Unity.VisualScripting;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{

    SOAbilityEffect _soAbilityEffect;
    int _damage;

    public void SetDamage(int d) => _damage = d;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<BrickBar>() != null)
        {
            other.GetComponent<BrickBar>().OnDamage(_damage);
        }
    }
}
