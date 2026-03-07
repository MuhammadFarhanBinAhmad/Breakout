using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AbilityDetail
{
    public SOAbilityEffect _brickAbilityList; // single SO for this tier
    public Button _abilityButton;
    public GameObject _abilityInfo;
}

[System.Serializable]
public class AbilityTreeDetail
{
    public BRICKABILITYTYPE _abilityType;
    public AbilityDetail[] _abilityDetail; // tiers for this ability tree, index is tier
}

public class UIStoreManager : AbstractStoreUI
{
    AbilityManager _abilityManager;
    TowerManager _towerManager;
    StoreManager _storeManager;

    public GameObject _storeUI,_OpenStoreIndicator;
    public Button _OpenStore,_CloseStore;

    [Header("Ability")]
    public AbilityTreeDetail[] _abilityContentDetail;

    private void Start()
    {
        _abilityManager = FindAnyObjectByType<AbilityManager>();
        _towerManager = FindAnyObjectByType<TowerManager>();
        _storeManager = FindAnyObjectByType<StoreManager>();

        storeUI = _storeUI;
        _CloseStore.onClick.AddListener(CloseStorePage);
        _OpenStore.onClick.AddListener(OpenStorePage);

        _storeManager.OnStoreOpen += StoreIsOpenIndicator;
        _storeManager.OnStoreClose += StoreIsCloseIndicator;

        // Wire each button and initialize interactable state
        for (int treeIndex = 0; treeIndex < _abilityContentDetail.Length; treeIndex++)
        {
            var tree = _abilityContentDetail[treeIndex];
            if (tree == null || tree._abilityDetail == null) continue;

            for (int tierIndex = 0; tierIndex < tree._abilityDetail.Length; tierIndex++)
            {
                int t = treeIndex;
                int ti = tierIndex;
                var btn = tree._abilityDetail[ti]._abilityButton;
                if (btn == null) continue;

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => PurchaseBrickAbility(t, ti));

                // set initial interactable state
                UpdateAbilityButton(t, ti);
            }
        }
    }
    private void OnDestroy()
    {

        _storeManager.OnStoreOpen -= StoreIsOpenIndicator;
        _storeManager.OnStoreClose -= StoreIsCloseIndicator;
    }

    // Called by button press
    public void PurchaseBrickAbility(int treeIndex, int tierIndex)
    {
        if (_abilityManager == null)
        {
            Debug.LogError("[UIStoreManager] AbilityManager not found.");
            return;
        }

        // Validate indices
        if (!IsValidTreeAndTier(treeIndex, tierIndex, out string invalidReason))
        {
            Debug.LogWarning(invalidReason);
            return;
        }

        var tree = _abilityContentDetail[treeIndex];
        var detail = tree._abilityDetail[tierIndex];
        var so = detail._brickAbilityList;



        // Check whether available for purchase
        if (!CanPurchaseTier(treeIndex, tierIndex, out string reason))
        {
            Debug.Log($"Cannot purchase ability: {reason}");
            // TODO: show UI feedback to player
            return;
        }

        //Ability level requirement
        var type = tree._abilityType;
        int currentlyTierLevel = _abilityManager.GetAbilityTierLevelIndex(type);
        int requiredAbilityLevel = _abilityManager._abilityLevelPreRequsite[currentlyTierLevel]; // default fallback
        if (_abilityManager.GetAbilityLevelIndex(type) < requiredAbilityLevel)
        {
            print($"Your ability level is {_abilityManager.GetAbilityLevelIndex(type)}. Upgrade ability to level {requiredAbilityLevel}");
            return;
        }
        //Cost requirement
        int _abilityCost = _storeManager._brickAbilityCostList[_abilityManager.GetCurrentBrickAbilityTierLevel()];
        if(_towerManager._currentPureEssence <  _abilityCost)
        {
            print($"Your total essence us not enough. Need {_abilityCost - _towerManager._totalEssenceCollected}");
            return;
        }

        _towerManager._currentPureEssence -= _abilityCost;
        _towerManager.OnEssenceCollect?.Invoke();

        // Perform purchase:
        _abilityManager.AddAbility(so); // instantiate and add ability
        _abilityManager.UpgradeAbilityTypeLevel(type);
        _abilityManager.UpgradeAbilityTierLevel(type);
        _abilityManager.IncreaseCurrentBrickAbilityAcquired();
        _abilityManager.IncreaseCurrentBrickAbilityTierLevel();

        // Refresh UI states (so next tier becomes interactable, button disabled, etc.)
        RefreshAllAbilityButtons();
    }

    bool IsValidTreeAndTier(int treeIndex, int tierIndex, out string reason)
    {
        reason = "";
        if (treeIndex < 0 || treeIndex >= _abilityContentDetail.Length)
        {
            reason = "Invalid tree index.";
            return false;
        }

        var tree = _abilityContentDetail[treeIndex];
        if (tree == null || tree._abilityDetail == null)
        {
            reason = "Tree/config missing.";
            return false;
        }

        if (tierIndex < 0 || tierIndex >= tree._abilityDetail.Length)
        {
            reason = "Invalid tier index.";
            return false;
        }

        if (tree._abilityDetail[tierIndex]._brickAbilityList == null)
        {
            reason = "Ability SO missing for this tier.";
            return false;
        }

        return true;
    }

    // Core purchase rules: must be next available tier AND player must meet required level
    bool CanPurchaseTier(int treeIndex, int tierIndex, out string reason)
    {
        reason = "";

        if (!IsValidTreeAndTier(treeIndex, tierIndex, out reason)) return false;

        var tree = _abilityContentDetail[treeIndex];
        var so = tree._abilityDetail[tierIndex]._brickAbilityList;
        var type = tree._abilityType;

        // 1) Must be the next tier to buy
        int currentlyTierLevel = _abilityManager.GetAbilityTierLevelIndex(type);
        if (tierIndex != currentlyTierLevel)
        {
            reason = $"You must buy tier {currentlyTierLevel} first (current owned: {currentlyTierLevel}).";
            return false;
        }





        return true;
    }

    // Update a single button's interactable state based on whether it's buyable
    void UpdateAbilityButton(int treeIndex, int tierIndex)
    {
        if (!IsValidTreeAndTier(treeIndex, tierIndex, out _)) return;

        var btn = _abilityContentDetail[treeIndex]._abilityDetail[tierIndex]._abilityButton;
        if (btn == null) return;

        bool canBuy = CanPurchaseTier(treeIndex, tierIndex, out _);
        btn.interactable = canBuy;
    }

    // Refresh all buttons (call when player levels up or after purchases)
    public void RefreshAllAbilityButtons()
    {
        for (int t = 0; t < _abilityContentDetail.Length; t++)
        {
            var tree = _abilityContentDetail[t];
            if (tree == null || tree._abilityDetail == null) continue;
            for (int ti = 0; ti < tree._abilityDetail.Length; ti++)
            {
                UpdateAbilityButton(t, ti);
            }
        }
    }

    public void StoreIsOpenIndicator() => _OpenStoreIndicator.SetActive(true);
    public void StoreIsCloseIndicator() => _OpenStoreIndicator?.SetActive(false);
    public void OpenStorePage() 
    {
        _storeUI?.SetActive(true);
        TimeManager.StopTime();
    }
    public void CloseStorePage()
    {
        _storeUI?.SetActive(false);
        TimeManager.StartTime();
    }

}
