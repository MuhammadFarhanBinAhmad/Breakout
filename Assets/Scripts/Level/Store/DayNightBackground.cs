using UnityEngine;

public class DayNightBackground : MonoBehaviour
{
    [Header("References")]
    TimeManager timeManager;
    [SerializeField] Camera mainCamera;

    [Header("Day Colors")]
    [SerializeField] Color dawnColor;
    [SerializeField] Color dayColor;
    [SerializeField] Color duskColor;
    [SerializeField] Color nightColor;

    [Header("Fade")]
    [SerializeField] float fadeSpeed = 0.5f;

    Color targetColor;

    void Start()
    {
        timeManager = FindAnyObjectByType<TimeManager>();

        if (!mainCamera)
            mainCamera = Camera.main;

        mainCamera.backgroundColor = dawnColor;
        targetColor = dawnColor;
    }

    void Update()
    {
        UpdateTargetColor();

        // Smooth fade
        mainCamera.backgroundColor = Color.Lerp(
            mainCamera.backgroundColor,
            targetColor,
            fadeSpeed * Time.deltaTime
        );
    }

    void UpdateTargetColor()
    {
        float t = GetDayProgress();

        if (t < 0.2f)
            targetColor = dawnColor;
        else if (t < 0.45f)
            targetColor = dayColor;
        else if (t < 0.65f)
            targetColor = dayColor; // noon (hold)
        else if (t < 0.85f)
            targetColor = duskColor;
        else
            targetColor = nightColor;
    }

    float GetDayProgress()
    {
        return timeManager.GetDayNormalized();
    }
}
