using UnityEngine;
using TMPro;


[CreateAssetMenu(fileName = "SOBrickFormation", menuName = "Brick/Brick Formation")]
public class SOBrickFormation : ScriptableObject
{
    [TextArea]
    public string formation;
}
