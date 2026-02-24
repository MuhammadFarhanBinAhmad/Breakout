using TMPro;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

[System.Serializable]
public struct ComboPerformance
{
    public int comboThreshold;
    public string word;
    public string rankLetter;
}

public class BallUIManager : MonoBehaviour
{
    Ball _ballManager;

    [Header("ComboUI")]
    [SerializeField] TextMeshProUGUI _currentComboText;
    [SerializeField] int _comboParticleThreshold;
    [SerializeField] GameObject _comboParticleEffect;
    [SerializeField] TextMeshProUGUI _comboPerformanceText;
    [SerializeField] ComboPerformance[] _comboPerformances;
    [SerializeField] int _rankLetterSize, _wordTextSize;

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
        UpdateComboPerformanceNumber();
        UpdateComboPerformanceText();

        if (comboAnim != null)
            StopCoroutine(comboAnim);

        comboAnim = StartCoroutine(AnimateCombo());
    }
    void UpdateComboPerformanceNumber()
    {
        if (_ballManager._currentCombo > 0)
        {
            _currentComboText.text = _ballManager._currentCombo.ToString() + 'x';
            if (_ballManager._currentCombo % _comboParticleThreshold == 0 && _currentscaleMultiplier < _capscaleMultiplier)
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



    }
    void UpdateComboPerformanceText()
    {
        int combo = _ballManager._currentCombo;

        for (int i = _comboPerformances.Length - 1; i >= 0; i--)
        {
            if (combo >= _comboPerformances[i].comboThreshold)
            {
                _comboPerformanceText.text =
                    $"<size={_rankLetterSize}><b>{_comboPerformances[i].rankLetter}</b></size>" +
                    $"<size={_wordTextSize}> {_comboPerformances[i].word}</size>";
                return;
            }
        }
    }
    IEnumerator AnimateCombo()
    {
        if(_ballManager._currentCombo >= _comboParticleThreshold)
            _comboParticleEffect.SetActive(true);

        Transform n = _currentComboText.transform;
        Transform t = _comboPerformanceText.transform;
        Vector3 startScale = Vector3.one;
        Vector3 targetScale = Vector3.one * _currentscaleMultiplier;

        float time = 0f;

        while (time < animationDuration)
        {
            float normalized = time / animationDuration;
            float curveValue = easeOutElastic.Evaluate(normalized);

            t.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);
            n.localScale = Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        t.localScale = Vector3.one;
        n.localScale = Vector3.one;
    }
}
