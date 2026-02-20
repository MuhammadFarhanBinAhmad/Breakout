using UnityEngine;

public class DeadZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Ball>() != null)
        {
            Ball ball = other.GetComponent<Ball>();
            ball.OnBallReset?.Invoke();
        }
        if(other.GetComponent<PaddleHealth>() != null)
        {
            PaddleHealth ph = other.GetComponent<PaddleHealth>();
            ph.OnPaddleDisable?.Invoke();
        }
        if( other.GetComponent<TowerEssence>() != null)
        {
            TowerEssence te = other.GetComponent<TowerEssence>();
            te.gameObject.SetActive(false);
        }
    }
}
