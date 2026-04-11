using System;
using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    internal Rigidbody2D _rigidbody;
    SpriteRenderer _spriteRenderer;
    AbilityManager _abilityManager;
    BrickPool _brickPool;
    BallFeedbackManager _ballFeedbackManager;

    public float _gravityScale;
    public float _maxVelocity;

    public Action OnBallHit;
    public Action OnBallReset;
    public Action OnBallDestroy;//For copy

    public Action OnBrickHit;

    public Action OnUpgradeBallBaseDamage;
    public Action OnUpgradeBallReviveSpeed;


    [Header("Respawn")]
    public float _respawnTime;
    public Transform _respawnPos;

    [Header("Damage")]
    [SerializeField] int _baseDamage;
    internal int _damageValueModifier;
    [SerializeField] float _camShakeStrength;


    [Header("Combo")]
    [SerializeField] int _feverThreshold;
    internal int _currentCombo;
    [SerializeField] GameObject _particleTrail;

    [Header("CopyBall")]
    public int _maxBounce;
    internal bool _copyBall;
    int _currentBounce;

    [Header("Homing (subtle)")]
    public float _delayTimeAfterHit;
    public float _currentDelayTime;
    [Range(0f, 1f)]
    [SerializeField] float _homingStrength = 0.08f;
    [SerializeField] float _minVerticalForHoming = 0.25f;
    [SerializeField] float _homingMaxDistance = 12f;

    [Header("AimingState")]
    [SerializeField] float _lerpSpeed;
    [SerializeField] float _slowMotionTimeValue;
    [SerializeField] float _coolDownPeriod;
    [SerializeField] int _manaShootCost;
    float _currentTimeScale = 1f;
    float _targetTimeScale = 1f;
    float _currentTargetTimeScale;
    float _currentCoolDownPeriod;
    Coroutine _timeScaleRoutine;
    bool _onAimingState;

    [Header("ManaBar")]
    [SerializeField] float _maxManaAmount;
    [SerializeField] float _currentManaAmount;
    [SerializeField] float _manaRegenRate;

    [Header("RespawnAnimation")]
    [SerializeField] AnimationCurve easeOutElastic;
    [SerializeField] float animationDuration;
    [SerializeField] float _capscaleMultiplier;
    [SerializeField] Camera _gameCamera;
    [SerializeField] bool _enableMouseRedirect = true;
    Vector3 _startingScale;

    // -------------------------
    // Push lock (prevents immediate re-attraction)
    // -------------------------
    float pushLockTimer = 0f; // seconds remaining where attraction is ignored

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
        _brickPool = FindAnyObjectByType<BrickPool>();
        _ballFeedbackManager = FindAnyObjectByType<BallFeedbackManager>();

        OnBrickHit += IncreaseCombo;
        OnBrickHit += _ballFeedbackManager.UpdateGlowIntensity;

        OnBallReset += ResetCombo;
        OnBallReset += ResetPosition;
        OnBallReset += PlayBallDestroyAudio;

        OnBallDestroy += DestroyCopyBall;
        OnBallDestroy += PlayBallDestroyAudio;
    }
    private void Start()
    {
        // initial downward velocity - preserve your original intent
        _rigidbody.linearVelocity = Vector2.down * _gravityScale;

        _startingScale = transform.localScale;
        _currentManaAmount = _maxManaAmount;
    }
    private void OnDisable()
    {
        OnBrickHit -= IncreaseCombo;
        OnBrickHit -= _ballFeedbackManager.UpdateGlowIntensity;

        OnBallReset -= ResetCombo;
        OnBallReset -= ResetPosition;
        OnBallReset -= PlayBallDestroyAudio;

        OnBallDestroy -= DestroyCopyBall;
        OnBallDestroy -= PlayBallDestroyAudio;
    }

    private void FixedUpdate()
    {
        if (pushLockTimer > 0f)
            pushLockTimer = Mathf.Max(0f, pushLockTimer - Time.fixedDeltaTime);

        if (_currentDelayTime < 0)
            ApplyHoming();
        else
            _currentDelayTime -= Time.deltaTime;

        if (_rigidbody.linearVelocity.magnitude > _maxVelocity)
            _rigidbody.linearVelocity = Vector2.ClampMagnitude(_rigidbody.linearVelocity, _maxVelocity);

        if (!_enableMouseRedirect) return;
        HandleTimeScaleInput();
        HandleManaRegen();
    }

    void HandleTimeScaleInput()
    {
        if (Input.GetMouseButton(1) && _currentCoolDownPeriod >= _coolDownPeriod) // holding
        {
            _onAimingState = true;
            _targetTimeScale = _slowMotionTimeValue;
        }
        else // released
        {
            _onAimingState = false;
            _targetTimeScale = 1f;
            if (_currentCoolDownPeriod < _coolDownPeriod)
                _currentCoolDownPeriod += Time.deltaTime;
        }
        _currentTimeScale = Mathf.Lerp(_currentTimeScale, _targetTimeScale, Time.unscaledDeltaTime * _lerpSpeed);
        TimeManager.SetCustomTimeScale(_currentTimeScale);

        if (_onAimingState && Input.GetMouseButton(0))
            RedirectBallToMouse();
    }
    void HandleManaRegen()
    {
        if (_currentManaAmount < _maxManaAmount)
            _currentManaAmount += _manaRegenRate * Time.deltaTime;

    }

    void RedirectBallToMouse()
    {
        if (_currentManaAmount < _manaShootCost)
        {
            //UI pop message
            return;
        }

        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_gameCamera == null) return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -_gameCamera.transform.position.z;

        Vector2 mouseWorld = _gameCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 direction = mouseWorld - (Vector2)transform.position;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        direction.Normalize();

        float speed = _rigidbody.linearVelocity.magnitude;
        if (speed < 0.01f)
            speed = _gravityScale;

        _rigidbody.linearVelocity = direction * speed;

        transform.up = -direction;

        _currentCoolDownPeriod = 0;
        MinusMana(_manaShootCost);
    }
    void ApplyHoming()
    {
        // basic sanity checks
        if (_brickPool == null) return;

        Vector2 vel = _rigidbody.linearVelocity;
        float speed = vel.magnitude;
        if (speed < 0.01f) return; // not moving

        Vector2 dir = vel.normalized;

        // only apply homing when ball is travelling mostly horizontally (so it helps escape horizontal trap)
        if (Mathf.Abs(dir.y) >= _minVerticalForHoming) return;

        // get nearest active brick from pool
        GameObject target = _brickPool.GetNearestActiveBrick(transform.position, _homingMaxDistance);
        if (target == null) return;

        Vector2 toTarget = ((Vector2)target.transform.position - (Vector2)transform.position).normalized;

        // avoid abrupt U-turns: if the target is almost directly behind, reduce or skip homing
        float forwardDot = Vector2.Dot(dir, toTarget); // 1 = same dir, -1 = opposite
        if (forwardDot < -0.8f) return;

        // compute new direction (lerp between current and target direction)
        float s = _homingStrength;
        Vector2 newDir = Vector2.Lerp(dir, toTarget, s).normalized;
        // preserve speed
        _rigidbody.linearVelocity = newDir * speed;
    }
    public void ResetPosition()
    {
        _spriteRenderer.enabled = false;
        _abilityManager.NotifyBallDestroyed(this);
        ResetCombo();
        StartCoroutine(ResettingBall());
    }

    public void DestroyCopyBall()
    {
        _abilityManager.NotifyBallDestroyed(this);
        Destroy(gameObject);

    }
    IEnumerator ResettingBall()
    {
        yield return new WaitForSeconds(_respawnTime);
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onBallRespawn, transform.position);
        transform.position = _respawnPos.position;
        StartCoroutine(AnimateBallRespawn());
        _spriteRenderer.enabled = true;
        _rigidbody.linearVelocity = Vector2.down * _gravityScale;
    }

    IEnumerator AnimateBallRespawn()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = _startingScale * _capscaleMultiplier;

        float time = 0f;

        while (time < animationDuration)
        {
            float normalized = time / animationDuration;
            float curveValue = easeOutElastic.Evaluate(normalized);

            transform.localScale =
                Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = _startingScale;
    }

    void IncreaseCombo()
    {
        _currentCombo++;
        if (_currentCombo >= _feverThreshold)
        {
            _particleTrail.SetActive(true);
        }
    }
    void ResetCombo()
    {
        _currentCombo = 0;
        _particleTrail.SetActive(false);
    }
    void PlayBallDestroyAudio() => AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onBallDestroy, transform.position);
    void PlayBallRespawnAudio() => AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onBallRespawn, transform.position);

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Paddle") || other.gameObject.CompareTag("Brick"))
        {
            GlobalFeedbackManager.Instance.SetSizeCapForBall();
            GlobalFeedbackManager.Instance.PlayGlobalFeedback?.Invoke();
            OnBallHit?.Invoke();

            Vector2 avgNormal = Vector2.zero;
            int contacts = Mathf.Max(1, other.contactCount);
            for (int i = 0; i < other.contactCount; i++)
            {
                avgNormal += other.GetContact(i).normal;
            }
            avgNormal /= contacts;

            if (avgNormal.sqrMagnitude > 0.0001f)
                avgNormal.Normalize();
            else
                avgNormal = Vector2.up; // fallback

            Vector2 opposite = -avgNormal;
            transform.up = opposite;
            if (other.gameObject.GetComponent<BrickBar>() != null)
            {
                if (_copyBall)
                {
                    _currentBounce++;
                    if (_currentBounce > _maxBounce)
                        Destroy(gameObject);
                }
                _currentDelayTime = _delayTimeAfterHit;

                OnBrickHit?.Invoke();
                _abilityManager.NotifyBrickHit(other.gameObject.GetComponent<BrickBar>(), (_baseDamage + _damageValueModifier));
            }
        }
    }
    //HELPER
    public int GetBallBaseDamage() => _baseDamage;
    public void SetTimeScaleSmooth(float target, float duration)
    {
        if (Mathf.Approximately(_currentTargetTimeScale, target))
            return;

        _currentTargetTimeScale = target;

        if (_timeScaleRoutine != null)
            StopCoroutine(_timeScaleRoutine);

        _timeScaleRoutine = StartCoroutine(LerpTimeScale(target, duration));
    }
    IEnumerator LerpTimeScale(float target, float duration)
    {
        float start = Time.timeScale;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            float newScale = Mathf.Lerp(start, target, t);

            TimeManager.SetCustomTimeScale(newScale);

            time += Time.unscaledDeltaTime; // IMPORTANT
            yield return null;
        }

        TimeManager.SetCustomTimeScale(target);
    }
    void MinusMana(int val)
    {
        _currentManaAmount = -val;
        if (_currentManaAmount < 0)
        {
            _currentManaAmount = 0;
        }
    }
    public void SetHomingValue(float value) => _homingStrength = value;
    public float GetCurrentManaAmount() => _currentManaAmount;
    public float GetMaxManaAmount() => _maxManaAmount;


}