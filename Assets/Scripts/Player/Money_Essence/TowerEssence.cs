using Cinemachine.Utility;
using System;
using UnityEngine;
using UnityEngine.Jobs;

public class TowerEssence : MonoBehaviour
{
    TowerManager _towerManager;
    PaddleVacoom _paddleVacoom;

    [Header("EssenseStas")]
    Action _OnCollectionEvent;
    [SerializeField] int _essenceMinWorth;
    [SerializeField] int _essenceMaxWorth;
    [SerializeField] float _essenceBonusMultiplier;
    int _currentExpirationPhase;
    [SerializeField] float[] _essenceExpirationTime;
    [SerializeField] float _essenceCurrentLiveTime;

    [Header("Physics / Movement")]
    Rigidbody2D rb;
    public float maxSpeed;
    [Tooltip("Use this to smooth motion while being attracted (lower = more drag).")]
    public float attractedDrag;
    [Tooltip("Normal drag when not being attracted.")]
    public float normalDrag;
    [Tooltip("Starting Impluse")]
    public float minImpulse;
    public float maxImpulse;
    
    [Header("Suction / Collection")]
    public float _collectDistance;

    [Header("Suction Tuning")]
    [Tooltip("Reduced drag while being sucked (so suction is smooth).")]
    public float suctionDrag = 0.1f;
    [Tooltip("How strongly we add an impulse in the last pull direction when suction stops.")]
    [Range(0f, 2f)]
    public float suctionReleaseImpulseMultiplier = 1.0f;

    // internal
    bool isAttracted = false;
    Vector2 attractorPos;
    Transform attractorTransform;
    float attractRadius = 1f;
    float attractStrength = 0f;


    // store last pull applied while being sucked (world-space force vector)
    Vector2 lastPullApplied = Vector2.zero;

    void Awake()
    {
        rb = rb ? rb : GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = normalDrag;

        _towerManager = FindAnyObjectByType<TowerManager>();
        _paddleVacoom = FindAnyObjectByType<PaddleVacoom>();
    }
    private void OnEnable()
    {
        Vector3 dir = UnityEngine.Random.onUnitSphere; // uniform random direction
        float mag = UnityEngine.Random.Range(minImpulse, maxImpulse);
        rb.AddForce(dir * mag, ForceMode2D.Impulse);
    }
    private void OnDisable()
    {
        _OnCollectionEvent = null;
    }
    void FixedUpdate()
    {
        if (gameObject.activeSelf)
        {
            _essenceCurrentLiveTime += Time.deltaTime;
            if (_essenceCurrentLiveTime > _essenceExpirationTime[_currentExpirationPhase] && _currentExpirationPhase < _essenceExpirationTime.Length - 1)
            {
                _currentExpirationPhase++;
            }
        }
        float distance = Vector2.Distance(transform.position, _paddleVacoom.transform.position);
        if (distance < _collectDistance)
        {
            HandleCollection();
        }
        // natural wandering or idle forces can be added here if you want (optional)
        if (!isAttracted) return;

        Vector2 to = (Vector2)attractorPos - rb.position;
        float dist = to.magnitude;

        // if out of radius, stop attraction (safety)
        if (dist > attractRadius)
        {
            StopAttraction();
            return;
        }

        float t = Mathf.Clamp01(1f - (dist / attractRadius)); // 0 far, 1 near
        float pull = attractStrength * (t * 0.9f + 0.1f); // never completely zero (0.1 base)

        Vector2 force = to.normalized * pull;

        rb.AddForce(force, ForceMode2D.Force);

        if (rb.linearVelocity.magnitude > maxSpeed)
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        if (attractorTransform != null)
            attractorPos = attractorTransform.position;


    }

    // Suction API (player uses this)
    public void StartAttraction(Transform targetTransform, float strength, float radius)
    {
        attractorTransform = targetTransform;
        attractorPos = targetTransform.position;
        attractRadius = Mathf.Max(0.01f, radius);
        attractStrength = strength;
        isAttracted = true;
        rb.linearDamping = attractedDrag;
    }

    public void UpdateAttractionTarget(Vector2 targetPosition)
    {
        attractorPos = targetPosition;
    }

    public void StopAttraction()
    {
        isAttracted = false;
        attractorTransform = null;
        rb.linearDamping = normalDrag; // restore drag so it slowly slows naturally
    }

    public void HandleCollection()
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_essenceCollect, transform.position);

        switch (_currentExpirationPhase)
        {
            case 0:
                _towerManager.IncreaseEssenceCount(GetBonusEssence());
                break;
            case 1:
                _towerManager.IncreaseEssenceCount(GetNormalEssence());
                break;
            case 2:
                _towerManager.IncreaseEssenceCount(GetHalfEssence());
                break;
        }
        gameObject.SetActive(false);
    }

    int GetBonusEssence()
    {
        int essence = UnityEngine.Random.Range(_essenceMinWorth, _essenceMaxWorth);
        return (int)(essence * _essenceBonusMultiplier);
    }
    int GetNormalEssence() => UnityEngine.Random.Range(_essenceMinWorth, _essenceMaxWorth);
    int GetHalfEssence() => UnityEngine.Random.Range(_essenceMinWorth, _essenceMaxWorth) / 2;
}
