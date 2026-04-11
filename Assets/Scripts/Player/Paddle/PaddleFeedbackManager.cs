using System;
using System.Collections;
using UnityEngine;

public class PaddleFeedbackManager : MonoBehaviour
{
    public Action OnHitBall;
    public Action OnHitBrick;
    public Action OnBeingKnockBack;
    public Action OnBeingDestroyed;
    public Action OnRespawn;

    [Header("OnHitBall")]
    public GameObject _hitParticleEffect;
    PaddleEyeManager _paddleEyeManager;
    [Header("OnHitBallAnimation")]
    [SerializeField] AnimationCurve easeOutElastic;
    Vector3 _startingScale;
    [SerializeField] float animationDuration = 0.5f;
    [SerializeField] float _startingscaleMultiplier;
    [SerializeField] float _capscaleMultiplier;
    Coroutine _paddleHitRoutine;
    float _maxBallSpeed = 20f;

    [Header("OnHitBrick")]
    [Header("Freeze Frame (Knockback)")]
    [Tooltip("How long the freeze-frame lasts in seconds (real time).")]
    [SerializeField] float _freezeRealtimeDuration;
    [Tooltip("Whether to set Time.timeScale to zero (true freeze) or a tiny slow-motion factor.")]
    [SerializeField] bool _useFullFreeze = true;
    [Tooltip("If not using full freeze, the time scale to set during effect (e.g. 0.05).")]
    [SerializeField] float _slowMotionScale;
    Coroutine _freezeRoutine;
    [Header("Flash Screen")]
    public bool _playFlash;
    public GameObject _flashScreen;
    public float _flashDuration;

    [Header("Breathing Animation")]
    [SerializeField] float _breathSpeed = 2f;
    [SerializeField] float _breathScaleAmount = 0.05f;

    Coroutine _breathingRoutine;
    bool _isBreathing = true;

    [Header("OnPaddleRespawn")]
    public GameObject _respawnVFX;

    private void OnEnable()
    {
        _paddleEyeManager = FindAnyObjectByType<PaddleEyeManager>();

        OnHitBall += SpawnHitParticleEffect;
        OnHitBall += PlayHitSoundEffect;
        OnHitBall += _paddleEyeManager.DoubleBlink;

        OnBeingKnockBack += PlayFreezeFrame;
        OnBeingKnockBack += PlayAnimatePaddleHit;

        OnBeingDestroyed += _paddleEyeManager.CloseEye;

        OnRespawn += PlayAnimatePaddleRespawn;
        OnRespawn += _paddleEyeManager.OpenEye;
    }
    private void Start()
    {
        _startingScale = transform.localScale;
        //StartBreathing();

    }
    private void OnDisable()
    {
        OnHitBall -= SpawnHitParticleEffect;
        OnHitBall -= PlayHitSoundEffect;
        OnHitBall -= _paddleEyeManager.DoubleBlink;

        OnBeingKnockBack -= PlayFreezeFrame;
        OnBeingKnockBack -= PlayAnimatePaddleHit;

        OnBeingDestroyed -= _paddleEyeManager.CloseEye;

        OnRespawn -= PlayAnimatePaddleRespawn;
        OnRespawn -= _paddleEyeManager.OpenEye;

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        GameObject hitObject = other.gameObject;
        if(hitObject.CompareTag("Ball"))
        {
            Rigidbody2D rb = other.rigidbody;
            if (rb != null)
            {
                PlayAnimatePaddleHit(rb.linearVelocity.magnitude);
            }
            OnHitBall?.Invoke();
        }
    }

    void SpawnHitParticleEffect() => _hitParticleEffect.SetActive(true);
    void PlayHitSoundEffect() => AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onPaddleHitByBrick, transform.position);
    void PlayAnimatePaddleHit(float ballSpeed)
    {
        StopBreathing();

        if (_paddleHitRoutine != null)
            StopCoroutine(_paddleHitRoutine);

        _paddleHitRoutine = StartCoroutine(AnimatePaddleHit(ballSpeed));
    }
    void PlayAnimatePaddleHit()
    {
        StopBreathing();

        if (_paddleHitRoutine != null)
            StopCoroutine(_paddleHitRoutine);

        _paddleHitRoutine = StartCoroutine(AnimatePaddleHit());
    }
    void PlayAnimatePaddleRespawn()
    {
        StopBreathing();

        if (_paddleHitRoutine != null)
            StopCoroutine(_paddleHitRoutine);

        _paddleHitRoutine = StartCoroutine(AnimatePaddleRespawn());
    }

    //void StartBreathing()
    //{
    //    if (_breathingRoutine != null)
    //        StopCoroutine(_breathingRoutine);

    //    _isBreathing = true;
    //    _breathingRoutine = StartCoroutine(AnimateBreathingRoutine());
    //}

    void StopBreathing()
    {
        _isBreathing = false;

        if (_breathingRoutine != null)
        {
            StopCoroutine(_breathingRoutine);
            _breathingRoutine = null;
        }

        transform.localScale = _startingScale;
    }
    //IEnumerator AnimateBreathingRoutine()
    //{
    //    float time = 0f;

    //    while (_isBreathing)
    //    {
    //        time += Time.deltaTime * _breathSpeed;

    //        float scaleOffset = Mathf.Sin(time) * _breathScaleAmount;
    //        transform.localScale = _startingScale * (1f + scaleOffset);

    //        yield return null;
    //    }

    //}
    IEnumerator AnimatePaddleHit(float ballSpeed)
    {

        // Normalize speed (0..1)
        float t = Mathf.InverseLerp(0f, _maxBallSpeed, ballSpeed);

        // Calculate scale multiplier based on speed
        float scaleMultiplier = Mathf.Lerp(
            _startingscaleMultiplier,
            _capscaleMultiplier,
            t
        );

        Vector3 startScale = _startingScale;
        Vector3 targetScale = _startingScale * scaleMultiplier;

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
        //StartBreathing();
    }
    IEnumerator AnimatePaddleHit()
    {
        Vector3 startScale = _startingScale;
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
        //StartBreathing();
    }
    IEnumerator AnimatePaddleRespawn()
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
        //StartBreathing();
    }
    void PlayFreezeFrame()
    {
        if (_freezeRoutine != null)
            StopCoroutine(_freezeRoutine);

        _freezeRoutine = StartCoroutine(FreezeFrameRoutine());
    }

    IEnumerator FreezeFrameRoutine()
    {
        // Save original time values
        float originalTimeScale = 1;
        float originalFixedDelta = Time.fixedDeltaTime;

        // Choose target timescale: either full freeze (0) or small slow-mo
        float targetScale = _useFullFreeze ? 0f : Mathf.Clamp(_slowMotionScale, 0.0001f, 1f);

        // Apply freeze/slowdown
        Time.timeScale = targetScale;
        // Adjust fixedDelta to keep physics consistent when timeScale changes
        Time.fixedDeltaTime = originalFixedDelta * Time.timeScale;

        // Wait in real time (unaffected by Time.timeScale)
        yield return new WaitForSecondsRealtime(_freezeRealtimeDuration);

        // Restore original values
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;

        _freezeRoutine = null;

        //Flash
        if (_playFlash)
            _flashScreen.SetActive(true);
        else
            yield return null;

        yield return new WaitForSecondsRealtime(_flashDuration);
        _flashScreen.SetActive(false);

    }

}
