using System;
using UnityEngine;

public static class RNGService
{
    private static System.Random _rng =
        new System.Random(Environment.TickCount);

    // PRD state per-stream
    public class PRDState
    {
        public int FailCount = 0;
    }

    static PRDState _critState = new PRDState();

    // Simple roll
    public static bool RollBernoulli(float chance)
    {
        return (float)_rng.NextDouble() < Mathf.Clamp01(chance);
    }

    // PRD roll
    public static bool RollPRD(
        float baseChance,
        float bonusPerFail,
        PRDState state)
    {
        float chance =
            Mathf.Clamp01(baseChance + state.FailCount * bonusPerFail);

        bool result =
            (float)_rng.NextDouble() < chance;

        if (result)
            state.FailCount = 0;
        else
            state.FailCount++;

        return result;
    }

    // Crit helper
    public static bool RollCrit(
        float chance,
        float bonusPerFail)
    {
        return RollPRD(
            chance,
            bonusPerFail,
            _critState);
    }

    public static void Reseed(int seed)
    {
        _rng = new System.Random(seed);
    }
}