
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public abstract class ABSAbility : MonoBehaviour
{
    protected Ball _ball;
    protected AbilityManager _abilityManager;

    public SOAbilityEffect _SOAbilityEffect;

    public bool IsUnlocked { get; private set; }

    public void SetUnlocked(bool value) => IsUnlocked = value;
    private void OnEnable()
    {
        _ball = FindAnyObjectByType<Ball>();
        _abilityManager = FindAnyObjectByType<AbilityManager>();
    }
   
    // Called once when added
    public virtual void OnAdded(AbilityManager manager) { }

    // Brick-related hooks
    // Modify phase: abilities & their addons can modify the context.All damage calculation to be done here
    public virtual void ModifyHit(HitContext ctx) { }

    // Resolve/execute phase: the concrete ability may roll/apply damage,
    // and addons can react via OnHitResolved (called after resolve).
    public virtual void OnHit(HitContext ctx) { }
    // After a hit on enemy has been resolved
    public virtual void OnHitResolved(HitContext ctx) { }
    // When a brick is destroyed upon hit
    public virtual void OnBrickDestroy(BrickBar bar) { }
    // When the ball is destroyed
    public virtual void OnBallDestroy(Ball ball) { }

    // Time-based hook
    public virtual void OnTick(float deltaTime) { }

    public virtual void ActivateAbility() { }
}
