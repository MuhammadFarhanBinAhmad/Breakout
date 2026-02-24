using System;
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

    [Header("In-Game Clock")]
    [SerializeField] bool _printClock = true;

    int _currentHour;
    int _currentMinute;
    bool _isPM;
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

        UpdateInGameClock();

    }

    void UpdateInGameClock()
    {
        // Normalize day progress (0–1)
        float dayProgress = GetDayNormalized();

        // Convert to total in-game minutes (24h * 60m)
        float totalMinutes = dayProgress * 1440f;

        // Snap to 10-minute intervals
        int snappedMinutes = Mathf.FloorToInt(totalMinutes / 10f) * 10;

        int hour24 = snappedMinutes / 60;
        int minute = snappedMinutes % 60;

        // Convert to 12-hour format
        _isPM = hour24 >= 12;
        _currentHour = hour24 % 12;
        if (_currentHour == 0) _currentHour = 12;

        _currentMinute = minute;

        if (_printClock)
        {
            Debug.Log(
                $"In-Game Time: {_currentHour:D2}:{_currentMinute:D2} {(_isPM ? "PM" : "AM")}"
            );
        }
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
        day = 1;       
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
    public int GetMaxWeek() => _maxWeek;
    public int GetMaxDay() => _maxDay;
    public int GetTotalDayPass() => _totalDayPass;

}
