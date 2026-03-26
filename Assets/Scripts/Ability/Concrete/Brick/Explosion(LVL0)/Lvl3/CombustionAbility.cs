using System.Runtime.InteropServices;
using UnityEngine;

public class CombustionAbility : ABSAbility, IFireContextModifier
{
    public void ModifyFireContext(HitContext hitCtx, ref HotZoneArea explosionCtx)
    {
        print("Add combustion");
        explosionCtx.SetCombustion(true);
    }
}
