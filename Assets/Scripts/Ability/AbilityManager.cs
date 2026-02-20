using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Playables;
using UnityEngine;

[System.Serializable]
public class ActiveAbility
{
    public ABSAbility ability;
    public int level;

    public ActiveAbility(ABSAbility ability, int level = 1)
    {
        this.ability = ability;
        this.level = level;
    }
}
[System.Serializable]
public class AbilityTypeThresholdCounter
{
    public int _abilityLevel;
    public int _currentAbilityTier;
}


public class AbilityManager : MonoBehaviour
{
    public List<ActiveAbility> _activeAbilities = new List<ActiveAbility>();

    int _currentTotalBrickAbility,_currentTotalBallAbility,_currentBrickAbilityTier;
    int _currentMaxBrickAbility = 1;

    public bool _brickAbilityPresent, _ballAbilityPresent, _environmentAbilityPresent;

    [Header("Ability threshold and counter")]
    [Tooltip("0 - Crit" +
        "1 - Explosive" +
        "2 - Toxic" +
        "3 - Lightning")]
    public AbilityTypeThresholdCounter[] _AbilityTypeThresholdCounter = new AbilityTypeThresholdCounter[4];
    public int[] _abilityLevelPreRequsite = new int[3];

    public List<SOAbilityEffect> test = new List<SOAbilityEffect>();

    private void Start()
    {
        foreach (var ability in test)
        {
            AddAbility(ability);
        }
    }
    

    public ActiveAbility AddAbility(SOAbilityEffect so)
    {
        if (so == null || so._abilityPrefab == null)
        {
            Debug.LogError("Ability SO or prefab missing.");
            return null;
        }

        GameObject go = Instantiate(so._abilityPrefab, transform);
        ABSAbility ability = go.GetComponent<ABSAbility>();

        if (ability == null)
        {
            Debug.LogError("Prefab does not contain ABSAbility.");
            Destroy(go);
            return null;
        }

        ability._SOAbilityEffect = so;
        ability.OnAdded(this);

        ActiveAbility activeAbility = new ActiveAbility(ability, 1);
        _activeAbilities.Add(activeAbility);

        return activeAbility;
    }
    public void RemoveAbility(ActiveAbility activeAbility)
    {
        if (_activeAbilities.Remove(activeAbility))
        {
            Destroy(activeAbility.ability.gameObject);
        }
    }
    // „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ Brick Events „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    public void NotifyBrickHit(BrickBar brick, int basedmg)
    {
        //create basic hit context
        HitContext ctx = new HitContext
        {
            _brick = brick,
            _baseDamage = basedmg,
            _finaleDamage = basedmg,
            _isCrit = false,
        };

        // Phase 1: Modifer
        foreach (var active in _activeAbilities)
            active.ability.ModifyHit(ctx);

        // Phase 2: On hit
        foreach (var active in _activeAbilities)
            active.ability.OnHit(ctx);

        // Phase 3: Apply damage
        brick.OnDamage(ctx._finaleDamage);

        // Phase 3: Notify abilities of outcome
        foreach (var active in _activeAbilities)
            active.ability.OnHitResolved(ctx);
    }

    public void NotifyBrickDestroyed(BrickBar brick)
    {
        foreach (var active in _activeAbilities)
        {
            active.ability.OnBrickDestroy(brick);
        }
    }
    public void NotifyBallDestroyed(Ball ball)
    {
        foreach (var active in _activeAbilities)
        {
            active.ability.OnBallDestroy(ball);
        }
    }

    private float _tickTimer;

    private void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= 1f)
        {
            _tickTimer -= 1f;

            foreach (var active in _activeAbilities)
            {
                active.ability.OnTick(1f);
            }
        }
    }

    //----------------ABILITYRELATED----------------//
    public void IncreaseCurrentBrickAbilityAcquired() => _currentTotalBrickAbility++;
    public void IncreaseCurrentMaxBrickAbility() => _currentMaxBrickAbility++;
    public void IncreaseCurrentBrickAbilityTierLevel() => _currentBrickAbilityTier++;

    public int GetCurrentMaxBrickAbility() => _currentMaxBrickAbility;
    public int GetCurrentBrickAbilityAcquired() => _currentTotalBrickAbility;
    public int GetCurrentBrickAbilityTierLevel() => _currentBrickAbilityTier;

    public int GetAbilityLevelIndex(BRICKABILITYTYPE bbt)
    {
        switch (bbt)
        {
            case BRICKABILITYTYPE.CRIT:
                return _AbilityTypeThresholdCounter[0]._abilityLevel;
            case BRICKABILITYTYPE.EXPLOSIVE: 
                return _AbilityTypeThresholdCounter[1]._abilityLevel;
            case BRICKABILITYTYPE.TOXIC: 
                return _AbilityTypeThresholdCounter[2]._abilityLevel;
            case BRICKABILITYTYPE.LIGHTNING: 
                return _AbilityTypeThresholdCounter[3]._abilityLevel;
            default: return 0;
        }
    }
    public int GetAbilityTierLevelIndex(BRICKABILITYTYPE bbt)
    {
        switch (bbt)
        {
            case BRICKABILITYTYPE.CRIT:
                return _AbilityTypeThresholdCounter[0]._currentAbilityTier;
            case BRICKABILITYTYPE.EXPLOSIVE:
                return _AbilityTypeThresholdCounter[1]._currentAbilityTier;
            case BRICKABILITYTYPE.TOXIC:
                return _AbilityTypeThresholdCounter[2]._currentAbilityTier;
            case BRICKABILITYTYPE.LIGHTNING:
                return _AbilityTypeThresholdCounter[3]._currentAbilityTier;
            default: return 0;
        }
    }
    public void UpgradeAbilityTypeLevel(BRICKABILITYTYPE bbt)
    {
        switch (bbt)
        {
            case BRICKABILITYTYPE.CRIT:
                {
                    _AbilityTypeThresholdCounter[0]._abilityLevel++;
                    break;
                }
            case BRICKABILITYTYPE.EXPLOSIVE:
                {
                    _AbilityTypeThresholdCounter[1]._abilityLevel++;
                    break;
                }
            case BRICKABILITYTYPE.TOXIC:
                {
                    _AbilityTypeThresholdCounter[2]._abilityLevel++;
                    break;
                }
            case BRICKABILITYTYPE.LIGHTNING:
                {
                    _AbilityTypeThresholdCounter[3]._abilityLevel++;
                    break;
                }
        }
    }
    public void UpgradeAbilityTierLevel(BRICKABILITYTYPE bbt)
    {
        switch (bbt)
        {
            case BRICKABILITYTYPE.CRIT:
                {
                    _AbilityTypeThresholdCounter[0]._currentAbilityTier++;
                    break;
                }
            case BRICKABILITYTYPE.EXPLOSIVE:
                {
                    _AbilityTypeThresholdCounter[1]._currentAbilityTier++;
                    break;
                }
            case BRICKABILITYTYPE.TOXIC:
                {
                    _AbilityTypeThresholdCounter[2]._currentAbilityTier++;
                    break;
                }
            case BRICKABILITYTYPE.LIGHTNING:
                {
                    _AbilityTypeThresholdCounter[3]._currentAbilityTier++;
                    break;
                }
        }
    }




}
