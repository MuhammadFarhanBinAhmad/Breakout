using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public enum STORETYPE
{
    BRICK,
    BALL,
    PADDLE_AND_BOX
}
public class StoreManager : MonoBehaviour
{

    TimeManager _timeManager;


    [Header("Store Open")]
    public int _weekToOpen;
    public bool _storeIsOpen;
    public bool _storeOpenThisMonth;

    [Header("Brick Ability Content")]
    public Button[] _abilityButton;
    public GameObject _abilityInfo;

    [Header("Brick Upgrade cost")]
    [SerializeField] TWEENTYPE _growthType;
    [Tooltip("Brick uses essence")]
    public int _brickAbilityStartCost, _brickAbilityEndCost;
    public int[] _brickAbilityCostList = new int[12];


    private void Start()
    {
        _timeManager = FindAnyObjectByType<TimeManager>();
        _timeManager._weekPass += OpenStore;

        GenerateBrickAbilityCosts();
    }
    private void OnDisable()
    {
        _timeManager._weekPass -= OpenStore;
    }
    public void GenerateBrickAbilityCosts()
    {
        if (_brickAbilityCostList == null || _brickAbilityCostList.Length == 0)
            return;

        int count = _brickAbilityCostList.Length;

        for (int i = 0; i < count; i++)
        {
            // Normalize index into 0–1 range
            float t = (count == 1) ? 1f : (float)i / (count - 1);

            // Apply your easing curve
            float easedT = GetEased(t);

            // Convert eased value into a cost
            int cost = Mathf.RoundToInt(
                Mathf.Lerp(_brickAbilityStartCost, _brickAbilityEndCost, easedT)
            );

            _brickAbilityCostList[i] = cost;
        }

        // Safety pass: enforce strictly increasing values
        for (int i = 1; i < count; i++)
        {
            if (_brickAbilityCostList[i] <= _brickAbilityCostList[i - 1])
            {
                _brickAbilityCostList[i] = _brickAbilityCostList[i - 1] + 1;
            }
        }
    }
    public int GetBrickAbilityCost(int tier) => _brickAbilityCostList[tier];
    void OpenStore()
    {
        if(_weekToOpen == _timeManager.GetCurrentWeek() && !_storeIsOpen)
        {
            _storeIsOpen = true;
            print("StoreOPEN");
        }
        else
        {
            _storeIsOpen = false;
            print("StoreClose");
        }
    }
    float GetEased(float t)
    {
        t = Mathf.Clamp01(t);
        switch (_growthType)
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
