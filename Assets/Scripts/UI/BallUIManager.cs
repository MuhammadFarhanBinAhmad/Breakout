using TMPro;
using UnityEngine;
using System.Collections;

public class BallUIManager : MonoBehaviour
{
    Ball _ballManager;

    [Header("BallUI")]
    [SerializeField] TextMeshProUGUI _currentComboText;
    [SerializeField] int _comboParticleThreshold;
    [SerializeField] GameObject _comboParticleEffect;

    [Header("Animation")]
    [SerializeField] AnimationCurve easeOutElastic;
    [SerializeField] float animationDuration = 0.5f;
    [SerializeField] float _startingscaleMultiplier;
    [SerializeField] float _increasescaleMultiplier;
    [SerializeField] float _currentscaleMultiplier;
    [SerializeField] float _capscaleMultiplier;


    Coroutine comboAnim;

    void Start()
    {
        _ballManager = FindAnyObjectByType<Ball>();
        _ballManager.OnBrickHit += UpdateComboUI;
        _ballManager.OnBallReset+= UpdateComboUI;
    }

    private void OnDisable()
    {
        _ballManager.OnBrickHit -= UpdateComboUI;
        _ballManager.OnBallReset -= UpdateComboUI;
    }

    public void UpdateComboUI()
    {
        if (_ballManager._currentCombo > 0)
        {
            _currentComboText.text = _ballManager._currentCombo.ToString() + 'x';
            if(_ballManager._currentCombo % _comboParticleThreshold == 0 && _currentscaleMultiplier < _capscaleMultiplier)
            {
                _currentscaleMultiplier += _increasescaleMultiplier;
            }
        }
        else
        {
            _currentscaleMultiplier = _startingscaleMultiplier;
            _currentComboText.text = "";
            return;
        }

        if (comboAnim != null)
            StopCoroutine(comboAnim);

        comboAnim = StartCoroutine(AnimateCombo());
    }

    IEnumerator AnimateCombo()
    {
        if(_ballManager._currentCombo >= _comboParticleThreshold)
            _comboParticleEffect.SetActive(true);

        Transform t = _currentComboText.transform;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * _currentscaleMultiplier;

        float time = 0f;

        while (time < animationDuration)
        {
            float normalized = time / animationDuration;
            float curveValue = easeOutElastic.Evaluate(normalized);

            t.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = Vector3.one;
    }
}
