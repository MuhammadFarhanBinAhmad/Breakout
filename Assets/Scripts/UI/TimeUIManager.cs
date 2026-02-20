using TMPro;
using UnityEngine;

public class TimeUIManager : MonoBehaviour
{
    TimeManager _timeManager;

    [Header("TimeText")]
    public TextMeshProUGUI _currentWeek;
    public TextMeshProUGUI _currentDay;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();

    }
    void Start()
    {
        _timeManager._weekPass += UpdateTimeUI;
        _timeManager._dayPass += UpdateTimeUI;

        UpdateTimeUI();
    }
    void UpdateTimeUI()
    {
        _currentWeek.text = "Week: " + _timeManager.GetCurrentWeek().ToString();

        switch (_timeManager.GetCurrentDay())
        {
            case 1:
                {
                    _currentDay.text = "Monday";
                    break;
                }
            case 2:
                {
                    _currentDay.text = "Tuesday";
                    break;
                }
            case 3:
                {
                    _currentDay.text = "Wednesday";
                    break;
                }
            case 4:
                {
                    _currentDay.text = "Thursday";
                    break;
                }
            case 5:
                {
                    _currentDay.text = "Friday";
                    break;
                }
            case 6:
                {
                    _currentDay.text = "Saturday";
                    break;
                }
            case 7:
                {
                    _currentDay.text = "SUNDAY";
                    break;
                }

        }
        


    }
}
