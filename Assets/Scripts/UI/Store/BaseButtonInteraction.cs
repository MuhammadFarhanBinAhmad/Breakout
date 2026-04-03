using UnityEngine;
using UnityEngine.EventSystems;

public abstract class BaseButtonInteraction : MonoBehaviour ,IPointerEnterHandler , IPointerExitHandler
{
    public virtual void OnButtonClick()
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onButtonPress, transform.position);
    }
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlayOneShot(FmodEvent.Instance.sfx_onButtonHover, transform.position);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
    }
}
