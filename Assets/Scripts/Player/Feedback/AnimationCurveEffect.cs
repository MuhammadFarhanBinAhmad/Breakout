using System.Collections;
using UnityEngine;

public class AnimationCurveEffect : MonoBehaviour
{

    public void PlayEffect(SOLerpAnimationEffect _effect,GameObject _go) => StartCoroutine(AnimateEffect(_effect,_go));

    IEnumerator AnimateEffect(SOLerpAnimationEffect _effect,GameObject _go)
    {
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = _effect._startingScale * _effect._capscaleMultiplier;

        float time = 0f;

        while (time < _effect.animationDuration)
        {
            float normalized = time / _effect.animationDuration;
            float curveValue = _effect.easeOutElastic.Evaluate(normalized);

            transform.localScale =
                Vector3.LerpUnclamped(startScale, targetScale, curveValue);

            time += Time.deltaTime;
            yield return null;
        }

        _go.transform.localScale = _effect._startingScale;
    }
}
