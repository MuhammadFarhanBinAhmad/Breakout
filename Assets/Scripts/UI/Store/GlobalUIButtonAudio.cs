using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class GlobalUIButtonAudio : MonoBehaviour
{
    [SerializeField] private List<GraphicRaycaster> _raycasters = new List<GraphicRaycaster>();
    [SerializeField] private EventSystem _eventSystem;

    private Button _currentHoveredButton;

    private void Awake()
    {
        _raycasters.AddRange(FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None));

        if (_eventSystem == null)
            _eventSystem = EventSystem.current;
    }

    private void Update()
    {
        if (_raycasters == null || _eventSystem == null)
            return;

        Button hoveredButton = GetHoveredButton();

        if (hoveredButton != _currentHoveredButton)
        {
            _currentHoveredButton = hoveredButton;

            if (_currentHoveredButton != null)
            {
                AudioManager.Instance.PlayOneShot(
                    FmodEvent.Instance.sfx_onButtonHover,
                    transform.position
                );
            }
        }

        if (Input.GetMouseButtonDown(0) && hoveredButton != null)
        {
            AudioManager.Instance.PlayOneShot(
                FmodEvent.Instance.sfx_onButtonPress,
                transform.position
            );
        }
    }

    Button GetHoveredButton()
    {
        PointerEventData pointerData = new PointerEventData(_eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();

        // Raycast ALL canvases
        for (int i = 0; i < _raycasters.Count; i++)
        {
            if (_raycasters[i] == null) continue;
            _raycasters[i].Raycast(pointerData, results);
        }

        // Sort so top-most UI is first (important!)
        results.Sort((a, b) => b.depth.CompareTo(a.depth));

        foreach (RaycastResult result in results)
        {
            Button btn = result.gameObject.GetComponentInParent<Button>();
            if (btn != null && btn.interactable)
                return btn;
        }

        return null;
    }
}
