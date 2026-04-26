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
    [Header("WarningScreen")]
    [SerializeField] GameObject _warningPopUp;
    [SerializeField] GameObject _warningStamp;
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
        _towerManager.OnAddPureEssence += UpdateTowerUI;
        _towerManager._OnGameOver += GameOverScreen;
        _towerManager.OnReceivingWarning += ShowWarningStamp;
        _towerManager.OnReceivingWarning += ShowWarningPopUp;
        UpdateTowerUI();
        UpdateEssenceUI();
    }
    private void OnDisable()
    {
        _towerManager.OnEssenceCollect -= UpdateTowerUI;
        _towerManager.OnEssenceCollect -= UpdateEssenceUI;
        _towerManager._OnGameOver -= GameOverScreen;
        _towerManager.OnAddPureEssence -= UpdateTowerUI;
        _towerManager.OnReceivingWarning -= ShowWarningStamp;
        _towerManager.OnReceivingWarning -= ShowWarningPopUp;
    }
    public void GameOverScreen() => _gameOverScreen.SetActive(true);
    public void UpdateTowerUI()
    {
        _floorFillImage.fillAmount = (float)_towerManager.GetCurrentBrickCount() / (float)_towerManager.GetBrickFloorConversionRate();
        _currentTowerHeightText.text = "Height: " + _towerManager._currentTowerHeight.ToString() + " M";

    }
    public void ShowWarningPopUp()
    {
        _warningPopUp.SetActive(true);
        TimeManager.StopTime();
    }
    public void ShowWarningStamp() => _warningStamp.SetActive(true);
    public void UpdateEssenceUI()
    {
        _brickFillImage.fillAmount = (float)_towerManager.GetCurrentEssence() / (float)_towerManager.GetEssencePureEssenceConversionRate();
        _currentPureEssenceText.text = ": " + _towerManager._currentPureEssence.ToString();
    }
}
