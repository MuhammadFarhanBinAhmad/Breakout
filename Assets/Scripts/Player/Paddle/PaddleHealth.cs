using System;
using System.Collections;
using UnityEngine;

public class PaddleHealth : MonoBehaviour
{
    PaddleMovement _paddleMovement;
    PaddleVacoom _paddleVacoom;
    PaddleFeedbackManager _paddleFeedbackManager;

    SpriteRenderer _spriteRenderer;

    [Header("Respawn")]
    public Transform _spawnPos;
    public GameObject _deathVFX;
    public float _timeTillRespawn;

         
    public Action OnPaddleDisable;
    public Action OnPaddleEnable;

    [Header("Knockback")]
    [SerializeField] float _knockbackDistance = 0.6f;
    [SerializeField] float _knockbackDuration = 0.15f;
    bool _isKnockbacking;

    private void Awake()
    {
        _paddleMovement = FindAnyObjectByType<PaddleMovement>();
        _paddleVacoom = FindAnyObjectByType<PaddleVacoom>();
        _paddleFeedbackManager = FindAnyObjectByType<PaddleFeedbackManager>();

        _spriteRenderer = GetComponentInParent<SpriteRenderer>();

        OnPaddleDisable += DisablePaddle;
        OnPaddleDisable += StartRespawnPaddleTimer;

        OnPaddleEnable += EnablePaddle;

    }
    private void OnDisable()
    {
        OnPaddleDisable -= DisablePaddle;
        OnPaddleDisable -= StartRespawnPaddleTimer;

        OnPaddleEnable -= EnablePaddle;

    }

    void DisablePaddle()
    {
        _paddleMovement.PaddleDisable(true);
        _paddleVacoom.PaddleDisable(true);
        _spriteRenderer.enabled = false;
        _deathVFX.SetActive(true);
        _paddleFeedbackManager.OnBeingDestroyed.Invoke();
    }

    void EnablePaddle()
    {
        _paddleMovement.PaddleDisable(false);
        _paddleVacoom.PaddleDisable(false);
        _spriteRenderer.enabled = true;
        transform.parent.position = _spawnPos.position;
        _paddleFeedbackManager.OnRespawn?.Invoke();
    }

    void StartRespawnPaddleTimer()
    {
        StartCoroutine(RespawnPaddle());
    }
    IEnumerator RespawnPaddle()
    {
        yield return new WaitForSeconds(_timeTillRespawn);
        OnPaddleEnable?.Invoke();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (_isKnockbacking) return;

        if (other.GetComponentInChildren<BrickBar>() !=null && _spriteRenderer.enabled)
        {
            _paddleFeedbackManager.OnBeingKnockBack?.Invoke();
            other.GetComponentInChildren<BrickBar>()._onDeathByPaddle?.Invoke();
            StartCoroutine(Knockback());
        }
    }

    IEnumerator Knockback()
    {
        _isKnockbacking = true;

        _paddleMovement.PaddleDisable(true);
        _paddleVacoom.PaddleDisable(true);

        Vector3 startPos = transform.parent.position;
        Vector3 targetPos = startPos + Vector3.down * _knockbackDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / _knockbackDuration;
            transform.parent.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // re-enable control
        _paddleMovement.PaddleDisable(false);
        _paddleVacoom.PaddleDisable(false);

        _isKnockbacking = false;
    }
}
