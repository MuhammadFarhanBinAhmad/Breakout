using TMPro;
using UnityEngine;

public class TowerUIManager : MonoBehaviour
{
    TowerManager _towerManager;

    [Header("TowerUI")]
    [SerializeField] TextMeshProUGUI _currentTowerHeightText;
    [Header("EssenceUI")]
    [SerializeField] TextMeshProUGUI _currentEssenceText;
    [SerializeField] TextMeshProUGUI _currentPureEssenceText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _towerManager = FindAnyObjectByType<TowerManager>();
    }
    void Start()
    {
        _towerManager.OnHeightIncrease += UpdateTowerUI;
        _towerManager.OnEssenceCollect += UpdateEssenceUI;
        UpdateTowerUI();
    }
    private void OnDisable()
    {
        _towerManager.OnHeightIncrease -= UpdateTowerUI;
        _towerManager.OnEssenceCollect -= UpdateEssenceUI;
    }

    public void UpdateTowerUI()
    {
        _currentTowerHeightText.text = "Level: \n$" + _towerManager._currentTowerHeight.ToString();
    }
    public void UpdateEssenceUI()
    {
        _currentEssenceText.text = "Essence: " + _towerManager._currentEssenceCount.ToString();
        _currentPureEssenceText.text = "PureEssence: " + _towerManager._currentPureEssence.ToString();
    }
}
