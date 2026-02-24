using System;
using System.Collections;
using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BallFeedbackManager : MonoBehaviour
{
    Ball _ballManager;


    [Header("Hit")]
    [SerializeField] GameObject _hitEffectVFX;
    [SerializeField] SpriteRenderer _spriteRenderer;
    [SerializeField] Sprite _startSpirite,_hitSprite;
    [SerializeField] float _spriteChangeTime;
    [SerializeField] Color _startHitColor, _endHitColor;
    Coroutine _changeSpriteCoroutine;

    [Header("Glow")]
    [SerializeField] Light2D _glowLight;
    [SerializeField] float _startLightIntensity;
    [SerializeField] float[] _glowThreshold;
    [SerializeField] float[] _glowIntensity;
    private void Start()
    {
        _ballManager = FindAnyObjectByType<Ball>();

        _ballManager.OnBallHit += ChangeSpriteOnHit;
        _ballManager.OnBallHit += ActivateHitVFX;

        _ballManager.OnBallReset += ResetGlow;

    }
    private void OnDestroy()
    {
        _ballManager.OnBallHit -= ChangeSpriteOnHit;
        _ballManager.OnBallHit -= ActivateHitVFX;

        _ballManager.OnBallReset -= ResetGlow;
    }
    void ActivateHitVFX() => _hitEffectVFX.SetActive(true);
    public void ChangeSpriteOnHit()
    {
        if(_changeSpriteCoroutine != null) 
            StopCoroutine(_changeSpriteCoroutine);
        _changeSpriteCoroutine = StartCoroutine(AnimateSpriteChange());

    }
    IEnumerator AnimateSpriteChange()
    {
        _spriteRenderer.sprite = _hitSprite;
        _spriteRenderer.color = _startHitColor;
        yield return new WaitForSeconds(_spriteChangeTime);
        _spriteRenderer.sprite = _startSpirite;
        _spriteRenderer.color = _endHitColor;

        Quaternion originalRotation = transform.rotation;

        float randomZ = UnityEngine.Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0f, 0f, randomZ);
    }
    public void UpdateGlowIntensity()
    {
        int combo = _ballManager._currentCombo;

        for (int i = _glowThreshold.Length - 1; i >= 0; i--)
        {
            if (combo >= _glowThreshold[i])
            {
                _glowLight.intensity = _glowThreshold[i];
                return;
            }
        }
    }
    void ResetGlow() => _glowLight.intensity = _startLightIntensity;
}
