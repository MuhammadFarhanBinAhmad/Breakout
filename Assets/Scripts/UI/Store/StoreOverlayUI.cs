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
        _openStore.onClick.AddListener(() => OpenOverlayWithButton(_storeOverlay));
        _closeStore.onClick.AddListener(() => CloseOverlayWithButton(_storeOverlay));

        // Ability
        _openAbility.onClick.AddListener(() => OpenOverlay(_abilityOverlay));
        _closeAbility.onClick.AddListener(() => CloseOverlay(_abilityOverlay));

        //// Item
        _openItem.onClick.AddListener(() => OpenOverlay(_itemOverlay));
        _closeItem.onClick.AddListener(() => CloseOverlay(_itemOverlay));
    }

    void OpenOverlayWithButton(GameObject _overlay)
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_openOverlay, transform.position);
        _closeStore.gameObject.SetActive(true);
        _overlay.SetActive(true);
    }
    void CloseOverlayWithButton(GameObject _overlay)
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_closeOverlay, transform.position);
        _closeStore.gameObject.SetActive(false);
        _overlay.SetActive(false);
    }
}
