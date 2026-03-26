using System;
using UnityEngine;

public class ClusterBombAbility : ABSAbility
{
    ClusterBombPool _clusterBombPool;

    private void Start()
    {
        _clusterBombPool = FindAnyObjectByType<ClusterBombPool>();

    }

    public override void OnHit(HitContext ctx)
    {
        for (int i = 0; i < _SOAbilityEffect._amountToSpawn; i++)
        {
            GameObject explosion = _clusterBombPool.GetObject();
            explosion.transform.position = ctx._brick.transform.position;
            ClusterBomb cb = explosion.GetComponent<ClusterBomb>();

            int _dmg = _ball.GetBallBaseDamage();
            cb.SetStats(_dmg);
        }

    }
}
