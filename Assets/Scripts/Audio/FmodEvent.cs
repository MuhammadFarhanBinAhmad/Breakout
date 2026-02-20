using UnityEngine;
using FMODUnity;

public class FmodEvent : MonoBehaviour
{
    [field: Header("Brick")]
    [field: SerializeField] public EventReference sfx_brickDestroy { get; private set; }
    [field: SerializeField] public EventReference sfx_brickHit { get; private set; }
    [field: Header("Essence")]
    [field: SerializeField] public EventReference sfx_essenceCollect { get; private set; }

    [field: Header("Paddle")]
    [field: SerializeField] public EventReference sfx_onPaddleHit { get; private set; }
    public static FmodEvent Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            print("more than one Fmod Event instance in the scene");

        Instance = this;
    }
}
