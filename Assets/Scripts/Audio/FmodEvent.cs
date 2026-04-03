using UnityEngine;
using FMODUnity;

public class FmodEvent : MonoBehaviour
{
    [field: Header("Brick")]
    [field: SerializeField] public EventReference sfx_brickDestroy { get; private set; }
    [field: SerializeField] public EventReference sfx_brickHit { get; private set; }
    [field: SerializeField] public EventReference sfx_onBrickMade { get; private set; }
    [field: Header("Floor")]
    [field: SerializeField] public EventReference sfx_onFloorMade { get; private set; }


    [field: Header("Essence")]
    [field: SerializeField] public EventReference sfx_essenceCollect { get; private set; }
    [field: SerializeField] public EventReference sfx_essenceDestroyed { get; private set; }


    [field: Header("Paddle")]
    [field: SerializeField] public EventReference sfx_onPaddleHitByBrick { get; private set; }
    [field: SerializeField] public EventReference sfx_onPaddleDestroy { get; private set; }
    [field: SerializeField] public EventReference sfx_onPaddleRespawning { get; private set; }
    [field: SerializeField] public EventReference sfx_onPaddleRespawn { get; private set; }
    [field: SerializeField] public EventReference sfx_onPaddleSucking { get; private set; }

    [field: Header("Ball")]
    [field: SerializeField] public EventReference sfx_onBallDestroy { get; private set; }
    [field: SerializeField] public EventReference sfx_onBallRespawn { get; private set; }
    [field: SerializeField] public EventReference sfx_onBallHitWall { get; private set; }

    [field: Header("Explosion")]
    [field: SerializeField] public EventReference sfx_onBombExplosion { get; private set; }


    [field: Header("Time")]
    [field: SerializeField] public EventReference sfx_onNewDay { get; private set; }
    [field: SerializeField] public EventReference sfx_onNewWeek { get; private set; }
    [field: SerializeField] public EventReference sfx_onNewMonth { get; private set; }

    [field: Header("Combo")]
    [field: SerializeField] public EventReference sfx_onDRank{ get; private set; }
    [field: SerializeField] public EventReference sfx_onCRank { get; private set; }
    [field: SerializeField] public EventReference sfx_onBRank { get; private set; }
    [field: SerializeField] public EventReference sfx_onARank { get; private set; }
    [field: SerializeField] public EventReference sfx_onSRank { get; private set; }



    [field: Header("UI")]
    [field: SerializeField] public EventReference sfx_onButtonPress { get; private set; }
    [field: SerializeField] public EventReference sfx_onButtonHover { get; private set; }
    [field: SerializeField] public EventReference sfx_closeOverlay { get; private set; }
    [field: SerializeField] public EventReference sfx_openOverlay { get; private set; }

    public static FmodEvent Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
            print("more than one Fmod Event instance in the scene");

        Instance = this;
    }
}
