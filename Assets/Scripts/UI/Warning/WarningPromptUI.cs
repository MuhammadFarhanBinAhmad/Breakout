using UnityEngine;
using UnityEngine.UI;

public class WarningPromptUI : BaseOverLayInteraction
{
    [SerializeField]GameObject _warningOverlay;
    [SerializeField] Button _warningCloseButton;

    private void Start()
    {
        _warningCloseButton.onClick.AddListener(() => CloseOverlay(_warningOverlay));
    }
}
