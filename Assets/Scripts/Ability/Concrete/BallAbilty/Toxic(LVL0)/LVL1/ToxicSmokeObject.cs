using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicSmokeObject : ABSAbility
{
    private class TargetData
    {
        public float timer;
    }

    private readonly Dictionary<BrickBar, TargetData> _targets = new Dictionary<BrickBar, TargetData>();
    private readonly List<BrickBar> _targetsToRemove = new List<BrickBar>();

    [SerializeField] private float _timeBeforeDespawn = 3f;
    [SerializeField] private float _shrinkDuration = 0.25f;
    [SerializeField] private float _damageTimeInterval = 0.5f;

    private Vector3 _startScale;
    private Coroutine _despawnRoutine;

    private void Awake()
    {
        if (!_abilityManager)
            _abilityManager = FindAnyObjectByType<AbilityManager>();
    }

    private void OnEnable()
    {
        _startScale = transform.localScale;

        if (_despawnRoutine != null)
            StopCoroutine(_despawnRoutine);

        _despawnRoutine = StartCoroutine(DespawnAfterDelay());
    }

    private void OnDisable()
    {
        _targets.Clear();
        _targetsToRemove.Clear();

        if (_despawnRoutine != null)
        {
            StopCoroutine(_despawnRoutine);
            _despawnRoutine = null;
        }

        transform.localScale = _startScale;
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(_timeBeforeDespawn);

        if (isActiveAndEnabled)
            yield return StartCoroutine(ShrinkAndDisable());
    }

    private IEnumerator ShrinkAndDisable()
    {
        float time = 0f;

        while (time < _shrinkDuration)
        {
            float t = time / _shrinkDuration;
            transform.localScale = Vector3.Lerp(_startScale, Vector3.zero, t);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_targets.Count == 0)
            return;

        float dt = Time.deltaTime;
        _targetsToRemove.Clear();

        foreach (var kvp in _targets)
        {
            BrickBar bb = kvp.Key;
            TargetData data = kvp.Value;

            if (bb == null || !bb.isActiveAndEnabled)
            {
                _targetsToRemove.Add(bb);
                continue;
            }

            data.timer += dt;

            while (data.timer >= _damageTimeInterval)
            {
                data.timer -= _damageTimeInterval;
                ApplyToxicTo(bb);
            }
        }

        for (int i = 0; i < _targetsToRemove.Count; i++)
        {
            if (_targetsToRemove[i] != null)
                _targets.Remove(_targetsToRemove[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out BrickBar target))
            return;

        if (!_targets.ContainsKey(target))
            _targets.Add(target, new TargetData());

        ApplyToxicTo(target);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out BrickBar target))
            _targets.Remove(target);
    }

    private void ApplyToxicTo(BrickBar target)
    {
        if (_abilityManager == null || target == null || _SOAbilityEffect == null)
            return;

        ToxicContext statusCtx = CreateToxicContext();
        _abilityManager.ApplyToxicModifiers(statusCtx);
        target.ApplyStatus(statusCtx);
    }

    private ToxicContext CreateToxicContext()
    {
        ToxicContext statusCtx = new ToxicContext
        {
            _abililty = this,
            _statusType = _SOAbilityEffect._statusType
        };

        statusCtx._Stats[STATID.STACKS_TO_ADD] = _SOAbilityEffect._stacksToAdd;
        statusCtx._Stats[STATID.MAX_STACKS] = _SOAbilityEffect._maxStacks;
        statusCtx._Stats[STATID.DAMAGE_PER_STACK] = _SOAbilityEffect._damagePerStack;
        statusCtx._Stats[STATID.STACK_LIFETIME] = _SOAbilityEffect._stackLifeTime;
        statusCtx._Stats[STATID.TIME_BEFORE_EFFECT_ACTIVATE] = _SOAbilityEffect._timeBeforeEffectActivate;
        statusCtx._Stats[STATID.SPEED_MULTIPLIER] = _SOAbilityEffect._speedMultiplier;

        statusCtx._Statsbool[STATID.RESET_STACK_TIMER] = _SOAbilityEffect._resetStackTimer;
        statusCtx._Statsbool[STATID.AFFECTS_SPEED] = _SOAbilityEffect._affectSpeed;

        return statusCtx;
    }
}