using UnityEngine;

public abstract class ABSAddOnAbility : MonoBehaviour
{
    protected ABSAbility owner;
    public SOAbilityEffect effect;


    // Called once when the parent ability is attached
    public virtual void OnAttach(ABSAbility ownerAbility)
    {
        owner = ownerAbility;
    }

    // Called during the Modify phase to change stats (crit chance/multiplier, damage, etc.)
    public virtual void OnModifyHit(HitContext ctx) { }
    public virtual void OnHit(HitContext ctx) { }
    // Called during Resolve/OnHit phase to react after the hit has been resolved
    public virtual void OnHitResolved(HitContext ctx) { }

    // Optional spawn hook
    public virtual void OnSpawnObject() { }
}
