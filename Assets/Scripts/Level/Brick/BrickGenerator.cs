using System;
using System.Collections;
using System.Collections.Generic;
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

public class PlannedBrick
{
    public SO_BrickHealthStats stats;
    public Vector3 position;
}
public class WavePlan
{
    public List<PlannedBrick> bricks = new List<PlannedBrick>();
    public int totalAPUsed;
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

    [Header("BrickSpawn")]
    [SerializeField] AnimationCurve easeOutElastic;
    [SerializeField] float animationDuration;
    [SerializeField] float _capscaleMultiplier;
    Vector3 _startingScale = new Vector3(1,1,1);
    [Header("BrickSpawn")]
    [SerializeField] List<BrickModifierBase> _brickModifierList = new List<BrickModifierBase>();

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
        _timeManager._dayPass += CheckBrickToAdd;
        _timeManager._dayPass += SetAPOfTheDay;
        _onSpawnNextWave += SpawnNextWave;


        SetAttributePointForEachPhase();

    }

    private void Start()
    {
        CheckBrickToAdd();
        SetAPOfTheDay();
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
            phases = _timeManager.GetMaxGameDuration() ;
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
    void SetAPOfTheDay() => _APPerWaveForTheDay = _attributePoints[_timeManager.GetTotalDayPass()];
    void SpawnNextWave()
    {
        WavePlan plan = BuildWavePlan(GetBrickFormation());
        StartCoroutine(ExecuteWavePlan(plan));

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

    IEnumerator ExecuteWavePlan(WavePlan plan)
    {
        foreach (var p in plan.bricks)
        {
            GameObject brick = _brickPool.GetBrick();
            _brickPool.PlaceActiveBrickInList(brick);

            BrickBar bb = brick.GetComponent<BrickBar>();

            brick.transform.position = p.position;
            brick.transform.localScale = _startingScale;
            bb.SetBrick(p.stats);
            bb.AddModifier(_brickModifierList[0]);

            _brickCounter++;

            StartCoroutine(AnimateBrickSpawn(brick.transform));

            yield return null; // optional pacing
        }

        yield return new WaitForSeconds(_timerBeforeNextWaveSpawn);
        _onSpawnNextWave?.Invoke();
    }
    WavePlan BuildWavePlan(SOBrickFormation formation)
    {
        WavePlan plan = new WavePlan();

        int ap = _APPerWaveForTheDay;
        int x = 0;
        int y = 0;

        foreach (char c in formation.formation)
        {
            if (c == '\n')
            {
                y++;
                x = 0;
                continue;
            }

            if (c == '0')
            {
                x++;
                continue;
            }

            if (c == '1')
            {
                x++;

                var available = GetAffordableBricks(ap);

                if (available.Count == 0)
                    continue;

                var stats = available[UnityEngine.Random.Range(0, available.Count)];

                Vector3 pos =
                    transform.position +
                    new Vector3(_offset.x * (x + 0.5f), _offset.y * (y + 0.5f));

                plan.bricks.Add(new PlannedBrick
                {
                    stats = stats,
                    position = pos
                });

                ap -= stats._APValue;
            }
        }

        plan.totalAPUsed = _APPerWaveForTheDay - ap;

        return plan;
    }
    //IEnumerator SpawnFormation(SOBrickFormation formation)
    //{
    //    _currentWaveAP = _APPerWaveForTheDay;
    //    int x = 0;
    //    int y = 0;

    //    string test = "";
    //    foreach (char c in formation.formation)
    //    {
    //        test += c;
    //    }

    //    foreach (char c in formation.formation)
    //    {
    //        if (c == '0')
    //        {
    //            x++;
    //            continue;
    //        }
    //        if (c == '1')
    //        {
    //            if (_currentWaveAP <= 0)
    //            {
    //                //end wave cause out of points
    //                break;
    //            }
    //            x++;
    //            GameObject brick = _brickPool.GetBrick();
    //            _brickPool.PlaceActiveBrickInList(brick);
    //            BrickBar bb = brick.GetComponent<BrickBar>();
    //            brick.transform.localScale = _startingScale;
    //            brick.transform.position =
    //                transform.position +
    //                new Vector3(_offset.x * (x + 0.5f), _offset.y * (y + 0.5f));

    //            SetBrickStats(bb);
    //            _brickCounter++;
    //            StartCoroutine(AnimateBrickSpawn(brick.transform));
    //            continue;
    //        }

    //        if (c == '\n')
    //        {
    //            y++;
    //            x = 0;
    //            yield return new WaitForSeconds(_timerBeforeNextLineSpawn);

    //        }
    //    }

    //    yield return new WaitForSeconds(_timerBeforeNextWaveSpawn);
    //    _onSpawnNextWave?.Invoke();
    //}
    public void CheckBrickToAdd()
    {
        int day = _timeManager.GetTotalDayPass();

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
    //public SO_BrickHealthStats GetStatsForHealth(int currentHealth)
    //{
    //    if (_brickTypesList == null || _brickTypesList.Count == 0)
    //        return null;

    //    // Pick the highest health tier that is still <= currentHealth
    //    SO_BrickHealthStats best = null;

    //    for (int i = 0; i < _brickTypesList.Count; i++)
    //    {
    //        var stats = _brickTypesList[i];

    //        if (stats == null)
    //            continue;

    //        if (stats._health <= currentHealth)
    //        {
    //            if (best == null || stats._health > best._health)
    //                best = stats;
    //        }
    //    }

    //    // Fallback: if currentHealth is lower than all tiers, use the lowest one
    //    if (best == null)
    //    {
    //        for (int i = 0; i < _brickTypesList.Count; i++)
    //        {
    //            var stats = _brickTypesList[i];
    //            if (stats == null) continue;

    //            if (best == null || stats._health < best._health)
    //                best = stats;
    //        }
    //    }

    //    return best;
    //}
    //void SetBrickStats(BrickBar _bb)
    //{
    //    List<SO_BrickHealthStats> bhs = new List<SO_BrickHealthStats>();

    //    // Collect affordable bricks
    //    for (int i = 0; i < _brickAvailableToSpawn.Count; i++)
    //    {
    //        if (_brickAvailableToSpawn[i]._APValue <= _currentWaveAP)
    //            bhs.Add(_brickAvailableToSpawn[i]);
    //    }

    //    // SAFETY CHECK
    //    if (bhs.Count == 0)
    //    {
    //        return;
    //    }


    //    int type = UnityEngine.Random.Range(0, bhs.Count);
    //    SO_BrickHealthStats stats = bhs[type];

    //    _bb.SetBrick(stats);
    //    //_bb.AddModifier(_brickModifier[0]);

    //    _currentWaveAP -= stats._APValue;
    //}


    IEnumerator AnimateBrickSpawn(Transform brickTransform)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = _startingScale * _capscaleMultiplier;

        float time = 0f;

        while (time < animationDuration)
        {
            float normalized = time / animationDuration;
            float curveValue = easeOutElastic.Evaluate(normalized);

            brickTransform.localScale =
                Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        brickTransform.localScale = _startingScale;
    }

    List<SO_BrickHealthStats> GetAffordableBricks(int ap)
    {
        List<SO_BrickHealthStats> result = new List<SO_BrickHealthStats>();

        for (int i = 0; i < _brickAvailableToSpawn.Count; i++)
        {
            if (_brickAvailableToSpawn[i]._APValue <= ap)
                result.Add(_brickAvailableToSpawn[i]);
        }

        return result;
    }
}
