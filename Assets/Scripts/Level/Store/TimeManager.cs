using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    StoreManager _storeManager;

    [Header("TimeKeeper")]
    [SerializeField] int week;
    [SerializeField] int day;
    [SerializeField] int _maxWeek;
    [SerializeField] int _maxDay;
    int _totalDayPass;
    [SerializeField] float _fullDayDuration;
    [SerializeField] float _currentDayDuration;

    public Action _dayPass;
    public Action _weekPass;

    [Header("RealTime")]
    [SerializeField] float _currentRealTimePass;

    private void Start()
    {
        _weekPass += OnEndOfWeek;
    }
    private void OnDisable()
    {
        _weekPass -= OnEndOfWeek;
    }
    private void Update()
    {
        WeekPass();
        _currentRealTimePass += Time.deltaTime;
    }
    public void WeekPass()
    {
        if(_currentDayDuration > 0)
        {
            _currentDayDuration -= Time.deltaTime;
        }
        else
        {
            day++;
            _totalDayPass++;
            _currentDayDuration = _fullDayDuration;
            _weekPass?.Invoke();
            if (day >= _maxDay)
            {
                _weekPass?.Invoke();
            }
        }
    }
    public void OnEndOfWeek()
    {
        week++;
        day = 1;       
    }
    public float GetCurrentRealTime () => _currentRealTimePass;
    public int GetCurrentWeek() => week;
    public int GetCurrentDay() => day;
    public int GetMaxWeek() => _maxWeek;
    public int GetMaxDay() => _maxDay;
    public int GetTotalDayPass() => _totalDayPass;

}
