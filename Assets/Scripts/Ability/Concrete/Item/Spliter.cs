using UnityEngine;

public class Spliter : ABSAbility
{
    [Header("Split Chance")]
    public float baseChance = 10f;        // starting chance %
    public float chanceIncrease = 5f;     // added per failure
    public float maxChance = 100f;

    private float currentChance;

    private GameObject _ball;

    private void Start()
    {
        _ball = FindAnyObjectByType<Ball>().gameObject;
        currentChance = baseChance;

    }
    public override void OnHit(HitContext ctx)
    {
        float roll = Random.Range(0f, 100f);

        if (roll <= currentChance)
        {
            Instantiate(_ball, ctx._brick.transform.position, ctx._brick.transform.rotation);
            _ball.tag = "CopyBall";
            _ball.GetComponent<Ball>()._copyBall = true;
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
