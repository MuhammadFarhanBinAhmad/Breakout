using UnityEditor;
using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    PolygonCollider2D _polygonCollider;

    public float _speed;
    public float _maxXPos;
    bool _isPaddleDisable;
    bool _isPaddleSucking;
    private void Start()
    {
        _polygonCollider = GetComponent<PolygonCollider2D>();
        SetCursorState(false);
    }

    void Update()
    {
        if (_isPaddleDisable || _isPaddleSucking)
            return;


        float mouseX = Input.GetAxis("Mouse X");

        if ((mouseX > 0 && transform.position.x < _maxXPos) ||
            (mouseX < 0 && transform.position.x > -_maxXPos))
        {
            transform.position += Vector3.right * mouseX * _speed * Time.deltaTime;
        }
    }

    public void PaddleDisable(bool disable)
    {
        _isPaddleDisable = disable;
        _polygonCollider.enabled = !disable;
    }
    public void SetCursorState(bool state)
    {
        _isPaddleSucking = state;
        Cursor.visible = state;

        if (Cursor.visible) 
        Cursor.lockState = CursorLockMode.None;
        else
            Cursor.lockState = CursorLockMode.Locked;

    }
}
