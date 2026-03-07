using UnityEngine;
public enum TWEENTYPE
{
    NONE,
    LINEAR,
    SINE,
    QUAD,
    EXPO,
    REVERSE_QUAD,
    REVERSE_EXPO
}
public class TweenService : MonoBehaviour
{

    public static float GetEased(float t,TWEENTYPE type)
    {
        t = Mathf.Clamp01(t);
        switch (type)
        {
            case TWEENTYPE.LINEAR:
                return Linear(t);
            case TWEENTYPE.SINE:
                return Sine(t);
            case TWEENTYPE.EXPO:
                return Expo(t);
            case TWEENTYPE.QUAD:
                return Quad(t);
            case TWEENTYPE.REVERSE_QUAD:
                return 1f - Quad(1f - t);
            case TWEENTYPE.REVERSE_EXPO:
                return 1f - Expo(1f - t);
            case TWEENTYPE.NONE:
            default:
                return Linear(t); // treat NONE as linear by default
        }
        float Linear(float tt)
        {
            return Mathf.Clamp01(tt);
        }
        float Quad(float tt)
        {
            tt = Mathf.Clamp01(tt);
            if (tt < 0.5f)
                return 2f * tt * tt;
            else
                return -2f * tt * tt + 4f * tt - 1f;
        }
        float Sine(float tt)
        {
            tt = Mathf.Clamp01(tt);
            return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * tt);
        }
        float Expo(float tt)
        {
            tt = Mathf.Clamp01(tt);

            if (tt == 0f) return 0f;
            if (tt == 1f) return 1f;

            if (tt < 0.5f)
                return 0.5f * Mathf.Pow(2f, (20f * tt) - 10f);
            else
                return 1f - 0.5f * Mathf.Pow(2f, (-20f * tt) + 10f);
        }

    }
}
