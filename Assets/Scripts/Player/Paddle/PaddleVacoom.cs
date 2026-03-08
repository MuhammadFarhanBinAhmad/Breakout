using UnityEngine;

public class PaddleVacoom : MonoBehaviour
{
    PaddleMovement _paddleMovement;

    [Header("Vacuum Settings")]
    public float attractRadius = 3f;
    [Tooltip("Cone angle in degrees (total). 60 means ±30° from forward.")]
    public float coneAngle = 60f;
    public float _pushPullStrength = 10f;
    [Tooltip("Max force per FixedUpdate applied to balls by the vacuum.")]
    public float ballAttractForceCap = 5f;       // cap specifically for Ball
    public LayerMask collectibleLayer;           // set to the layer used by essences/balls
    public int pullmouseButton = 0;              // 0 = left mouse

    bool _isPaddleDisable;

    private void Start()
    {
        _paddleMovement = GetComponentInParent<PaddleMovement>();
    }

    void Update()
    {
        if (_isPaddleDisable)
            return;

        bool attracting = Input.GetMouseButton(pullmouseButton);
        _paddleMovement.SetCursorState(attracting);

        // Get all colliders in radius on the collectible layer
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attractRadius, collectibleLayer);

        if (hits == null || hits.Length == 0) return;

        // compute forward direction toward mouse in worldspace (2D)
        Vector2 forward = GetMouseForward();

        // Precompute dot threshold for angle test (faster than Angle())
        float halfAngleRad = (coneAngle * 0.5f) * Mathf.Deg2Rad;
        float cosThreshold = Mathf.Cos(halfAngleRad);

        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;

            // Vector from paddle to candidate
            Vector2 toTarget = (Vector2)hits[i].transform.position - (Vector2)transform.position;
            float dist = toTarget.magnitude;

            if (dist <= Mathf.Epsilon) continue; // ignore exact overlap

            Vector2 dir = toTarget / dist; // normalized direction to target

            // Angle test: dot(forward, dir) >= cos(halfAngle)
            if (Vector2.Dot(forward, dir) < cosThreshold)
                continue; // outside cone

            // within cone AND within radius -> handle pickup
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

            // Ball
            var ball = hits[i].GetComponent<Ball>();
            if (ball != null)
            {
                if (attracting)
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

    /// <summary>
    /// Returns a normalized forward direction vector pointing from this object to the mouse world position.
    /// Falls back to transform.up if mouse world position is essentially the same as this object's position.
    /// </summary>
    Vector2 GetMouseForward()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            // fallback to transform.up if no camera found
            return transform.up;
        }

        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld3 = cam.ScreenToWorldPoint(mouseScreen);
        Vector2 mouseWorld = new Vector2(mouseWorld3.x, mouseWorld3.y);

        Vector2 toMouse = mouseWorld - (Vector2)transform.position;
        if (toMouse.sqrMagnitude < 0.0001f)
        {
            // mouse is on top of object — use transform.up as reasonable default
            return transform.up.normalized;
        }

        return toMouse.normalized;
    }

    public void PaddleDisable(bool disable) => _isPaddleDisable = disable;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractRadius); // optional radial guide

        // draw cone oriented toward mouse (or transform.up fallback)
        Vector3 origin = transform.position;
        Vector2 forward2 = GetGizmoForward();
        Vector3 forward3 = new Vector3(forward2.x, forward2.y, 0f);

        float halfAngle = coneAngle * 0.5f;

        // edge directions
        Vector3 leftDir = Quaternion.Euler(0f, 0f, -halfAngle) * forward3;
        Vector3 rightDir = Quaternion.Euler(0f, 0f, halfAngle) * forward3;

        Gizmos.color = new Color(1f, 1f, 1f, 0.6f);
        Gizmos.DrawLine(origin, origin + leftDir * attractRadius);
        Gizmos.DrawLine(origin, origin + rightDir * attractRadius);

        // draw arc
        int segments = 24;
        Vector3 prevPoint = origin + (Quaternion.Euler(0f, 0f, -halfAngle) * forward3) * attractRadius;
        for (int i = 1; i <= segments; i++)
        {
            float a = -halfAngle + (coneAngle * i / (float)segments);
            Vector3 nextPoint = origin + (Quaternion.Euler(0f, 0f, a) * forward3) * attractRadius;
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
    Vector2 GetGizmoForward()
    {
        if (Application.isPlaying)
        {
            // during play, GetMouseForward works fine
            return GetMouseForward();
        }


        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 mouseScreen = Input.mousePosition;
            Vector3 mouseWorld3 = cam.ScreenToWorldPoint(mouseScreen);
            Vector2 mouseWorld = new Vector2(mouseWorld3.x, mouseWorld3.y);
            Vector2 toMouse = mouseWorld - (Vector2)transform.position;
            if (toMouse.sqrMagnitude >= 0.0001f)
                return toMouse.normalized;
        }

        return transform.up.normalized;
    }
}
