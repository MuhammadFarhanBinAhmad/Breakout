using UnityEngine;

public class DeadZone : MonoBehaviour
{
    public GameObject _deathVFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Ball>() != null)
        {
            Ball ball = other.GetComponent<Ball>();
            _deathVFX.SetActive(true);
            if(!ball._copyBall)
            {
                ball.OnBallReset?.Invoke();
            }
            else
            {
                ball.OnBallDestroy?.Invoke();
            }

        }
        if(other.GetComponent<PaddleHealth>() != null)
        {
            PaddleHealth ph = other.GetComponent<PaddleHealth>();
            ph.OnPaddleDisable?.Invoke();
        }
        if( other.GetComponent<TowerEssence>() != null)
        {
            TowerEssence te = other.GetComponent<TowerEssence>();
            AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_essenceDestroyed, transform.position);
            te.gameObject.SetActive(false);
        }
    }
}
