using System.Collections.Generic;
using UnityEngine;

public class ExplosionPool : MonoBehaviour
{
    public GameObject _explosionPrefab;

    public List<GameObject> _explosionPrefabPool = new List<GameObject>();

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

            _e.transform.parent = transform;
            _explosionPrefabPool.Add(_e);
            _e.SetActive(false);
        }
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
