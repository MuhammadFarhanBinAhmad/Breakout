using UnityEngine;

public class OneUp : ABSAbility
{
    public override void ModifyHit(HitContext ctx)
    {
        ctx._baseDamage++;
    }

}
