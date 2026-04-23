using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.Playables;
using UnityEngine;



public class AbilityManager : MonoBehaviour
{
    // now store abilities directly as ABSAbility instances
    public List<ABSAbility> _brickAbilities = new List<ABSAbility>();
    public List<ABSAbility> _ballAbilities = new List<ABSAbility>();

    Dictionary<string, ABSAbility> _abilitiesByID = new Dictionary<string, ABSAbility>();

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

        string id = so._abilityName;

        if (!_abilitiesByID.ContainsKey(id))
        {
            _abilitiesByID.Add(id, ability);
        }
        else
        {
            Debug.LogWarning($"Ability {id} already exists.");
        }
        UnlockAbility(id);

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
            string id = ability._SOAbilityEffect._abilityName;

            if (_abilitiesByID.ContainsKey(id))
                _abilitiesByID.Remove(id);

            Destroy(ability.gameObject);
            return ability;
        }

        return null;
    }

    public bool HasAbility(string id)
    {
        return _abilitiesByID.ContainsKey(id);
    }

    public T GetAbility<T>(string id) where T : ABSAbility
    {
        if (_abilitiesByID.TryGetValue(id, out ABSAbility ability))
            return ability as T;

        return null;
    }

    public bool IsAbilityUnlocked(string id)
    {
        if (_abilitiesByID.TryGetValue(id, out ABSAbility ability))
            return ability.IsUnlocked;

        return false;
    }
    public void UnlockAbility(string id)
    {
        if (_abilitiesByID.TryGetValue(id, out ABSAbility ability))
        {
            ability.SetUnlocked(true);
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
            _isCrit = false,
        };

        // Phase 1: Modifier
        foreach (var ability in _brickAbilities)
            ability.ModifyHit(ctx);

        // Phase 2: On hit
        foreach (var ability in _brickAbilities)
            ability.OnHit(ctx);

        // Phase 3: Apply damage
        brick.OnDamage(ctx._baseDamage);

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

    public ABSAbility GetAbility(string id)
    {
        if (_abilitiesByID.TryGetValue(id, out ABSAbility ability))
            return ability;

        return null;
    }

    public void ApplyExplosionModifiers(HitContext hitCtx, ref ExplosionContext explosionCtx)
    {
        foreach (var ability in _brickAbilities)
        {
            if (ability is IExplosionContextModifier modifier)
            {
                modifier.ModifyExplosionContext(hitCtx, ref explosionCtx);
            }
        }
    }
    public void ApplyFireModifiers(HitContext hitCtx, ref HotZoneArea hza)
    {
        foreach (var ability in _brickAbilities)
        {
            if (ability is IFireContextModifier modifier)
            {
                modifier.ModifyFireContext(hitCtx, ref hza);
            }
        }
    }
    public bool RemoveAbility(string id)
    {
        if (_abilitiesByID.TryGetValue(id, out ABSAbility ability))
        {
            return RemoveAbility(ability) != null;
        }

        return false;
    }
}
