using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class HotZoneArea : MonoBehaviour
{
    Dictionary<BrickBar,float> _targets = new Dictionary<BrickBar, float>();
    float _damageTimeInterval;
    [SerializeField] float _timeBeforeDespawn;
    int _damage;
    [SerializeField] float _shrinkDuration;
    Vector3 _startScale;

    [SerializeField]List<GameObject> _fireParticle = new List<GameObject>();


    internal bool _isCombustion;

    private void OnEnable()
    {
        Invoke("SetActiveFalseSelf",_timeBeforeDespawn);
        _startScale = transform.localScale;

    }
    private void OnDisable()
    {
        foreach (GameObject obj in _fireParticle)
            obj.transform.localScale = _startScale;
    }
    void SetActiveFalseSelf()
    {
        if(isActiveAndEnabled)
        StartCoroutine(ShrinkAndDisable());
    }
    IEnumerator ShrinkAndDisable()
    {
        float time = 0f;

        while (time < _shrinkDuration)
        {
            float t = time / _shrinkDuration;

            foreach (GameObject obj in _fireParticle)
            {
                obj.transform.localScale = Vector3.Lerp(_startScale, Vector3.zero, t);
            }

            time += Time.deltaTime;
            yield return null;
        }
        foreach (GameObject obj in _fireParticle)
        {
            obj.transform.localScale = Vector3.zero;

        }
        SetCombustion(false);
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        var target = other.GetComponent<BrickBar>();
        if (target != null && !_targets.ContainsKey(target))
            _targets.Add(target, 0f);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        var targets = other.GetComponent<BrickBar>();
        if(targets != null) 
        _targets.Remove(targets);
    }
    public void SetStats(int d, float t, float scale = 1)
    {
        _damage = d;
        _damageTimeInterval = t;
        transform.localScale = transform.localScale * scale;
    }

    public void SetCombustion(bool set) => _isCombustion = set; 
    private void Update()
    {
        List<BrickBar> _key = new List<BrickBar>(_targets.Keys);

        foreach(var bb in _key)
        {
            if(bb == null)
            {
                _targets.Remove(bb);
                continue;
            }

            _targets[bb] += Time.deltaTime;

            if (_targets[bb] >= _damageTimeInterval)
            {
                if(!_isCombustion)
                {
                    _damageTimeInterval = 0;
                    bb.OnDamage(_damage);
                }
                else
                {
                    bb.OnDamage(999);
                    //Add spawn explosion here
                }
            }
        }
    }
}
