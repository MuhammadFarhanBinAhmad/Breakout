using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class AbstractStoreUI : MonoBehaviour
{
    protected GameObject storeUI;

    public virtual void ToggleUICanvas()
    {
        storeUI.SetActive(!storeUI.activeSelf);
    }
}
