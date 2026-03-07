using System;
using TMPro.EditorUtilities;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    StoreManager _storeManager;

    [Header("TimeKeeper")]
    [SerializeField] int week;
    [SerializeField] int day;
    [SerializeField] int _month;

    [SerializeField] int _maxDay;
    [SerializeField] int _maxWeek;
    [SerializeField] int _maxMonth;
    int _totalDayPass;
    [SerializeField] float _fullDayDuration;
    [SerializeField] float _currentDayDuration;

    public Action _dayPass;
    public Action _weekPass;
    public Action _monthPass;

    [Header("RealTime")]
    [SerializeField] float _currentRealTimePass;

    private void Start()
    {
        _weekPass += OnEndOfWeek;
        _monthPass += OnEndOfMonth;
    }
    private void OnDisable()
    {
        _weekPass -= OnEndOfWeek;
        _monthPass -= OnEndOfMonth;
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
            _dayPass?.Invoke();
            if (day >= _maxDay)
            {
                _weekPass?.Invoke();
            }
            if(week >= _maxWeek)
            {
                _monthPass?.Invoke();
            }
            
        }
    }
    public void OnEndOfWeek()
    {
        week++;
        day = 0;       
    }
    public void OnEndOfMonth()
    {
        week = 0;
        _month++;
        if (_month >= _maxMonth)
        {
            print("end of game");
        }
    }
    public float GetDayNormalized()
    {
        return 1f - (_currentDayDuration / _fullDayDuration);
    }
    public static void StopTime() => Time.timeScale = 0f;
    public static void StartTime() => Time.timeScale = 1f;

    public float GetCurrentRealTime () => _currentRealTimePass;
    public int GetCurrentWeek() => week;
    public int GetCurrentDay() => day;
    public int GetCurrentMonth() => _month;
    public int GetMaxWeek() => _maxWeek;
    public int GetMaxDay() => _maxDay;
    public int GetMaxMonth() => _maxMonth;
    public int GetTotalDayPass() => _totalDayPass;

}
