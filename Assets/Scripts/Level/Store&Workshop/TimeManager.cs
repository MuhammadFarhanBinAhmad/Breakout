using System;
using TMPro.EditorUtilities;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [Header("TimeKeeper")]

    [SerializeField] int _maxGameDuration;
    int _totalDayPass;
    [SerializeField] float _fullDayDuration;
    [SerializeField] float _currentDayDuration;

    public Action _dayPass;
    public Action _endGame;

    [Header("RealTime")]
    [SerializeField] float _currentRealTimePass;

    private void Start()
    {
        _dayPass += PlayDayPassAudio;
    }
    private void OnDisable()
    {
        _dayPass -= PlayDayPassAudio;
    }
    private void Update()
    {
        CountDayTime();
        _currentRealTimePass += Time.deltaTime;
    }
    public void CountDayTime()
    {
        if(_currentDayDuration > 0)
        {
            _currentDayDuration -= Time.deltaTime;
        }
        else
        {
            _totalDayPass++;
            _currentDayDuration = _fullDayDuration;
            _dayPass?.Invoke();
            
        }
    }
    public float GetDayNormalized()
    {
        return 1f - (_currentDayDuration / _fullDayDuration);
    }
    public static void StopTime() => Time.timeScale = 0f;
    public static void ResetTimeScale() => Time.timeScale = 1f;
    public static void SetCustomTimeScale(float val) => Time.timeScale = val;
    public float GetCurrentRealTime () => _currentRealTimePass;
    public int GetTotalDayPass() => _totalDayPass;
    public int GetMaxGameDuration() => _maxGameDuration;
    public void PlayDayPassAudio() => AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onNewDay, transform.position);
    

}
