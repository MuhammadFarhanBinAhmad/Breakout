using System.Collections.Generic;
using UnityEngine;

public class AbilityContext
{
    public ABSAbility _abililty;

    public GameObject _source;
    public Vector2 _position;

    public STATUSTYPE _statusType;
    public SOStatusEffect _statusEffect;

    public Dictionary<STATID, float> _Stats = new Dictionary<STATID, float>();
    public Dictionary<STATID, bool> _Statsbool = new Dictionary<STATID, bool>();
    public GameObject _spawnPrefab;
}
