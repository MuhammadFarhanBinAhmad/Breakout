using TMPro.EditorUtilities;
using UnityEngine;

public class StoreAbilityContent : MonoBehaviour
{

    StoreAbilityManager _storeAbilityManager;

    public string ability_Name;
    public string ability_Description;
    public string ability_Level;
    public GameObject ability_ToSpawn;

    private void Start()
    {
        _storeAbilityManager = FindAnyObjectByType<StoreAbilityManager>();
    }




}

