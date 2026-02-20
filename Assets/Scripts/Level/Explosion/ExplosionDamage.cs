using Unity.VisualScripting;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour
{

    SOAbilityEffect _soAbilityEffect;
    public int _damage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<BrickBar>() != null)
        {
            print("damage");
            other.GetComponent<BrickBar>().OnDamage(_damage);
        }
    }
}
