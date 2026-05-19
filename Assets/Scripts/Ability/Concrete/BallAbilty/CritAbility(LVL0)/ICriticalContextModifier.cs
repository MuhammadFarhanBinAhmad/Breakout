using UnityEngine;

public interface ICriticalContextModifier
{
    void ModifyCriticalContext(HitContext hitCtx, AbilityContext critContext);

}
