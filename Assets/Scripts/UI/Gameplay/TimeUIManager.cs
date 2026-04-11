using TMPro;
using UnityEngine;

public class TimeUIManager : MonoBehaviour
{
    TimeManager _timeManager;

    [Header("TimeText")]
    public TextMeshProUGUI text_dayLeft;


    [Header("In-Game Clock")]
    public TextMeshProUGUI text_currentTime;

    int _currentHour;
    int _currentMinute;
    bool _isPM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();

    }
    private void Update()
    {
        UpdateInGameClock();
    }
    void Start()
    {
        _timeManager._dayPass += UpdateTimeUI;

        UpdateTimeUI();
    }
    private void OnDestroy()
    {
        _timeManager._dayPass -= UpdateTimeUI;
    }

    void UpdateInGameClock()
    {
        // Normalize day progress (0–1)
        float dayProgress = _timeManager.GetDayNormalized();

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

        text_currentTime.text =
                $"{_currentHour:D2}:{_currentMinute:D2} {(_isPM ? "PM" : "AM")}";
    }
    void UpdateTimeUI()
    {
        text_dayLeft.text = "Days Left: " + (_timeManager.GetMaxGameDuration() - _timeManager.GetTotalDayPass()).ToString();
    }
}
