using UnityEngine;
using TMPro;


[CreateAssetMenu(fileName = "SOBrickFormation", menuName = "Brick/SOBrickFormation")]
public class SOBrickFormation : ScriptableObject
{
    [TextArea]
    public string formation;
}
