using UnityEngine;

public class CheatCodeManager : MonoBehaviour
{
    TowerManager _towerManager;

    public void Start()
    {
        _towerManager = FindAnyObjectByType<TowerManager>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            AddBrick();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            AddEssence();

        if (Input.GetKeyDown(KeyCode.Alpha3))
            AddPureEssence();
    }

    public void AddBrick() => _towerManager.IncreaseBrickCount();
    public void AddEssence() => _towerManager.IncreaseEssenceCount(5);
    public void AddPureEssence() => _towerManager._currentPureEssence++;

}
