using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class GlobalFeedbackManager : MonoBehaviour
{


    public Action PlayGlobalFeedback;

    Transform _ballTransform;
    Transform _paddleTransform;
    [SerializeField]Transform[] _wallTransforms;
    BrickPool _brickPool;
    [SerializeField] Transform[] _brickTransforms;

    [Header("OnHitBallAnimation")]
    [SerializeField] AnimationCurve easeOutElastic;
    [SerializeField] float animationDuration;
    [SerializeField] float _startingscaleMultiplier;
    [Header("BallHit")]
    public float _ballHitPaddleTargetScale;
    public float _ballHitWallTargetScale;
    public float _ballHitBallTargetScale;
    public float _ballHitBrickTargetScale;

    [Header("BrickDestroy")]
    public float _brickDestroyPaddleTargetScale;
    public float _brickDestroyWallTargetScale;
    public float _brickDestroyBallTargetScale;
    public float _brickDestroyBrickTargetScale;


    float _paddleScaleMultiplier, _ballScaleMultiplier, _wallScaleMultipler, _brickScaleMultipler;
    Coroutine _shakeWorldRoutine;

    // cached originals
    Vector3 ballOriginalScale = new Vector3(1, 1, 1);
    Vector3 paddleOriginalScale = new Vector3(1,1,1);
    Vector3[] wallOriginalScales = Array.Empty<Vector3>();
    Vector3[] brickOriginalScales = Array.Empty<Vector3>();



    public static GlobalFeedbackManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
            print("more than one instance in the scene");

        Instance = this;

        _brickPool = FindAnyObjectByType<BrickPool>();

        // fallback finds if not assigned in inspector (only once)
        if (_paddleTransform == null)
        {
            var p = FindAnyObjectByType<PaddleHealth>();
            if (p != null) _paddleTransform = p.transform;
        }

        if (_ballTransform == null)
        {
            var b = FindAnyObjectByType<Ball>();
            if (b != null) _ballTransform = b.transform;
        }

        //if (_wallTransforms == null || _wallTransforms.Length == 0)
        //{
        //    var walls = GameObject.FindGameObjectsWithTag("Wall");
        //    _wallTransforms = walls.Select(w => w.transform).ToArray();
        //}
        List<GameObject> bricks = _brickPool.GetListOfBrick();
        _brickTransforms = new Transform[bricks.Count];

        for (int i = 0; i < bricks.Count; i++)
        {
            _brickTransforms[i] = bricks[i].transform;
        }

        // cache original scales
        if (_paddleTransform != null)
            paddleOriginalScale = _paddleTransform.localScale;

        if (ballOriginalScale != null)
            ballOriginalScale = _ballTransform.localScale;

        wallOriginalScales = new Vector3[_wallTransforms != null ? _wallTransforms.Length : 0];
        for (int i = 0; i < wallOriginalScales.Length; i++)
            wallOriginalScales[i] = _wallTransforms[i].localScale;

        brickOriginalScales = new Vector3[_brickTransforms.Length];
        for (int i = 0; i < _brickTransforms.Length; i++)
            brickOriginalScales[i] = _brickTransforms[i].localScale;


        PlayGlobalFeedback += PlayShakeWorldAnimation;

    }
    private void OnDestroy()
    {
        PlayGlobalFeedback -= PlayShakeWorldAnimation;
    }

    void PlayShakeWorldAnimation()
    {
        if (_shakeWorldRoutine != null)
            StopCoroutine(_shakeWorldRoutine);

        // reset scales immediately to originals before starting (helps prevent stacking scale errors)
        ResetAllScalesToOriginal();

        _shakeWorldRoutine = StartCoroutine(AnimateShakeWorld());
    }
    void ResetAllScalesToOriginal()
    {
        if (_paddleTransform != null)
            _paddleTransform.localScale = paddleOriginalScale;

        if( _ballTransform != null)
            _ballTransform.localScale = ballOriginalScale;

        for (int i = 0; i < wallOriginalScales.Length; i++)
            if (_wallTransforms[i] != null)
                _wallTransforms[i].localScale = wallOriginalScales[i];

        for(int i=0; i < brickOriginalScales.Length;i++)
            if(_brickTransforms[i] != null)
                _brickTransforms[i].localScale = brickOriginalScales[i];
    }


    IEnumerator AnimateShakeWorld()
    {
        float time = 0f;

        // precompute targets
        Vector3 paddleTarget = paddleOriginalScale * _paddleScaleMultiplier;
        Vector3 paddleStart = paddleOriginalScale * _startingscaleMultiplier;

        Vector3 ballTarget = ballOriginalScale * _ballScaleMultiplier;
        Vector3 ballStart = ballOriginalScale * _startingscaleMultiplier;

        Vector3[] wallTargets = new Vector3[wallOriginalScales.Length];
        Vector3[] wallStarts = new Vector3[wallOriginalScales.Length];

        for (int i = 0; i < wallOriginalScales.Length; i++)
        {
            wallTargets[i] = wallOriginalScales[i] * _wallScaleMultipler;
            wallStarts[i] = wallOriginalScales[i] * _startingscaleMultiplier;
        }

        Vector3[] brickTargets = new Vector3[brickOriginalScales.Length];
        Vector3[] brickStarts = new Vector3[brickOriginalScales.Length];
        for (int i = 0; i < brickOriginalScales.Length; i++)
        {
            brickTargets[i] = brickOriginalScales[i] * _brickScaleMultipler;
            brickStarts[i] = brickOriginalScales[i] * _startingscaleMultiplier;
        }

        while (time < animationDuration)
        {
            float normalized = time / animationDuration;
            float curveValue = easeOutElastic.Evaluate(normalized);

            // paddle
            if (_paddleTransform != null)
                _paddleTransform.localScale = Vector3.LerpUnclamped(paddleStart, paddleTarget, curveValue);

            // ball
            if (_ballTransform != null)
                _ballTransform.localScale = Vector3.LerpUnclamped(ballStart, ballTarget, curveValue);

            // walls
            for (int i = 0; i < _wallTransforms.Length; i++)
            {
                var wt = _wallTransforms[i];
                if (wt == null) continue;
                wt.localScale = Vector3.LerpUnclamped(wallStarts[i], wallTargets[i], curveValue);
            }

            //bricks
            for(int i=0; i < _brickTransforms.Length;i++)
            {
                var bt = _brickTransforms[i];
                if (bt == null || !bt.gameObject.activeInHierarchy) continue;
                bt.localScale = Vector3.LerpUnclamped(brickStarts[i], brickTargets[i], curveValue);
            }

            time += Time.deltaTime;
            yield return null;
        }

        // ensure exact final and then restore originals

        if (_ballTransform!= null)
            _ballTransform.localScale = ballOriginalScale;

        if (_paddleTransform != null)
            _paddleTransform.localScale = paddleOriginalScale;

        for (int i = 0; i < _wallTransforms.Length; i++)
            if (_wallTransforms[i] != null)
                _wallTransforms[i].localScale = wallOriginalScales[i];

        _shakeWorldRoutine = null;
    }
    public void SetSizeCapForBall()
    {
        _ballScaleMultiplier = _ballHitBallTargetScale;
        _paddleScaleMultiplier = _ballHitPaddleTargetScale;
        _wallScaleMultipler = _ballHitWallTargetScale;
        _brickScaleMultipler = _ballHitBrickTargetScale;
    }
    public void SetSizeCapForBrickDestroy()
    {
        _ballScaleMultiplier = _brickDestroyBallTargetScale;
        _paddleScaleMultiplier = _brickDestroyPaddleTargetScale;
        _wallScaleMultipler = _brickDestroyWallTargetScale;
        _brickScaleMultipler = _brickDestroyBrickTargetScale;

    }
}
