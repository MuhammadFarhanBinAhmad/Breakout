
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public abstract class ABSAbility : MonoBehaviour
{
    protected Ball _ball;
    [SerializeField] protected AbilityManager _abilityManager;

    public SOAbilityEffect _SOAbilityEffect;
    public SOStatusEffect _SOStatusEffect;

    public bool IsUnlocked { get; private set; }

    public void SetUnlocked(bool value)
    {
        IsUnlocked = value;
    }
    private void OnEnable()
    {
        _ball = FindAnyObjectByType<Ball>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
    }

   
   
    // Called once when added
    public virtual void OnAdded(AbilityManager manager) { }


    // Brick-related hooks
    // Modify phase: abilities & their addons can modify the context.All damage calculation to be done here
    public virtual void ModifyHit(HitContext ctx)
    {
        ctx._finaleDamage += (int)((float)(ctx._baseDamage) * _SOAbilityEffect._baseDamageMultiplier);

    }

    // Resolve/execute phase: the concrete ability may roll/apply damage,
    // and addons can react via OnHitResolved (called after resolve).
    public virtual void OnHit(HitContext ctx)
    {

    }
    public virtual void OnHitResolved(HitContext ctx)
    {
    }
    public virtual void OnBrickDestroy(BrickBar bar) { }
    public virtual void OnBallDestroy(Ball ball) {
    }

    // Time-based hook
    public virtual void OnTick(float deltaTime) { }
    
}
