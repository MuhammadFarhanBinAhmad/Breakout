using UnityEngine;

public class SharpnelAbility : ABSAbility
{
    SharpnelPool _sharpnelPool;
    
    private void Start()
    {
        _sharpnelPool = FindAnyObjectByType<SharpnelPool>();

    }
    public override void OnHit(HitContext ctx)
    {
        for (int i=0; i < _SOAbilityEffect._amountToSpawn; i++)
        {
            GameObject explosion = _sharpnelPool.GetObject();
            explosion.transform.position = ctx._brick.transform.position;
            explosion.GetComponent<SharpnelBits>().SetStats();
        }

    }
}
