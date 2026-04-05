using UnityEngine;

public class Spliter : ABSAbility
{
    [Header("Split Chance")]
    public float baseChance = 10f;        // starting chance %
    public float chanceIncrease = 5f;     // added per failure
    public float maxChance = 100f;

    private float currentChance;

    private void Start()
    {
        currentChance = baseChance;
    }
    public override void OnHit(HitContext ctx)
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= currentChance)
        {
            Ball ballCopy = Instantiate(_ball, ctx._brick.transform.position, ctx._brick.transform.rotation);
            ballCopy.tag = "CopyBall";
            ballCopy._copyBall = true;
            // Reset after success
            currentChance = baseChance;
        }
        else
        {
            // Increase chance on failure
            currentChance = Mathf.Min(currentChance + chanceIncrease, maxChance);
        }
    }
}
