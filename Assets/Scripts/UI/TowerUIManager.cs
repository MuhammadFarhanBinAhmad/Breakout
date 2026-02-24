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
    [Header("GameOverScreen")]
    public GameObject _gameOverScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _towerManager = FindAnyObjectByType<TowerManager>();
    }
    void Start()
    {
        _towerManager.OnHeightIncrease += UpdateTowerUI;
        _towerManager.OnEssenceCollect += UpdateEssenceUI;
        _towerManager._OnGameOver += GameOverScreen;
        UpdateTowerUI();
    }
    private void OnDisable()
    {
        _towerManager.OnHeightIncrease -= UpdateTowerUI;
        _towerManager.OnEssenceCollect -= UpdateEssenceUI;
        _towerManager._OnGameOver -= GameOverScreen;
    }
    public void GameOverScreen() => _gameOverScreen.SetActive(true);
    public void UpdateTowerUI()
    {
        _currentTowerHeightText.text = "Height: " + _towerManager._currentTowerHeight.ToString() + " M";
    }
    public void UpdateEssenceUI()
    {
        _currentEssenceText.text = "Essence: " + _towerManager._currentEssenceCount.ToString() + " / " +
                                    _towerManager._essenceThreshold.ToString();
        _currentPureEssenceText.text = "PureEssence: " + _towerManager._currentPureEssence.ToString();
    }
}
