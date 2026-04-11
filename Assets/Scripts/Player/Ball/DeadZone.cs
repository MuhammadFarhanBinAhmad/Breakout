using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DeadZone : MonoBehaviour
{
    TowerManager _towerManager;

    public Action OnShieldDamage;

    public GameObject _deathVFX;

    [Header("Shield")]
    [SerializeField] float _maxShieldMana;
    [SerializeField] float _currentShieldMana;
    [SerializeField] float _coolDownPeriod;
    [SerializeField] float _currentCoolDownTime;
    [SerializeField] float _shieldRegenRate;

    [Header("ShieldFeedback")]
    [SerializeField] Light2D _shieldLight;
    [SerializeField] float _shieldBrightnessMaxIntensity;
    [SerializeField] float _shieldBrightnessCurrentIntensity;


    private void Start()
    {
        _towerManager = FindAnyObjectByType<TowerManager>();
        _currentShieldMana = _maxShieldMana;

        UpdateShieldVisual();

    }
    private void Update()
    {
        if (_currentShieldMana >= _maxShieldMana) return;

        if(_currentShieldMana < _maxShieldMana)
        {
            if(_currentCoolDownTime > 0)
                _currentCoolDownTime -= Time.deltaTime;
            else
                _currentShieldMana += _shieldRegenRate * Time.deltaTime;

            _currentShieldMana = Mathf.Clamp(_currentShieldMana, 0, _maxShieldMana);

            UpdateShieldVisual();
        }
    }
    public void ShieldTakingDamage(int val)
    {
        _currentShieldMana -= val;
        _currentCoolDownTime = _coolDownPeriod;

        _currentShieldMana = Mathf.Max(_currentShieldMana, 0);

        UpdateShieldVisual();
    }
    void UpdateShieldVisual()
    {
        if (_shieldLight == null) return;

        float normalized = _currentShieldMana / _maxShieldMana;
        normalized = Mathf.Pow(normalized, 1.5f); // tweak this

        _shieldBrightnessCurrentIntensity = normalized * _shieldBrightnessMaxIntensity;

        _shieldLight.intensity = _shieldBrightnessCurrentIntensity;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Ball>() != null)
        {
            Ball ball = other.GetComponent<Ball>();
            _deathVFX.SetActive(true);
            if(!ball._copyBall)
            {
                ball.OnBallReset?.Invoke();
            }
            else
            {
                ball.OnBallDestroy?.Invoke();
            }
        }
        if(other.GetComponent<PaddleHealth>() != null)
        {
            PaddleHealth ph = other.GetComponent<PaddleHealth>();
            ph.OnPaddleDisable?.Invoke();
        }
        if( other.GetComponent<TowerEssence>() != null)
        {
            TowerEssence te = other.GetComponent<TowerEssence>();
            AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_essenceDestroyed, transform.position);
            te.gameObject.SetActive(false);
        }
        if (other.GetComponent<BrickBar>() != null)
        {
            BrickBar _bb = other.GetComponent<BrickBar>();
            int health = _bb.GetHealth();
            ShieldTakingDamage(health);
            _bb.OnDamage(999,DeathCause.TOWER);
            _towerManager._onTowerTakingDamage?.Invoke();
        }
    }
}
