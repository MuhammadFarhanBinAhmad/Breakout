using UnityEngine;

public class PaddleReflection : MonoBehaviour
{
    public float _maxBounceAngle;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Ball _ball = other.gameObject.GetComponent<Ball>();
        if(_ball != null )
        {
            Vector3 _paddlePos = this.transform.position;
            Vector2 _contactPoint = other.GetContact(0).point;

            float offset = _paddlePos.x - _contactPoint.x;
            float width = other.otherCollider.bounds.size.x/2;

            //calculate current angle to bounce
            float _currentAngle = Vector2.SignedAngle(Vector2.up, _ball._rigidbody.linearVelocity);
            float _bounceAngle = (offset / width) * _maxBounceAngle;

            float _newAngle = Mathf.Clamp(_currentAngle + _bounceAngle,-_maxBounceAngle, _maxBounceAngle);

            Quaternion _rotation = Quaternion.AngleAxis(_newAngle,Vector3.forward);

            _ball._rigidbody.linearVelocity = _rotation * Vector2.up * _ball._rigidbody.linearVelocity.magnitude;
        }
    }
}
