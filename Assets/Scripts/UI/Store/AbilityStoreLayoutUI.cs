using System.Collections.Generic;
using UnityEngine;

public class AbilityStoreLayoutUI : AbstractStoreUI
{

    StoreAbilityManager _storeAbilityManager;

    [Header("UI")]
    [SerializeField] Transform _contentParent;
    [SerializeField] BallAbilityButtonUI _abilityButtonPrefab;

    [Header("Data")]
    [SerializeField] List<SOStoreAbilityContent> abilityList = new List<SOStoreAbilityContent>();

    // Tiered containers (0–3)
    List<SOStoreAbilityContent>[] abilityLevels = new List<SOStoreAbilityContent>[4];
    List<BallAbilityButtonUI>[] spawnedButtonsLevels = new List<BallAbilityButtonUI>[4];

    [Header("Radial Layout")]
    [SerializeField] float minRadius;
    [SerializeField] float maxRadius; [SerializeField] float startAngle;

    private void Start()
    {
        _storeAbilityManager = FindAnyObjectByType<StoreAbilityManager>();
        
        InitLevels();
        BuildStore();
    }

    void InitLevels()
    {
        for (int i = 0; i < 4; i++)
        {
            abilityLevels[i] = new List<SOStoreAbilityContent>();
            spawnedButtonsLevels[i] = new List<BallAbilityButtonUI>();
        }
    }
    public void BuildStore()
    {


        // Get abilities
        abilityList = _storeAbilityManager.GetAbilityList();

        // Group abilities by level
        foreach (SOStoreAbilityContent ability in abilityList)
        {
            int level = Mathf.Clamp(ability.ability_Level, 0, 3);
            abilityLevels[level].Add(ability);
        }

        // Spawn buttons per level
        for (int level = 0; level < 4; level++)
        {
            foreach (SOStoreAbilityContent ability in abilityLevels[level])
            {
                BallAbilityButtonUI button = Instantiate(_abilityButtonPrefab, _contentParent);
                button.Setup(ability, _storeAbilityManager);

                spawnedButtonsLevels[level].Add(button);
            }
        }

        // Arrange into concentric circles
        ArrangeRadial();
    }

    void ArrangeRadial()
    {
        for (int level = 0; level < 4; level++)
        {
            List<BallAbilityButtonUI> buttons = spawnedButtonsLevels[level];

            int count = buttons.Count;
            if (count == 0) continue;

            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                RectTransform rect = buttons[i].GetComponent<RectTransform>();

                float angle = startAngle + (angleStep * i);
                float rad = angle * Mathf.Deg2Rad;

                float r = GetRadius(level, 4);

                float x = Mathf.Cos(rad) * r;
                float y = Mathf.Sin(rad) * r;

                rect.anchoredPosition = new Vector2(x, y);

                // Optional: keep UI upright
                rect.localRotation = Quaternion.identity;
            }
        }
    }

    public override void ToggleUICanvas()
    {
        base.ToggleUICanvas();
        RefreshAll();
    }
    public void RefreshAll()
    {
        for (int level = 0; level < 4; level++)
        {
            foreach (BallAbilityButtonUI button in spawnedButtonsLevels[level])
            {
                button.Refresh();
            }
        }
    }
    float GetRadius(int level, int totalLevels)
    {
        if (totalLevels <= 1) return minRadius;

        float t = (float)level / (totalLevels - 1);
        return Mathf.Lerp(minRadius, maxRadius, t);
    }
}
