using UnityEngine;
using UnityEngine.UI;

public class StoreOverlayUI : BaseOverLayInteraction
{
    [Header("Store")]
    [SerializeField] Button _openStore;
    [SerializeField] Button _closeStore;
    [SerializeField] GameObject _storeOverlay;

    [Header("Ability")]
    [SerializeField] Button _openAbility;
    [SerializeField] Button _closeAbility;
    [SerializeField] GameObject _abilityOverlay;

    [Header("Item")]
    [SerializeField] Button _openItem;
    [SerializeField] Button _closeItem;
    [SerializeField] GameObject _itemOverlay;

    private void Start()
    {
        // Store
        _openStore.onClick.AddListener(() => OpenOverlay(_storeOverlay));
        _closeStore.onClick.AddListener(() => CloseOverlay(_storeOverlay));

        // Ability
        _openAbility.onClick.AddListener(() => OpenOverlay(_abilityOverlay));
        _closeAbility.onClick.AddListener(() => CloseOverlay(_abilityOverlay));

        //// Item
        //_openItem.onClick.AddListener(() => OpenOverlay(_itemOverlay));
        //_closeItem.onClick.AddListener(() => CloseOverlay(_itemOverlay));
    }
}
