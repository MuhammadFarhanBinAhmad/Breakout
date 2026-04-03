using UnityEngine;



public abstract class BaseOverLayInteraction : MonoBehaviour
{
    
    public virtual void OpenOverlay(GameObject _overlay)
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_openOverlay, transform.position);
        _overlay.SetActive(true);
    }
    public virtual void CloseOverlay(GameObject _overlay)
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_closeOverlay, transform.position);
        _overlay.SetActive(false);
    }
}
