using UnityEngine;

public class CleanShot : ABSAddOnAbility
{

    
    public override void OnModifyHit(HitContext ctx) 
    {
        if(ctx._isCrit)
        {
            ctx._finaleDamage *= 2;
            print("Finale Damage: " + ctx._finaleDamage);
        }
        else
        {
            print("no effect");
        }


    }

}
