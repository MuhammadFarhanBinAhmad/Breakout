using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class ShopLevelState
{
    public int _level;
    public int _purchases;
    public int _baseCost;
    public float _multiplier = 1.25f;

    public int GetPrice()
    {
        return Mathf.RoundToInt(_baseCost * Mathf.Pow(_multiplier, _purchases));
    }

    public void RegisterPurchase() => _purchases++;
}

public class StoreAbilityManager : MonoBehaviour
{
    private TowerManager _towerManager;

    [Header("Ability Database")]
    [SerializeField] private List<SOStoreAbilityContent> allAbilities = new List<SOStoreAbilityContent>();

    [SerializeField] private ShopLevelState[] _abilityLevelState = new ShopLevelState[4];

    public List<string> unlockedAbilities = new List<string>();
    private Dictionary<string, SOStoreAbilityContent> abilityLookup;

    [Header("Store Open")]
    public Action OnStoreOpen, OnStoreClose;
    public bool _storeIsOpen;


    private void Awake()
    {
        _towerManager = FindAnyObjectByType<TowerManager>();

        for (int i = 0; i < _abilityLevelState.Length; i++)
        {
            if (_abilityLevelState[i] == null)
            {
                _abilityLevelState[i] = new ShopLevelState();
            }

            _abilityLevelState[i]._level = i;
        }

        abilityLookup = allAbilities
            .Where(a => a != null && !string.IsNullOrEmpty(a.abilityID))
            .ToDictionary(a => a.abilityID, a => a);
    }

    public bool CanPurchase(string abilityID)
    {
        if (!abilityLookup.ContainsKey(abilityID))
            return false;

        if (unlockedAbilities.Contains(abilityID))
            return false;

        SOStoreAbilityContent ability = abilityLookup[abilityID];
        int abilityLevel = ability.ability_Level;

        if (abilityLevel < 0 || abilityLevel >= _abilityLevelState.Length)
            return false;

        int price = _abilityLevelState[abilityLevel].GetPrice();

        if (_towerManager.GetTotalPureEssenceCount() < price)
            return false;

        if (!ability._availableToPurchaseAtStart)
        {
            foreach (string requiredID in ability.requiredAbilityIDs)
            {
                if (!unlockedAbilities.Contains(requiredID))
                    return false;
            }
        }

        return true;
    }

    public bool PurchaseAbility(string abilityID)
    {
        if (!CanPurchase(abilityID))
            return false;

        SOStoreAbilityContent ability = abilityLookup[abilityID];
        int abilityLevel = ability.ability_Level;

        int price = _abilityLevelState[abilityLevel].GetPrice();

        _towerManager.DeductPureEssence(price);
        unlockedAbilities.Add(abilityID);

        _abilityLevelState[abilityLevel].RegisterPurchase();

        print($"Purchased ability: {ability.ability_Name}");

        return true;
    }

    public bool IsUnlocked(string abilityID)
    {
        return unlockedAbilities.Contains(abilityID);
    }

    public bool IsAvailableToPurchase(string abilityID)
    {
        if (!abilityLookup.ContainsKey(abilityID))
            return false;

        SOStoreAbilityContent ability = abilityLookup[abilityID];

        if (ability._availableToPurchaseAtStart)
            return true;

        foreach (string requiredID in ability.requiredAbilityIDs)
        {
            if (!unlockedAbilities.Contains(requiredID))
                return false;
        }

        return true;
    }

    void OpenStore()
    {
        if (!_storeIsOpen)
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
    public List<SOStoreAbilityContent> GetAbilityList() => new List<SOStoreAbilityContent>(allAbilities);

    public int GetAbilityCost(int level)
    {
        if (level < 0 || level >= _abilityLevelState.Length || _abilityLevelState[level] == null)
            return 0;

        return _abilityLevelState[level].GetPrice();
    }
}