using Unity.VisualScripting;
using UnityEngine;

public class MiniBombAbility : ABSAbility
{
    ClusterBombAbility cluster;
    MiniBombPool _miniBombPool;

    public override void OnAdded(AbilityManager manager)
    {
        base.OnAdded(manager);

        cluster = manager.GetAbility<ClusterBombAbility>("cluster_bomb");
        _miniBombPool = FindAnyObjectByType<MiniBombPool>();

    }
    private void OnEnable()
    {
        GlobalGameplayEventManager.OnClusterBombExplode += OnExplosion;
    }

    private void OnDisable()
    {
        GlobalGameplayEventManager.OnClusterBombExplode -= OnExplosion;
    }

    //spawn first wave of mini bomb
    void OnExplosion(ExplosionContext e)
    {
        SpawnMiniBomb(e._position, e._damage);
    }

    public void SpawnMiniBomb(Vector2 pos, int damage)
    {
        for (int i = 0; i < _SOAbilityEffect._amountToSpawn; i++)
        {
            GameObject minibomb = _miniBombPool.GetObject();
            minibomb.transform.position = pos;
            MiniBomb mb = minibomb.GetComponent<MiniBomb>();
            mb.SetStats(damage);
        }
    }
}
