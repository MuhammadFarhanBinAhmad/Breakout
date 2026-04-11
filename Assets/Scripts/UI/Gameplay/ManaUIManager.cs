using UnityEngine;
using UnityEngine.UI;

public class ManaUIManager : MonoBehaviour
{
    Ball _ball;
    
    public Image _manaImage;


    private void Start()
    {
        _ball = FindAnyObjectByType<Ball>();
    }

    private void Update()
    {
        UpdateManaBarUI();
    }
    void UpdateManaBarUI()
    {
        _manaImage.fillAmount = _ball.GetCurrentManaAmount() / _ball.GetMaxManaAmount();
    }

}
