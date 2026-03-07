using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum BRICKLAYER
{
    NONE,
    RED,
    GREEN, 
    BLUE,
    ORANGE,
    PINK
}


[System.Serializable]
public class BrickFormationEntry
{
    public List<SOBrickFormation> formations;
}
public class BrickGenerator : MonoBehaviour
{
    BrickPool _brickPool;
    TimeManager _timeManager;

    public List<BrickFormationEntry> _brickFormationList = new List<BrickFormationEntry>();

    public List<BrickModifierBase> _brickModifier = new List<BrickModifierBase>();
    public List<SO_BrickHealthStats> _brickTypesList;
    public List<SO_BrickHealthStats> _brickAvailableToSpawn = new List<SO_BrickHealthStats>();

    [Header("AttributePoints")]
    TWEENTYPE _APTweenType;
    [SerializeField] int _firstAttributePoints;
    [SerializeField] int _lastAttributePoints;
    public int[] _attributePoints;
    int _APPerWaveForTheDay;
    [Header("Brick position")]
    public Vector2Int _size;
    public Vector2 _offset;

    [Header("Level and Wave")]
    public List<int> _spawnedWaves = new List<int>();
    public GameObject _brickPrefab;
    [SerializeField] float _timerBeforeNextLineSpawn;
    public int _brickCounter;
    public int _currentWave;
    int _currentWaveAP;

    [Header("Timer before next wave spawn")]
    [SerializeField] float _timerBeforeNextWaveSpawn;
    public Action _onSpawnNextWave;


    private void Awake()
    {
        _brickPool = GetComponent<BrickPool>();
        _timeManager = FindAnyObjectByType<TimeManager>();
        _onSpawnNextWave += SpawnNextWave;
        _timeManager._dayPass += CheckBrickToAdd;
        _timeManager._dayPass += SetAPOfTheDay;
    }

    private void Start()
    {
        SetAttributePointForEachPhase();
        _onSpawnNextWave?.Invoke();
    }
    private void OnDisable()
    {
        _onSpawnNextWave -= SpawnNextWave;
        _timeManager._dayPass -= CheckBrickToAdd;
        _timeManager._dayPass -= SetAPOfTheDay;
    }

    public void OnBrickDestroyed()
    {
        _brickCounter--;
    }

    public SOBrickFormation GetBrickFormation()
    {
        var formations = _brickFormationList[0].formations;

        if (formations == null || formations.Count == 0)
        {
            Debug.LogWarning($"Brick formation list is empty for level {0}");
            return null;
        }

        // Fast lookup of already spawned indices
        var used = new HashSet<int>(_spawnedWaves);

        // Build list of available indices
        var available = new List<int>(formations.Count);
        for (int i = 0; i < formations.Count; i++)
        {
            if (!used.Contains(i))
                available.Add(i);
        }

        // Pick a random index from the remaining ones
        int pick = available[UnityEngine.Random.Range(0, available.Count)];
        _spawnedWaves.Add(pick);

        return formations[pick];
    }

    void SetAttributePointForEachPhase()
    {
        int phases = 1 ;
        if (_timeManager != null)
            phases = _timeManager.GetMaxWeek() * _timeManager.GetMaxDay();
        else
            Debug.LogWarning("TimeManager not found when generating health per phase. Defaulting to 1 phase.");

        // Ensure arrays have correct size
        _attributePoints = new int[phases];

        for (int i = 0; i < phases; i++)
        {
            float tStart = (phases == 1) ? 0f : (float)i / (phases - 1);

            float easedStart = TweenService.GetEased(tStart, _APTweenType);

            float val = Mathf.Lerp(_firstAttributePoints, _lastAttributePoints, easedStart);

            _attributePoints[i] = Mathf.RoundToInt(val);
        }
    }
    void SetAPOfTheDay() => _APPerWaveForTheDay = _attributePoints[_timeManager.GetCurrentDay()];
    void SpawnNextWave()
    {
        StartCoroutine(SpawnFormation(GetBrickFormation()));

        if (_currentWave >= _brickFormationList[0].formations.Count - 1)
        {
            _currentWave = 0;
            _spawnedWaves.Clear();
        }
        else
        {
            _currentWave++;
        }
    }

    IEnumerator SpawnFormation(SOBrickFormation formation)
    {
        _currentWaveAP = _APPerWaveForTheDay;
        int x = 0;
        int y = 0;

        foreach (char c in formation.formation)
        {
            x++;
            
            if (c == '1')
            {
                if (_currentWaveAP <= 0)
                {
                    print("out of ap");
                    break;
                }
                GameObject brick = _brickPool.GetBrick();
                _brickPool.PlaceActiveBrickInList(brick);
                BrickBar bb = brick.GetComponent<BrickBar>();

                brick.transform.position =
                    transform.position +
                    new Vector3(_offset.x * (x + 0.5f), _offset.y * (y + 0.5f));

                SetBrickStats(bb);
                _brickCounter++;

            }

            if (c == '\n')
            {
                yield return new WaitForSeconds(_timerBeforeNextLineSpawn);
                y++;
                x = 0;
            }
        }

        yield return new WaitForSeconds(_timerBeforeNextWaveSpawn);
        _onSpawnNextWave?.Invoke();
    }
    public void CheckBrickToAdd()
    {
        int day = _timeManager.GetCurrentDay();

        for (int i = 0; i < _brickTypesList.Count; i++)
        {
            // 1. Skip if already unlocked
            if (_brickAvailableToSpawn.Contains(_brickTypesList[i]))
                continue;

            // 2. Unlock if day matches
            if (_brickTypesList[i]._daytoUnlock == day)
            {
                _brickAvailableToSpawn.Add(_brickTypesList[i]);
            }
        }
    }

    void SetBrickStats(BrickBar _bb)
    {
        List<SO_BrickHealthStats> bhs = new List<SO_BrickHealthStats>();

        // Collect affordable bricks
        for (int i = 0; i < _brickAvailableToSpawn.Count; i++)
        {
            if (_brickAvailableToSpawn[i]._APValue <= _currentWaveAP)
                bhs.Add(_brickAvailableToSpawn[i]);
        }

        // SAFETY CHECK
        if (bhs.Count == 0)
        {
            Debug.LogWarning("No bricks available for current AP!");
            return;
        }


        int type = UnityEngine.Random.Range(0, bhs.Count);
        SO_BrickHealthStats stats = bhs[type];

        _bb.SetBrick(stats);
        _bb.AddModifier(_brickModifier[0]);

        _currentWaveAP -= stats._APValue;
        Debug.Log("AP left: " + _currentWaveAP);
    }
}
