using UnityEngine;

public class PaddleVacoom : MonoBehaviour
{
    [Header("Vacuum Settings")]
    public float attractRadius;
    public float _pushPullStrength;
    [Tooltip("Max force per FixedUpdate applied to balls by the vacuum.")]
    public float ballAttractForceCap;       // cap specifically for Ball
    public LayerMask collectibleLayer;           // set to the layer used by essences/balls
    public int pullmouseButton = 0;                  // 0 = left mouse
    [Header("Push Settings")]
    [Tooltip("Button used to push (default: right mouse).")]
    public int pushMouseButton = 1;              // 1 = right mouse
    [Tooltip("How long the ball resists re-attraction after being pushed.")]
    public float pushLockDuration = 0.35f;
    [Tooltip("Optional minimum distance multiplier so very close balls get a guaranteed strong push.")]
    [Range(0f, 1f)]
    public float pushMinFalloff = 0.15f;

    bool _isPaddleDisable;
    void Update()
    {
        if (_isPaddleDisable)
            return;

        bool attracting = Input.GetMouseButton(pullmouseButton);
        bool pushing = Input.GetMouseButton(pushMouseButton);

        // get all colliders in radius on the collectible layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attractRadius, collectibleLayer);

        if (hits == null || hits.Length == 0) return;

        for (int i = 0; i < hits.Length; i++)
        {
            // TowerEssence (existing)
            var ess = hits[i].GetComponent<TowerEssence>();
            if (ess != null)
            {
                if (attracting)
                {
                    ess.StartAttraction(transform, _pushPullStrength, attractRadius);
                    ess.UpdateAttractionTarget(transform.position);
                }
                else
                {
                    ess.StopAttraction();
                }

                continue; // skip ball handling if this collider is an essence
            }

            // Ball (new)
            var ball = hits[i].GetComponent<Ball>();
            if (ball != null)
            {
                if (pushing)
                {
                    ball.StartAttraction(transform, -_pushPullStrength, attractRadius, ballAttractForceCap);
                    ball.UpdateAttractionTarget(transform.position);
                }
                else if(attracting)
                {
                    // pass the cap so Ball limits the pull applied to itself
                    ball.StartAttraction(transform, _pushPullStrength, attractRadius, ballAttractForceCap);
                    ball.UpdateAttractionTarget(transform.position);
                }
                else
                {
                    ball.StopAttraction();
                }
            }
        }
    }
    public void PaddleDisable(bool disable) => _isPaddleDisable = disable;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
