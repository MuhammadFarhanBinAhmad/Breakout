using UnityEngine;

[CreateAssetMenu(fileName = "SO_BrickHealthStats", menuName = "Brick/Brick Health Stats")]
public class SO_BrickHealthStats : ScriptableObject
{
    public BRICKLAYER e_Parent, e_Child;
    public int _health;
    public int _dropSpeed;
    public int _APValue;
    public int _daytoUnlock;
    public int _layerNumber;
    public Color _color;
}
