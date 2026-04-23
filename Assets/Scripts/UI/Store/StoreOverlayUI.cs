using UnityEngine;
using UnityEngine.UI;

public class StoreOverlayUI : BaseOverLayInteraction
{

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

        // Ability
        _openAbility.onClick.AddListener(() => OpenOverlay(_abilityOverlay));
        _closeAbility.onClick.AddListener(() => CloseOverlay(_abilityOverlay));
        // Item
        _openItem.onClick.AddListener(() => OpenOverlay(_itemOverlay));
        _closeItem.onClick.AddListener(() => CloseOverlay(_itemOverlay));
    }
}
