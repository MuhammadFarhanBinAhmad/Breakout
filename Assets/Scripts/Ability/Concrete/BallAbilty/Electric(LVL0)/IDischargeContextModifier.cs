using UnityEngine;

public interface IDischargeContextModifier 
{
    void ModifyDischargeContext(HitContext hitCtx, AbilityContext dischargeCtx);

}
