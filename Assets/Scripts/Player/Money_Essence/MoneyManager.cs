using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    TimeManager _timeManager;

    [Header("Balance / Output")]
    public int _currentMoney;

    [Header("Reward tuning (use brick stats + month)")]
    [Tooltip("Base value per 1 health point.")]
    [SerializeField] float _baseHealthValueMultiplier;
    [Tooltip("Base value per 1 speed unit.")]
    [SerializeField] float _baseSpeedValueMultiplier;
    [Tooltip("Per-month growth")]
    [SerializeField] float _dayMultiplier;
    [Tooltip("use exponential scaling (1+m)^month. Otherwise linear (1 + month*m).")]
    [SerializeField] bool _useExponentialMonthScaling;
    [Tooltip("Minimum money granted for destroying any brick.")]
    [SerializeField] int _minReward;
    [Tooltip("Ocap to prevent runaway growth (0 = no cap).")]
    [SerializeField] int _moneyCap;

    private void Start()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();
    }

    public void CalculateBrickValue(BrickBar bb)
    {
        if (bb == null)
            return;

        int currentday = 1;
        if (_timeManager != null)
            currentday = _timeManager.GetTotalDayPass();

        // 1) compute month scale
        float dayScale;
        if (_useExponentialMonthScaling)
            dayScale = Mathf.Pow(1f + _dayMultiplier, currentday);
        else
            dayScale = 1f + currentday * _dayMultiplier;

        // 2) base components
        float healthComponent = bb._startingHealth * _baseHealthValueMultiplier;
        float speedComponent = bb._fallSpeed * _baseSpeedValueMultiplier;

        // 3) combined and scaled
        float rawValue = (healthComponent + speedComponent) * dayScale;
        print("HCP: " + healthComponent + " SCP: " + speedComponent + " MS: " + dayScale + " Total: " + rawValue) ;
        AddMoney(Mathf.RoundToInt(rawValue));
    }

    public void AddMoney(int amount) => _currentMoney += amount;
    public void RemoveMoney(int amount) => _currentMoney -= amount;
}
