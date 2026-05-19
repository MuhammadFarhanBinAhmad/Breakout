using UnityEngine;

public class PaddleBallShooter : MonoBehaviour
{
    [SerializeField] private Ball _ball;
    [SerializeField] private Transform _launchPoint; // optional, can be the paddle child
    [SerializeField] private Camera _gameCamera;

    private void Start()
    {
        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_ball != null && _launchPoint != null)
            _ball.PrepareForLaunch(_launchPoint.position);
    }

    private void Update()
    {
        if (_ball == null || !_ball.IsAwaitingLaunch)
            return;

        // Keep the ball stuck to the launch point while waiting
        if (_launchPoint != null)
            _ball.transform.position = _launchPoint.position;

        if (Input.GetMouseButtonDown(0))
        {
            LaunchBallTowardMouse();
        }
    }

    void LaunchBallTowardMouse()
    {
        if (_gameCamera == null)
            _gameCamera = Camera.main;

        if (_gameCamera == null || _ball == null)
            return;

        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = -_gameCamera.transform.position.z;

        Vector2 mouseWorld = _gameCamera.ScreenToWorldPoint(mouseScreen);
        Vector2 origin = _launchPoint != null ? (Vector2)_launchPoint.position : (Vector2)_ball.transform.position;

        Vector2 direction = mouseWorld - origin;
        _ball.Launch(direction);
    }
}
