using UnityEngine;

public class BallDirectionArrow : MonoBehaviour
{
    [SerializeField] Transform _target;     // ball
    [SerializeField] float _radius = 2f;

    [SerializeField] Camera _gameCamera;
    [SerializeField] float _rotationOffset = -90f;

    void Update()
    {
        if (_target == null) return;

        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_gameCamera == null) return;

        // --- Mouse world position ---
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -_gameCamera.transform.position.z;

        Vector2 mouseWorld = _gameCamera.ScreenToWorldPoint(mouseScreen);

        // --- Direction from BALL to mouse ---
        Vector2 dirFromBall = mouseWorld - (Vector2)_target.position;

        if (dirFromBall.sqrMagnitude < 0.0001f)
            return;

        dirFromBall.Normalize();

        // --- Position: orbit relative to mouse direction ---
        transform.position = (Vector2)_target.position + dirFromBall * _radius;

        // --- Rotation: face mouse ---
        Vector2 dirToMouse = mouseWorld - (Vector2)transform.position;

        float angle = Mathf.Atan2(dirToMouse.y, dirToMouse.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + _rotationOffset);
    }
    public void SetEnableArrow(bool enable) => gameObject.SetActive(enable);

}
