using System.Collections.Generic;
using UnityEngine;

public abstract class VBasePool : MonoBehaviour, IBasePool
{
    [SerializeField] GameObject go_ObjectToSpawn;
    [SerializeField] int _amountToSpawn;

    [SerializeField]List<GameObject> _objectPool = new List<GameObject>();

    private void Start()
    {
        SpawnObject();
    }

    public virtual GameObject GetObject()
    {
        foreach (var e in _objectPool)
        {
            if (!e.activeSelf)
            {
                e.SetActive(true);
                return e;
            }
        }
        return null;
    }

    public virtual void SpawnObject()
    {
        for (int i = 0; i < _amountToSpawn; i++)
        {
            GameObject _b = Instantiate(go_ObjectToSpawn, transform);
            _b.transform.parent = transform;
            _objectPool.Add(_b);
            _b.SetActive(false);
        }
    }
}
