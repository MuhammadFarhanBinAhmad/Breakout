using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BallAbilityButtonUI : BaseButtonInteraction
{
    AbilityManager _abilityManager;
    AbilityStoreLayoutUI _abilityStoreUI;
    TowerUIManager _towerUIManager;


    private SOStoreAbilityContent _abilityData;
    private StoreAbilityManager _storeAbilityManager;

    [Header("UI Detail")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _costText;
    [SerializeField] private Button _purchaseButton;
    [SerializeField] private GameObject _lockedOverlay;
    [SerializeField] private GameObject _abilityDescription;
    bool _abilityPurchased;

    public Action OnAbilityPurchase;

    private void Start()
    {
        _abilityManager = FindAnyObjectByType<AbilityManager>();
        _abilityStoreUI = FindAnyObjectByType<AbilityStoreLayoutUI>();
        _towerUIManager = FindAnyObjectByType<TowerUIManager>();

        OnAbilityPurchase += _abilityStoreUI.RefreshAll;
        OnAbilityPurchase += _towerUIManager.UpdateEssenceUI;
    }
    private void OnDestroy()
    {
        OnAbilityPurchase -= _abilityStoreUI.RefreshAll;
        OnAbilityPurchase -= _towerUIManager.UpdateEssenceUI;

    }
    public void Setup(SOStoreAbilityContent ability, StoreAbilityManager manager)
    {
        _abilityData = ability;
        _storeAbilityManager = manager;
        _icon.sprite = _abilityData.icon;
        _titleText.text = _abilityData.ability_Name;
        _descriptionText.text = _abilityData.ability_Description;
        _costText.text = _storeAbilityManager.GetAbilityCost(ability.ability_Level).ToString();

        _purchaseButton.onClick.RemoveAllListeners();
        _purchaseButton.onClick.AddListener(OnPurchaseClicked);
        _purchaseButton.onClick.AddListener(base.OnButtonClick);


        Refresh();
    }
    
    private void OnEnable()
    {
        Refresh();
    }
    public void Refresh()
    {
        if (_abilityPurchased)
            return;

        bool unlocked = _storeAbilityManager.IsUnlocked(_abilityData.abilityID);
        bool AvailableToPurchase = _storeAbilityManager.IsAvailableToPurchase(_abilityData.abilityID);
        bool canBuy = _storeAbilityManager.CanPurchase(_abilityData.abilityID);


        _purchaseButton.interactable = canBuy;
        _lockedOverlay.SetActive(!canBuy && !unlocked);

        if (unlocked)
        {
            _costText.text = "Owned";
        }
        else
        {
            _costText.text = _storeAbilityManager.GetAbilityCost(_abilityData.ability_Level).ToString();
        }
    }

    private void OnPurchaseClicked()
    {
        _abilityManager.AddAbility(_abilityData.ability_ToSpawn);
        if (_storeAbilityManager.PurchaseAbility(_abilityData.abilityID))
        {
            OnAbilityPurchase?.Invoke();
            _abilityPurchased = true;
        }
    }
    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        _abilityDescription.SetActive(true);
    }
    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        _abilityDescription.SetActive(false);
    }
}
