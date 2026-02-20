using System.Collections.Generic;
using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
    public GameObject _explosionPrefab,_smallExplosionPrefab;

    public List<GameObject> _explosionPrefabPool = new List<GameObject>();
    public List<GameObject> _smallExplosionPrefabPool = new List<GameObject>();

    [SerializeField] int _explosionToSpawn;

    private void OnEnable()
    {
        SpawnAllExplosion();
    }
    void SpawnAllExplosion()
    {
        for (int i = 0; i < _explosionToSpawn; i++)
        {
            GameObject _e = Instantiate(_explosionPrefab, transform);
            GameObject _se = Instantiate(_smallExplosionPrefab, transform);

            _e.transform.parent = transform;
            _explosionPrefabPool.Add(_e);
            _e.SetActive(false);

            _se.transform.parent = transform;
            _smallExplosionPrefabPool.Add(_se);
            _se.SetActive(false);
        }
    }

    public GameObject GetSmallExplosion()
    {
        foreach (var se in _smallExplosionPrefabPool)
        {
            if (!se.activeSelf)
            {
                se.SetActive(true);
                return se;
            }
        }
        return null;
    }
    public GameObject GetExplosion()
    {
        foreach (var e in _explosionPrefabPool)
        {
            if (!e.activeSelf)
            {
                e.SetActive(true);
                return e;
            }
        }
        return null;
    }
}
