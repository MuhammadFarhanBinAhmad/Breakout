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
    [Header("RespawnAnimation")]
    [SerializeField] AnimationCurve easeOutElastic;
    Vector3 _startingScale;
    [SerializeField] float animationDuration;
    [SerializeField] float _capscaleMultiplier;
    // -------------------------
    // Attraction fields (vacuum)
    // -------------------------
    [Header("Attraction (vacuum)")]
    bool isAttracted = false;
    Vector2 attractorPos;
    Transform attractorTransform;
    float attractRadius = 1f;
    float attractStrength = 0f;
    float attractForceCap = 0f;            // max force applied per FixedUpdate
    [Tooltip("Drag while being attracted (lower = less braking).")]
    [SerializeField] float attractedDrag = 0.2f;
    [Tooltip("Normal drag when not being attracted.")]
    [SerializeField] float normalDrag = 1f;

    // rotation while attracted: how fast the ball turns toward the attractor
    [Tooltip("How quickly the ball rotates to face the attractor (higher = faster).")]
    [SerializeField] float _attractRotationSpeed = 5f; // tweak in Inspector
    Vector2 _attractDesiredDir = Vector2.up;
    Vector2 lastPullApplied = Vector2.zero;




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
        _rigidbody.linearDamping = normalDrag;

        _startingScale = transform.localScale;
    }
    private void OnDisable()
    {
        OnBrickHit -= IncreaseCombo;
        OnBrickHit -= _ballFeedbackManager.UpdateGlowIntensity;

        OnBallReset -= ResetCombo;
        OnBallReset -= ResetPosition;
        OnBallReset -= PlayBallDestroyAudio;

        OnBallDestroy -= DestroyCopyBall ;
        OnBallDestroy -= PlayBallDestroyAudio;
    }

    private void FixedUpdate()
    {
        // decrement push lock timer
        if (pushLockTimer > 0f)
            pushLockTimer = Mathf.Max(0f, pushLockTimer - Time.fixedDeltaTime);

        // 2) Homing behavior (as before)
        if (_currentDelayTime < 0)
            ApplyHoming();
        else
            _currentDelayTime -= Time.deltaTime;

        // 3) Clamp speed
        if (_rigidbody.linearVelocity.magnitude > _maxVelocity)
        {
            _rigidbody.linearVelocity = Vector2.ClampMagnitude(_rigidbody.linearVelocity, _maxVelocity);
        }
    }

    // ---------- rest of your original Ball script methods unchanged ----------
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

    public void SetHomingValue(float value) => _homingStrength = value;

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
}