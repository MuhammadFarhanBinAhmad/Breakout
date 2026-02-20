using System.Collections;
using UnityEngine;

public class PaddleEyeManager : MonoBehaviour
{
    public GameObject [] _paddleEyes = new GameObject [2];

    [Header("Blink Settings")]
    [SerializeField] float _minBlinkInterval = 2f;
    [SerializeField] float _maxBlinkInterval = 5f;
    [SerializeField] float _blinkDuration = 0.1f;
    public bool _stopBlinking;

    Coroutine _blinkRoutine;
    Coroutine _forcedBlinkRoutine;

    void Start()
    {
        _blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkRoutine()
    {
        if (_stopBlinking)
            yield return null;

        while (true)
        {
            float waitTime = Random.Range(_minBlinkInterval, _maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            yield return BlinkOnce();
        }
    }

    public void BlinkNow()
    {
        // Prevent overlapping forced blinks
        if (_forcedBlinkRoutine != null)
            StopCoroutine(_forcedBlinkRoutine);

        _forcedBlinkRoutine = StartCoroutine(BlinkOnce());
    }
    public void DoubleBlink()
    {
        StartCoroutine(DoubleBlinkRoutine());
    }
    public void CloseEye()
    {
        StopAllCoroutines();
        SetEyesActive(false);
        _stopBlinking = true;
    }


    public void OpenEye() 
    {
        SetEyesActive(true);
        _stopBlinking = false;
        _blinkRoutine = StartCoroutine(BlinkRoutine());
    }

    IEnumerator BlinkOnce()
    {
        if (_stopBlinking)
            yield return null;

        SetEyesActive(false);
        yield return new WaitForSeconds(_blinkDuration);
        SetEyesActive(true);
    }
    IEnumerator DoubleBlinkRoutine()
    {
        if (_stopBlinking)
            yield return null;

        yield return BlinkOnce();
        yield return new WaitForSeconds(0.1f);
        yield return BlinkOnce();
    }

    void SetEyesActive(bool isActive)
    {
        foreach (var eye in _paddleEyes)
        {
            if (eye != null)
                eye.SetActive(isActive);
        }
    }
}
