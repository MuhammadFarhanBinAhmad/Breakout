using UnityEngine;

public class NukemAbility : ABSAbility, IFireContextModifier
{
    HotZonePool _hotZonePool;

    public override void OnHit(HitContext ctx)
    {
        //Instant kill brick
        ctx._brick.OnDamage(999);
    }

    public void ModifyFireContext(HitContext hitCtx, ref HotZoneArea hza)
    {
        print("Add Nukem");
        hza.SetStats(_SOAbilityEffect._statusDamage, _SOAbilityEffect._effectDuration, _SOAbilityEffect._scaleSizeMultiplier);

    }
}
