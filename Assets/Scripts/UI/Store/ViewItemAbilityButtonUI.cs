using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewItemAbilityButtonUI : MonoBehaviour
{


    [Header("Item Detail")]
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText, _costText,_descriptionText;
    int _purchaseCost;
    ItemAbilityButtonUI _selectedItem;


    public void SetViewItemButton(ItemAbilityButtonUI sellectedItem) 
    { 
        _selectedItem = sellectedItem;
        SOItemAbilityContentUI sOItemAbilityContentUI = sellectedItem._itemAbilityContent;
        _icon.sprite = sOItemAbilityContentUI.icon;
        _nameText.text = sOItemAbilityContentUI.ability_Name;
        _descriptionText.text = sOItemAbilityContentUI.ability_Description;
        _purchaseCost = _selectedItem._purchaseCost;
        _costText.text = _purchaseCost.ToString();
    }

    public void SetContentToNull()
    {
        _selectedItem = null;
        _icon.sprite = null;
        _nameText.text = null;
        _purchaseCost = 0;
        _costText.text = null;
    }
}
