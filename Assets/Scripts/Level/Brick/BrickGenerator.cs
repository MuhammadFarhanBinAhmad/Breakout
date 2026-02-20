using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum TWEENTYPE
{
    NONE,
    LINEAR,
    SINE,
    QUAD,
    EXPO,
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

    public List<BrickFormationEntry> _brickFormationList;

    [Header("AttributePoints")]
    [SerializeField] int _firstAttributePoints;
    [SerializeField] int _lastAttributePoints;
    public int[] _attributePoints;

    [Header("Brick position")]
    public Vector2Int _size;
    public Vector2 _offset;

    [Header("Level and Wave")]
    public List<int> _spawnedWaves = new List<int>();
    public GameObject _brickPrefab;
    [SerializeField] float _timerBeforeNextLineSpawn;
    public int _brickCounter;
    public int _currentWave;

    [Header("Timer before next wave spawn")]
    [SerializeField] float _timerBeforeNextWaveSpawn;
    public Action _onSpawnNextWave;


    [Header("SetBrickHealth")]
    [SerializeField] TWEENTYPE _healthType;
    [SerializeField] int _firstStartValueHealth;
    [SerializeField] int _lastStartValueHealth;
    [SerializeField] int _firstEndValueHealth;
    [SerializeField] int _lastEndValueHealth;

    [SerializeField] int[] _healthStartValue_PerPhase;
    [SerializeField] int[] _healthEndValue_PerPhase;

    [Header("SetBrickHealth")]
    [SerializeField] int _slowSpeed;
    [SerializeField] int _NormalSpeed;
    [SerializeField] int _FastSpeed;

    // Tweak this to control how wide the health range above min can be
    [Header("Health tuning")]
    [Tooltip("How many health points per attribute point to allow above the phase-min health.")]
    [SerializeField] float _healthRangeMultiplier = 1f;

    private void Awake()
    {
        _brickPool = GetComponent<BrickPool>();
        _time_manager_find();
        _onSpawnNextWave += SpawnNextWave;
    }

    private void Start()
    {
        SetHealthForEachPhase();
        SetAttributePointForEachPhase();
        _onSpawnNextWave?.Invoke();
    }
    private void OnDisable()
    {
        _onSpawnNextWave -= SpawnNextWave;
    }

    void _time_manager_find()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();
        if (_timeManager == null)
            Debug.LogWarning("TimeManager not found on Awake.");
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

    void SetHealthForEachPhase()
    {
        int phases = 1;
        if (_timeManager != null)
            phases = _timeManager.GetMaxWeek() * _timeManager.GetMaxDay();
        else
            Debug.LogWarning("TimeManager not found when generating health per phase. Defaulting to 1 phase.");

        // Ensure arrays have correct size
        _healthStartValue_PerPhase = new int[phases];
        _healthEndValue_PerPhase = new int[phases];

        for (int i = 0; i < phases; i++)
        {
            // tStart at the beginning of this phase, tEnd at the end of this phase
            float tStart = (phases == 1) ? 0f : (float)i / (phases - 1);
            float tEnd = (phases == 1) ? 1f : ((i == phases - 1) ? 1f : (float)(i + 1) / (phases - 1));

            float easedStart = GetEased(tStart);
            float easedEnd = GetEased(tEnd);

            float startVal = Mathf.Lerp(_firstStartValueHealth, _firstEndValueHealth, easedStart);
            float endVal = Mathf.Lerp(_lastStartValueHealth, _lastEndValueHealth, easedEnd);

            _healthStartValue_PerPhase[i] = Mathf.RoundToInt(startVal);
            _healthEndValue_PerPhase[i] = Mathf.RoundToInt(endVal);
        }
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

            float easedStart = GetEased(tStart);

            float val = Mathf.Lerp(_firstAttributePoints, _lastAttributePoints, easedStart);

            _attributePoints[i] = Mathf.RoundToInt(val);
        }
    }

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
        int x = 0;
        int y = 0;

        foreach (char c in formation.formation)
        {
            x++;

            if (c == '1')
            {
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

    void SetBrickStats(BrickBar _bb)
    {
        //0-normal
        //1-fast
        //2-tank
        int _brickType = UnityEngine.Random.Range(0, 3); // fixed to include 2
        int _healthBudget = 0;
        int _speedBudget = 0;

        switch (_brickType)
        {
            case 0: // normal
                _healthBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.5f);
                _speedBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.5f);
                break;

            case 1: // fast
                _healthBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.2f);
                _speedBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.8f);
                break;

            case 2: // tank
                _healthBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.8f);
                _speedBudget = Mathf.RoundToInt(_attributePoints[_timeManager.GetTotalDayPass()] * 0.2f);
                break;
        }

        // --- REPLACED: Instead of using real-time phaseElapsed/phaseDuration,
        // we compute a normalized t based on current week progress inside the phase.
        float t = 0f;
        if (_timeManager != null)
        {
            int currentWeek = Mathf.Max(1, _timeManager.GetCurrentWeek());
            int maxWeek = Mathf.Max(1, _timeManager.GetMaxWeek());
            // normalized 0..1 through current phase (week-1)/(maxWeek-1)
            t = (maxWeek <= 1) ? 0f : ((currentWeek - 1f) / (maxWeek - 1f));
            t = Mathf.Clamp01(t);
        }

        // --- Simplified health calculation ---
        float minHealth = EvaluatePhaseValue(
            t,
            _healthStartValue_PerPhase[_timeManager.GetTotalDayPass()],
            _healthEndValue_PerPhase[_timeManager.GetTotalDayPass()]
        );

        // Max health is a simple function of minHealth + attribute-based range.
        float maxHealth = minHealth + _attributePoints[_timeManager.GetTotalDayPass()] * _healthRangeMultiplier;

        // Simple mapping: allocate budget linearly on top of minHealth, clamped to maxHealth.
        float finalHealth = minHealth + _healthBudget;
        finalHealth = Mathf.Clamp(finalHealth, minHealth, maxHealth);

        // --- SPEED (unchanged, kept for context) ---
        // Tunables
        float speedBase = 2f;
        float speedExponent = 1.5f;
        float softMultiplier = 1.5f;

        float finalSpeed;

        // Phase-based speed caps (simple version for now)
        if (_timeManager.GetCurrentWeek() > 2)
        {
            float minSpeed = _slowSpeed;
            float softSpeed = _NormalSpeed + _timeManager.GetCurrentWeek();
            float hardSpeed = _FastSpeed + _timeManager.GetCurrentWeek();


            float costToSoftSpeed =
                speedBase * Mathf.Pow(softSpeed, speedExponent);

            if (_speedBudget <= costToSoftSpeed)
            {
                finalSpeed =
                    Mathf.Pow(_speedBudget / speedBase, 1f / speedExponent);
            }
            else
            {
                float remaining = _speedBudget - costToSoftSpeed;
                finalSpeed =
                    softSpeed +
                    Mathf.Pow(
                        remaining / (speedBase * softMultiplier),
                        1f / speedExponent
                    );
            }

            finalSpeed = Mathf.Clamp(finalSpeed, minSpeed, hardSpeed);
        }
        else
        {
            finalSpeed = 1;
        }
        

        //Assign to brick
        _bb._startingHealth = Mathf.RoundToInt(finalHealth);
        _bb._health = _bb._startingHealth;
        _bb._fallSpeed = finalSpeed;
    }

    float EvaluatePhaseValue(
        float t,
        float startValue,
        float endValue)
    {
        float easedT = GetEased(t);
        return Mathf.Lerp(startValue, endValue, easedT);
    }

    //Helper function
    float GetEased(float t)
    {
        t = Mathf.Clamp01(t);
        switch (_healthType)
        {
            case TWEENTYPE.LINEAR:
                return Linear(t);
            case TWEENTYPE.SINE:
                return Sine(t);
            case TWEENTYPE.EXPO:
                return Expo(t);
            case TWEENTYPE.QUAD:
                return Quad(t);
            case TWEENTYPE.NONE:
            default:
                return Linear(t); // treat NONE as linear by default
        }
        float Linear(float tt)
        {
            return Mathf.Clamp01(tt);
        }
        float Quad(float tt)
        {
            tt = Mathf.Clamp01(tt);
            if (tt < 0.5f)
                return 2f * tt * tt;
            else
                return -2f * tt * tt + 4f * tt - 1f;
        }
        float Sine(float tt)
        {
            tt = Mathf.Clamp01(tt);
            return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * tt);
        }
        float Expo(float tt)
        {
            tt = Mathf.Clamp01(tt);

            if (tt == 0f) return 0f;
            if (tt == 1f) return 1f;

            if (tt < 0.5f)
                return 0.5f * Mathf.Pow(2f, (20f * tt) - 10f);
            else
                return 1f - 0.5f * Mathf.Pow(2f, (-20f * tt) + 10f);
        }
    }

}
