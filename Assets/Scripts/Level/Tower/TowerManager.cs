using System;
using UnityEngine;

public class TowerManager : MonoBehaviour
{

    TimeManager _timeManager;


    public GameObject _collectedBrick;

    [Header("Essence")]
    [Tooltip("Starting threshold of essence required to trigger an OnEssenceCollect.")]
    public int _initialEssenceThreshold = 5;
    [Tooltip("Current threshold; starts at initial and is increased on milestones.")]
    public int _essenceThreshold;
    public int _totalEssenceCollected;
    public int _currentEssenceCount;
    public int _currentPureEssence;
    public Action OnEssenceCollect;

    [Header("Essence scaling (milestones)")]
    public TWEENTYPE _essenceTweenType = TWEENTYPE.LINEAR;
    [Tooltip("How many floors make 1 milestone (default 5).")]
    public int milestoneFloors = 5;
    [Tooltip("Base increase applied each milestone (multiplied by eased progress).")]
    public int _essenceIncreaseBase = 2;

    [Header("Brick")]
    public int _totalBrickCount;
    public int _brickThreshold;
    int _currentBrickCount;
    public Action OnBrickIncrease;
    public Action OnBrickDecrease;
    public Vector2 _posOffset;

    [Header("Layer")]
    public int _currentTowerHeight;
    public Action OnHeightIncrease;

    [Header("MonthlyCheck")]
    public TWEENTYPE _towerTweenType = TWEENTYPE.LINEAR;
    public Action _OnGameOver;
    [SerializeField] int _startTowerHeightCheck, _endTowerHeightCheck;
    [SerializeField] int _totalTowerHeightCheck;
    public int[] _towerHeightCheck;
    bool _receiveWarning;


    void Awake()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();
        _essenceThreshold = _initialEssenceThreshold;

    }
    private void Start()
    {
        // subscribe to month pass if TimeManager exposes this
        if (_timeManager != null)
            _timeManager._weekPass += EndOfWeekCheck;

        OnEssenceCollect += IncreaseBrickCount;
        OnHeightIncrease += CreateNewFloor;

        PopulateTowerHeightCheck();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        // ensure array is updated in editor without entering playmode
        PopulateTowerHeightCheck();
    }
#endif
    private void OnDisable()
    {
        _timeManager._weekPass -= EndOfWeekCheck;
        OnEssenceCollect -= IncreaseBrickCount;
        OnHeightIncrease -= CreateNewFloor;
    }

    public void IncreaseEssenceCount(int amt)
    {
        _currentEssenceCount += amt;

        if (_currentEssenceCount >= _essenceThreshold)
        {
            _currentEssenceCount = 0;
            //_totalEssenceCollected += _essenceThreshold;
            _totalBrickCount++;
            _currentBrickCount++;
            _currentPureEssence++;
        }
        OnEssenceCollect?.Invoke();
    }
    public void IncreaseBrickCount()
    {

        OnBrickIncrease?.Invoke();

        if (_currentBrickCount >= _brickThreshold)
        {
            _currentBrickCount = 0;
            _currentTowerHeight++;

            // Apply milestone logic: every milestoneFloors floors, increase essence threshold
            if (milestoneFloors > 0 && _currentTowerHeight % milestoneFloors == 0)
            {
                ApplyEssenceMilestoneIncrease();
            }

            OnHeightIncrease?.Invoke();
        }

        GameObject brick = Instantiate(_collectedBrick);
        Vector3 pos = new Vector2(transform.position.x + (_posOffset.x * _currentBrickCount), transform.position.y);
        brick.transform.position = pos;
    }

    void ApplyEssenceMilestoneIncrease()
    {
        // compute which milestone we are at (1-based)
        int milestoneIndex = Mathf.FloorToInt((float)_currentTowerHeight / milestoneFloors);

        // use TimeManager's max month as a normaliser to build progress for the easing curve
        int maxPhases;
        maxPhases = _timeManager.GetMaxWeek();

        // progress in [0,1] = milestoneIndex / maxPhases (clamped)
        float progress = (float)milestoneIndex / (float)maxPhases;
        progress = Mathf.Clamp01(progress);

        float eased = TweenService.GetEased(progress, _essenceTweenType);

        // compute increase (at least 1)
        int increase = Mathf.Max(1, Mathf.RoundToInt(_essenceIncreaseBase * eased));

        _essenceThreshold += increase;

    }

    void CreateNewFloor()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y - (_posOffset.y * _currentTowerHeight));
    }
    public void EndOfWeekCheck()
    {
        if(_currentTowerHeight >= _towerHeightCheck[_timeManager.GetCurrentWeek()])
        {
            print("pass");
        }
        else
        {
            if (!_receiveWarning)
                _receiveWarning = true;
            else
            {
                _OnGameOver?.Invoke();
                TimeManager.StopTime();
                print("fail");
            }
        }
    }
    void PopulateTowerHeightCheck()
    {
        if (_totalTowerHeightCheck <= 0)
        {
            _towerHeightCheck = new int[0];
            return;
        }

        _towerHeightCheck = new int[_totalTowerHeightCheck];

        // if only one sample, use start value
        if (_totalTowerHeightCheck == 1)
        {
            _towerHeightCheck[0] = _startTowerHeightCheck;
            return;
        }

        int steps = _totalTowerHeightCheck - 1; // denom so last element = end value
        for (int i = 0; i < _totalTowerHeightCheck; i++)
        {
            float t = (float)i / (float)steps;               // normalized [0,1]
            float eased = TweenService.GetEased(t, _towerTweenType);      // apply chosen easing
            float val = Mathf.Lerp(_startTowerHeightCheck, _endTowerHeightCheck, eased);
            _towerHeightCheck[i] = Mathf.RoundToInt(val);    // integer thresholds
        }
    }

    
}
