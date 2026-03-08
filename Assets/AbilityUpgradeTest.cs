using UnityEngine;

public class AbilityUpgradeTest : MonoBehaviour
{

    [Header("Test Target")]
    public ABSAbility targetAbility;

    [Header("Input")]
    public KeyCode upgradeKey = KeyCode.U;
    public int statIndexToUpgrade = 0;

    void Update()
    {
        if (Input.GetKeyDown(upgradeKey))
        {
            if (targetAbility == null)
            {
                Debug.LogWarning("No ability assigned to upgrade test");
                return;
            }
        }
    }
}
