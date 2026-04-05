using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class TowerUIManager : MonoBehaviour
{
    TowerManager _towerManager;

    [Header("TowerUI")]
    [SerializeField] TextMeshProUGUI _currentTowerHeightText;
    [SerializeField] Image _brickFillImage, _floorFillImage;


    [Header("EssenceUI")]
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
        _towerManager.OnEssenceCollect += UpdateTowerUI;
        _towerManager.OnEssenceCollect += UpdateEssenceUI;
        _towerManager._OnGameOver += GameOverScreen;
        UpdateTowerUI();
        UpdateEssenceUI();
    }
    private void OnDisable()
    {
        _towerManager.OnEssenceCollect -= UpdateTowerUI;
        _towerManager.OnEssenceCollect -= UpdateEssenceUI;
        _towerManager._OnGameOver -= GameOverScreen;
    }
    public void GameOverScreen() => _gameOverScreen.SetActive(true);
    public void UpdateTowerUI()
    {
        _floorFillImage.fillAmount = (float)_towerManager.GetCurrentBrickCount() / (float)_towerManager.GetBrickFloorConversionRate();
        _currentTowerHeightText.text = "Height: " + _towerManager._currentTowerHeight.ToString() + " M";

    }
    public void UpdateEssenceUI()
    {
        _brickFillImage.fillAmount = (float)_towerManager.GetCurrentEssence() / (float)_towerManager.GetEssencePureEssenceConversionRate();
        _currentPureEssenceText.text = "PE: " + _towerManager._currentPureEssence.ToString();
    }
}
