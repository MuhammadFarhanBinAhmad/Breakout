using UnityEngine;

public class PaddleEyeFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // the ball

    [Header("Behaviour")]
    public Vector2 maxOffset = new Vector2(0.12f, 0.08f); // how far the pupil can move locally (x,y)
    public Vector2 minRange = new Vector2(0.0f, 0.0f);    // distances inside this -> no offset (close)
    public Vector2 maxRange = new Vector2(1.0f, 1.0f);    // distances >= this -> full offset (far)
    [Range(0f, 30f)] public float smoothSpeed = 12f;      // how quickly the pupil moves

    [Header("Options")]
    public bool useInitialLocalAsOrigin = true;
    public Vector3 originLocalOverride = Vector3.zero;

    Vector3 _originLocal;
    Vector3 _currentLocal;

    void Start()
    {
        if (useInitialLocalAsOrigin)
            _originLocal = transform.localPosition;
        else
            _originLocal = originLocalOverride;

        _currentLocal = _originLocal;

        if (target == null)
        {
            var ball = GameObject.FindWithTag("Ball");
            if (ball != null) target = ball.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Convert target position into this transform's parent's local space
        // so we measure relative distance from the eye's origin.
        // If pupil is the child of the eye, we want the eye-parent as reference.
        Transform parentTransform = transform.parent != null ? transform.parent : transform;
        Vector3 targetLocalToParent = parentTransform.InverseTransformPoint(target.position);

        // If origin is in parent's local space, we compute delta from origin
        Vector3 originLocalVec3 = _originLocal;
        Vector3 delta = targetLocalToParent - originLocalVec3;

        // For each axis compute a normalized t (0..1) where:
        // - t == 0 when abs(delta) <= minRange  => pupil stays at origin on that axis
        // - t == 1 when abs(delta) >= maxRange  => pupil at full offset on that axis
        // - in between -> interpolated
        float tx = ComputeAxisT(delta.x, minRange.x, maxRange.x);
        float ty = ComputeAxisT(delta.y, minRange.y, maxRange.y);

        // Desired offset direction follows sign of delta (left/right, up/down)
        float offsetX = Mathf.Sign(delta.x) * tx * Mathf.Abs(maxOffset.x);
        float offsetY = Mathf.Sign(delta.y) * ty * Mathf.Abs(maxOffset.y);

        Vector3 targetLocal = originLocalVec3 + new Vector3(offsetX, offsetY, 0f);

        // Smoothly move pupil toward targetLocal in parent-local space
        _currentLocal = Vector3.Lerp(_currentLocal, targetLocal, 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime));

        // Apply (if this object is child of parent, set localPosition; if not, convert back)
        if (transform.parent != null)
            transform.localPosition = _currentLocal;
        else
            transform.position = parentTransform.TransformPoint(_currentLocal);
    }

    // Returns t in [0..1] for the absolute axis distance
    float ComputeAxisT(float axisDelta, float minR, float maxR)
    {
        float d = Mathf.Abs(axisDelta);

        // Edge cases: if maxR <= minR, treat as binary (0 if d <= minR else 1)
        if (maxR <= minR)
        {
            return d > minR ? 1f : 0f;
        }

        // Map d from [minR..maxR] to [0..1], clamp
        float t = Mathf.InverseLerp(minR, maxR, d);
        return Mathf.Clamp01(t);
    }

}
