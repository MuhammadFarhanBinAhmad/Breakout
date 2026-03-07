using UnityEngine;

public interface IBrickModifier
{
    // called once when this modifier is applied to a brick
    void Initialize(BrickBar brick);

    // called every frame
    void Tick(float dt);

    // modify incoming damage. Return the damage after modification
    int ModifyIncomingDamage(int incomingDamage);

    // called when damage is actually applied (after ModifyIncomingDamage)
    void OnDamageApplied(int appliedDamage);

    // called on death / removal - cleanup here
    void OnRemove();
}
