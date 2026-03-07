using System;
using UnityEngine;
using UnityEngine.UI;

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
    public Action OnStoreOpen,OnStoreClose;
    public int _dayToOpen;
    public bool _storeIsOpen;

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
        _timeManager._dayPass += OpenStore;

        GenerateBrickAbilityCosts();
    }
    private void OnDisable()
    {
        _timeManager._dayPass -= OpenStore;
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
            float easedT = TweenService.GetEased(t,_growthType);

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
        if(_dayToOpen == _timeManager.GetCurrentDay() && !_storeIsOpen)
        {
            _storeIsOpen = true;
            OnStoreOpen?.Invoke();
            print("StoreOPEN");
        }
        else
        {
            _storeIsOpen = false;
            OnStoreClose?.Invoke();
            print("StoreClose");
        }
    }
}
