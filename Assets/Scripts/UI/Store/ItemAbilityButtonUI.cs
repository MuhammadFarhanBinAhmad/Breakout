using TMPro;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public class ItemAbilityButtonUI : MonoBehaviour, IPointerEnterHandler
{
    TowerManager _towerManager;
    TowerUIManager _towerUIManager;
    AbilityManager _abilityManager;
    ViewItemAbilityButtonUI _viewItemAbilityButtonUI;
    StoreAbilityManager _storeAbilityManager;
    internal SOItemAbilityContentUI _itemAbilityContent { get;private set; }

    [Header("UI Detail")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _selectButton;
    [SerializeField] private GameObject _lockedOverlay;
    internal int _purchaseCost { get; private set; }
    bool _isPurchase;

    private void Awake()
    {
        _viewItemAbilityButtonUI = FindAnyObjectByType<ViewItemAbilityButtonUI>();
        _towerManager = FindAnyObjectByType<TowerManager>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
        _storeAbilityManager = FindAnyObjectByType<StoreAbilityManager>();
        _towerUIManager = FindAnyObjectByType<TowerUIManager>();
    }
    void EnsureReferences()
    {
        if (_storeAbilityManager == null)
            _storeAbilityManager = FindAnyObjectByType<StoreAbilityManager>();

        if (_towerManager == null)
            _towerManager = FindAnyObjectByType<TowerManager>();

        if (_abilityManager == null)
            _abilityManager = FindAnyObjectByType<AbilityManager>();

        if (_viewItemAbilityButtonUI == null)
            _viewItemAbilityButtonUI = FindAnyObjectByType<ViewItemAbilityButtonUI>();
    }
    private void Start()
    {
        _selectButton.onClick.AddListener(PurchaseItem);
    }
    public void SetItemAbilityContent(SOItemAbilityContentUI content)
    {
        EnsureReferences();

        _itemAbilityContent = content;
        SetItemButton();
    }
    public void SetItemButton()
    {
        _icon.sprite = _itemAbilityContent.icon;
        _nameText.text = _itemAbilityContent.ability_Name.ToString();
        _purchaseCost = _storeAbilityManager.GetItemCost(_itemAbilityContent._itemRarity);
    }
    public void ResetButton()
    {
        _lockedOverlay.SetActive(false);
        _selectButton.interactable = true;
        _isPurchase = false;
    }
    public void DeactiveButton()
    {
        _lockedOverlay.SetActive(true);
        _selectButton.interactable = false;
    }
    public void ViewItemContent() => _viewItemAbilityButtonUI.SetViewItemButton(this);
    public SOAbilityEffect GetAbilityToSpawn() => _itemAbilityContent.ability_ToSpawn;
    public void PurchaseItem()
    {
        if (_towerManager._currentPureEssence >= _purchaseCost)
        {
            _towerManager.DeductPureEssence(_purchaseCost);
            _abilityManager.AddAbility(GetAbilityToSpawn());
            DeactiveButton();
            _viewItemAbilityButtonUI.SetContentToNull();
            _isPurchase = true;
            _towerUIManager.UpdateEssenceUI();
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!_isPurchase)
        ViewItemContent();
    }
}
