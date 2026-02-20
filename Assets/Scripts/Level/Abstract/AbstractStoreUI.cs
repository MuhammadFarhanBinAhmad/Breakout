using UnityEngine;

public abstract class AbstractStoreUI : MonoBehaviour
{
    protected GameObject storeUI;

    public virtual void ToggleUICanvas()
    {
        storeUI.SetActive(!storeUI.activeSelf);
    }

    public void OnButtonClick()
    {
        print("play audio");
    }
}
