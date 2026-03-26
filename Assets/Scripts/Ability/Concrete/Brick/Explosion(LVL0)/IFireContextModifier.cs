using UnityEngine;

public interface IFireContextModifier
{
    void ModifyFireContext(HitContext hitCtx, ref HotZoneArea explosionCtx);
}
