using NUnit.Framework.Constraints;
using UnityEngine;

public class MineBombAbility : ABSAbility
{
    MineBombPool _mineBombPool;

    private void Start()
    {
        _mineBombPool = FindAnyObjectByType<MineBombPool>();
    }

    public override void OnHit(HitContext ctx)
    {
        for (int i = 0; i < _SOAbilityEffect._amountToSpawn; i++)
        {
            GameObject explosion = _mineBombPool.GetObject();
            explosion.transform.position = ctx._brick.transform.position;
            MineBomb mb = explosion.GetComponent<MineBomb>();
            mb.SetStats();
        }

    }
}
