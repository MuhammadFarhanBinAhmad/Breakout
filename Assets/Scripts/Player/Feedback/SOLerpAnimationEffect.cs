using UnityEngine;

[CreateAssetMenu(fileName = "SOLerpAnimationEffect", menuName = "FeedbackEffect/LerpAnimation")]
public class SOLerpAnimationEffect : ScriptableObject
{
    public AnimationCurve easeOutElastic;
    public Vector3 _startingScale;
    public float animationDuration;
    public float _capscaleMultiplier;
}
