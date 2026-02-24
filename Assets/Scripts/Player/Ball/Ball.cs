using System;
using System.Collections;
using UnityEngine;

public class Ball : MonoBehaviour
{
    Rigidbody2D _rigRigidbody;
    SpriteRenderer _spriteRenderer;
    AbilityManager _abilityManager;
    BrickPool _brickPool;
    GlobalFeedbackManager _globalFeedbackManager;
    BallFeedbackManager _ballFeedbackManager;

    public float _gravityScale;
    public float _maxVelocity;

    public Action OnBallHit;
    public Action OnBallReset;
    public Action OnBrickHit;

    [Header("Respawn")]
    public float _respawnTime;
    public Transform _respawnPos;

    [Header("Damage")]
    [SerializeField] int _baseDamage;
    internal int _damageValueModifier;

    [Header("Combo")]
    internal int _currentCombo;
    [SerializeField] int _feverThreshold;
    [SerializeField] GameObject _particleTrail;

    [Header("CopyBall")]
    internal bool _copyBall;
    public int _maxBounce;
    int _currentBounce;

    [Header("Homing (subtle)")]
    public float _delayTimeAfterHit;
    public float _currentDelayTime;
    [Range(0f, 1f)]
    [SerializeField] float _homingStrength = 0.08f;
    [SerializeField] float _minVerticalForHoming = 0.25f;
    [SerializeField] float _homingMaxDistance = 12f;

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
        _rigRigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
        _brickPool = FindAnyObjectByType<BrickPool>();
        _globalFeedbackManager = FindAnyObjectByType<GlobalFeedbackManager>();
        _ballFeedbackManager = FindAnyObjectByType<BallFeedbackManager>();

        OnBrickHit += IncreaseCombo;
        OnBrickHit += _ballFeedbackManager.UpdateGlowIntensity;

        OnBallReset += ResetCombo;
        OnBallReset += ResetPosition;
    }
    private void Start()
    {
        // initial downward velocity - preserve your original intent
        _rigRigidbody.linearVelocity = Vector2.down * _gravityScale;
        _rigRigidbody.linearDamping = normalDrag;
    }
    private void OnDisable()
    {
        OnBrickHit -= IncreaseCombo;
        OnBrickHit -= _ballFeedbackManager.UpdateGlowIntensity;

        OnBallReset -= ResetCombo;
        OnBallReset -= ResetPosition;
    }

    private void FixedUpdate()
    {
        // decrement push lock timer
        if (pushLockTimer > 0f)
            pushLockTimer = Mathf.Max(0f, pushLockTimer - Time.fixedDeltaTime);

        // 1) Apply attraction if active and not locked
        if (isAttracted && pushLockTimer <= 0f)
            HandleAttraction();

        // 2) Homing behavior (as before)
        if (_currentDelayTime < 0)
            ApplyHoming();
        else
            _currentDelayTime -= Time.deltaTime;

        // 3) Clamp speed
        if (_rigRigidbody.linearVelocity.magnitude > _maxVelocity)
        {
            _rigRigidbody.linearVelocity = Vector2.ClampMagnitude(_rigRigidbody.linearVelocity, _maxVelocity);
        }
    }

    void HandleAttraction()
    {
        if (_rigRigidbody == null) return;

        Vector2 to = attractorPos - _rigRigidbody.position;
        float dist = to.magnitude;

        // safety: if out of radius, stop attraction
        if (dist > attractRadius)
        {
            StopAttraction();
            return;
        }

        // --- rotate slowly toward attractor direction ---
        Vector2 desiredDir = to.normalized;
        _attractDesiredDir = desiredDir; // cache (also updated by UpdateAttractionTarget)
        float rotT = Mathf.Clamp01(_attractRotationSpeed * Time.fixedDeltaTime);
        // Slerp the transform.up vector toward desired direction (2D)
        Vector3 currentUp = transform.up;
        Vector3 desiredUp = new Vector3(desiredDir.x, desiredDir.y, 0f);
        Vector3 newUp = Vector3.Slerp(currentUp, desiredUp, rotT).normalized;
        if (newUp.sqrMagnitude > 0.0001f)
            transform.up = newUp;

        // --- compute attraction force as before ---
        float t = Mathf.Clamp01(1f - (dist / attractRadius)); // 0 far, 1 near
        float pull = attractStrength * (t * 0.9f + 0.1f); // never completely zero

        Vector2 forceVec = desiredDir * pull;

        // clamp force magnitude per FixedUpdate to the configured cap
        if (attractForceCap > 0f)
            forceVec = Vector2.ClampMagnitude(forceVec, attractForceCap);

        // apply as acceleration-like force
        _rigRigidbody.AddForce(forceVec, ForceMode2D.Force);

        // record last pull
        lastPullApplied = forceVec;

        // update attractor position if it follows a transform
        if (attractorTransform != null)
            attractorPos = attractorTransform.position;
    }

    // Suction API for Ball (existing)
    public void StartAttraction(Transform targetTransform, float strength, float radius, float maxForce)
    {
        attractorTransform = targetTransform;
        attractorPos = targetTransform.position;
        attractRadius = Mathf.Max(0.01f, radius);
        attractStrength = strength;
        attractForceCap = Mathf.Max(0f, maxForce);
        isAttracted = true;
        _rigRigidbody.linearDamping = attractedDrag;

        // set initial desired direction and nudge rotation a tiny bit toward it (so rotation begins immediately)
        Vector2 initialTo = attractorPos - (Vector2)transform.position;
        if (initialTo.sqrMagnitude > 0.0001f)
        {
            _attractDesiredDir = initialTo.normalized;
            float initT = Mathf.Clamp01(_attractRotationSpeed * Time.deltaTime);
            Vector3 newUp = Vector3.Slerp(transform.up, new Vector3(_attractDesiredDir.x, _attractDesiredDir.y, 0f), initT).normalized;
            if (newUp.sqrMagnitude > 0.0001f)
                transform.up = newUp;
        }
    }

    public void UpdateAttractionTarget(Vector2 targetPosition)
    {
        attractorPos = targetPosition;
        // also update desired direction so rotation continues to track moving targets
        Vector2 to = attractorPos - (Vector2)transform.position;
        if (to.sqrMagnitude > 0.0001f)
            _attractDesiredDir = to.normalized;
    }

    public void StopAttraction()
    {
        isAttracted = false;
        attractorTransform = null;
        // restore normal drag
        _rigRigidbody.linearDamping = normalDrag;
    }

    public void ApplyPush(Vector2 origin, float impulseMagnitude, float lockDuration)
    {
        if (_rigRigidbody == null) return;

        // compute direction away from origin
        Vector2 dir = ((Vector2)transform.position - origin);
        float dist = dir.magnitude;
        if (dist <= 0.0001f)
            dir = UnityEngine.Random.insideUnitCircle.normalized;
        else
            dir /= dist;

        // apply impulse (impulseMagnitude already can take falloff from caller)
        Vector2 impulse = dir * impulseMagnitude;
        _rigRigidbody.AddForce(impulse, ForceMode2D.Impulse);

        // cancel attraction and apply lock
        StopAttraction();
        pushLockTimer = Mathf.Max(pushLockTimer, lockDuration);
    }

    // ---------- rest of your original Ball script methods unchanged ----------
    void ApplyHoming()
    {
        // basic sanity checks
        if (_brickPool == null) return;

        Vector2 vel = _rigRigidbody.linearVelocity;
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
        _rigRigidbody.linearVelocity = newDir * speed;
    }

    public void SetHomingValue(float value) => _homingStrength = value;

    public void ResetPosition()
    {
        if (_copyBall)
        {
            Destroy(gameObject);
        }
        else
        {
            _spriteRenderer.enabled = false;
            _abilityManager.NotifyBallDestroyed(this);
            ResetCombo();
            StartCoroutine(ResettingBall());
        }
    }
    IEnumerator ResettingBall()
    {
        yield return new WaitForSeconds(_respawnTime);
        transform.position = _respawnPos.position;
        _spriteRenderer.enabled = true;
        _rigRigidbody.linearVelocity = Vector2.down * _gravityScale;
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

    

    private void OnCollisionEnter2D(Collision2D other)
    {

        
        if (other.gameObject.CompareTag("Wall") || other.gameObject.CompareTag("Paddle") || other.gameObject.CompareTag("Brick"))
        {
            _globalFeedbackManager.PlayGlobalFeedback?.Invoke();
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
}