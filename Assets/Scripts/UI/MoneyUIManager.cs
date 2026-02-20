using TMPro;
using UnityEngine;

public class MoneyUIManager : MonoBehaviour
{
    MoneyManager _moneyManager;

    [Header("MoneyUI")]
    [SerializeField] TextMeshProUGUI _currentMoneyText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _moneyManager = FindAnyObjectByType<MoneyManager>();
        UpdateMoneyUI();
    }

    public void UpdateMoneyUI()
    {
        _currentMoneyText.text = "Money: " + _moneyManager._currentMoney.ToString();
    }
}
