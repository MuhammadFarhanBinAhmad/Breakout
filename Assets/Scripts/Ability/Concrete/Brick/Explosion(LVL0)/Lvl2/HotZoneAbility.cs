using UnityEngine;

public class HotZoneAbility : ABSAbility
{
    protected HotZonePool _hotZonePool;

    private void Start()
    {
        _hotZonePool = FindAnyObjectByType<HotZonePool>();
    }

    public override void OnHit(HitContext ctx)
    {
        if (_hotZonePool == null) return;

        GameObject fireGO = _hotZonePool.GetObject();
        fireGO.transform.position = ctx._brick.transform.position;
        fireGO.GetComponent<HotZoneArea>().SetStats(_SOAbilityEffect._statusDamage, _SOAbilityEffect._effectDuration);

        HotZoneArea area = fireGO.GetComponent<HotZoneArea>();

        _abilityManager.ApplyFireModifiers(ctx, ref area);
    }
}
