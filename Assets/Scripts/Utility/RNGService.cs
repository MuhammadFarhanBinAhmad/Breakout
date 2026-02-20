using UnityEngine;
using System;

public class RNGService : MonoBehaviour
{
    private static RNGService _instance;
    public static RNGService Instance => _instance ??= new RNGService(Environment.TickCount);

    private System.Random _rng;

    public RNGService(int seed)
    {
        _rng = new System.Random(seed);
    }

    public RNGService() : this(Environment.TickCount) { }

    // Simple Bernoulli roll
    public bool RollBernoulli(float chance)
    {
        return (float)_rng.NextDouble() < Mathf.Clamp01(chance);
    }

    // PRD state per-stream
    public class PRDState { public int FailCount = 0; }

    // PRD roll: baseChance + FailCount * bonusPerFail. If roll succeeds FailCount reset to 0, else increment.
    public bool RollPRD(float baseChance, float bonusPerFail, PRDState state)
    {
        float chance = Mathf.Clamp01(baseChance + state.FailCount * bonusPerFail);
        bool result = (float)_rng.NextDouble() < chance;
        if (result) state.FailCount = 0;
        else state.FailCount++;
        return result;
    }

    // Optional seeded reset
    public void Reseed(int seed) => _rng = new System.Random(seed);
}
