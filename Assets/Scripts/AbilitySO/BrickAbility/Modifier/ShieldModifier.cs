using UnityEngine;

public class ShieldModifier : BrickModifierBase
{

    public int shieldHit;

    public override int ModifyIncomingDamage(int incomingDamage)
    {
        shieldHit--;
        print("shieldLeft: " + shieldHit);
        if (shieldHit < 0)
        {
            _brick.RemoveModifier(this);
            return 0;
        }
        return 0;
    }
}
