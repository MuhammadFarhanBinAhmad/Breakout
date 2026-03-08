using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Playables;
using UnityEngine;



public class AbilityManager : MonoBehaviour
{
    // now store abilities directly as ABSAbility instances
    public List<ABSAbility> _brickAbilities = new List<ABSAbility>();
    public List<ABSAbility> _ballAbilities = new List<ABSAbility>();

    int _currentTotalBrickAbility, _currentTotalBallAbility, _currentBrickAbilityTier;
    //int _currentMaxBrickAbility = 1;

    public List<SOAbilityEffect> test = new List<SOAbilityEffect>();

    private void Start()
    {
        foreach (var abilitySo in test)
        {
            AddAbility(abilitySo);
        }
    }

    /// <summary>
    /// Add ability from SO. Returns the created ABSAbility instance, or null on failure.
    /// </summary>
    public ABSAbility AddAbility(SOAbilityEffect so)
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

        // store ability directly
        _brickAbilities.Add(ability);

        return ability;
    }

    /// <summary>
    /// Remove ability instance. Returns the removed ABSAbility (so caller can inspect), or null if not found.
    /// </summary>
    public ABSAbility RemoveAbility(ABSAbility ability)
    {
        if (ability == null)
            return null;

        if (_brickAbilities.Remove(ability))
        {
            Destroy(ability.gameObject);
            return ability;
        }

        return null;
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

        // Phase 1: Modifier
        foreach (var ability in _brickAbilities)
            ability.ModifyHit(ctx);

        // Phase 2: On hit
        foreach (var ability in _brickAbilities)
            ability.OnHit(ctx);

        // Phase 3: Apply damage
        brick.OnDamage(ctx._finaleDamage);

        // Phase 4: Notify abilities of outcome
        foreach (var ability in _brickAbilities)
            ability.OnHitResolved(ctx);
    }

    public void NotifyBrickDestroyed(BrickBar brick)
    {
        foreach (var ability in _brickAbilities)
        {
            ability.OnBrickDestroy(brick);
        }
    }

    public void NotifyBallDestroyed(Ball ball)
    {
        foreach (var ability in _brickAbilities)
        {
            ability.OnBallDestroy(ball);
        }
    }

    private float _tickTimer;

    private void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer >= 1f)
        {
            _tickTimer -= 1f;

            foreach (var ability in _brickAbilities)
            {
                ability.OnTick(1f);
            }
        }
    }


    //----------------ABILITYRELATED----------------//
    public void IncreaseCurrentBrickAbilityAcquired() => _currentTotalBrickAbility++;
    //public void IncreaseCurrentMaxBrickAbility() => _currentMaxBrickAbility++;
    public void IncreaseCurrentBrickAbilityTierLevel() => _currentBrickAbilityTier++;

    //public int GetCurrentMaxBrickAbility() => _currentMaxBrickAbility;
    public int GetCurrentBrickAbilityAcquired() => _currentTotalBrickAbility;

   
}
