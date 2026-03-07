using Mono.Cecil;
using UnityEngine;

public abstract class BrickModifierBase : MonoBehaviour, IBrickModifier
{
    protected BrickBar _brick;

    public virtual void Initialize(BrickBar brick)
    {
        _brick = brick;
        transform.SetParent(_brick.transform, false);
    }

    public virtual int ModifyIncomingDamage(int incomingDamage) => incomingDamage;

    public virtual void OnDamageApplied(int appliedDamage)
    {
    }

    public virtual void OnRemove()
    {
        Destroy(this.gameObject);
    }

    public virtual void Tick(float dt)
    {
    }
}
