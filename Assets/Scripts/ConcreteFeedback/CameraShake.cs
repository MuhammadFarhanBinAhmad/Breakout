using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float maxAmplitude = 1f;
    public float maxRotationAngle = 10f;
    public float traumaDecay = 1f;

    [Range(1f, 3f)]
    public float traumaPower = 2f;

    public float frequency = 2f;

    [Header("Transform Shake")]
    [Tooltip("Maximum transform position offset")]
    public float transformShakeStrength = 0.5f;

    [Tooltip("Seed for deterministic shake")]
    public int shakeSeed = 1337;

    private float trauma;

    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;

    private Vector3 originalLocalPos;

    // Seeded offsets
    private float seedX;
    private float seedY;

    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        originalLocalPos = transform.localPosition;

        // Deterministic seeds
        seedX = shakeSeed * 0.37f;
        seedY = shakeSeed * 0.73f;
    }

    private void Update()
    {

        if (trauma <= 0f)
        {
            ResetShake();
            return;
        }

        float shake = Mathf.Pow(trauma, traumaPower);
        float time = Time.time * frequency;

        // -------- Cinemachine Noise --------
        noise.m_AmplitudeGain = maxAmplitude * shake;
        noise.m_FrequencyGain = frequency;

        // Deterministic Dutch rotation
        float rotNoise =
            (Mathf.PerlinNoise(seedX, time) - 0.5f) * 2f;

        vcam.m_Lens.Dutch = maxRotationAngle * shake * rotNoise;

        // -------- Transform Position Shake --------
        float x =
            (Mathf.PerlinNoise(seedX, time) - 0.5f) * 2f;
        float y =
            (Mathf.PerlinNoise(seedY, time) - 0.5f) * 2f;

        Vector3 offset = new Vector3(x, y, 0f)
                         * transformShakeStrength
                         * shake;

        transform.localPosition = originalLocalPos + offset;

        // Decay trauma
        trauma = Mathf.Clamp01(trauma - traumaDecay * Time.deltaTime);
    }

    private void ResetShake()
    {
        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }

        if (vcam != null)
            vcam.m_Lens.Dutch = 0f;

        transform.localPosition = originalLocalPos;
    }

    // -------- Public API --------
    public void AddTrauma(float amount)
    {
        trauma = Mathf.Clamp01(trauma + Mathf.Abs(amount));
    }

    public float GetTrauma() => trauma;
}
