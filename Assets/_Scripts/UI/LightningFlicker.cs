// LightningFlicker.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningFlicker : MonoBehaviour
{
    [Header("What to flicker")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private bool includeChildren = true;

    [Header("Flicker Timings (seconds)")]
    [SerializeField] private Vector2 onTimeRange = new Vector2(0.02f, 0.08f);
    [SerializeField] private Vector2 offTimeRange = new Vector2(0.02f, 0.12f);

    [Header("Behavior")]
    [SerializeField] private bool randomizeStartPhase = true;
    [SerializeField] private bool useUnscaledTime = true;

    private readonly List<Renderer> _targets = new List<Renderer>(8);
    private Coroutine _loop;
    private System.Random _rng;

    void Awake()
    {
        _rng = new System.Random(GetInstanceID());
        CollectTargets();
    }

    void OnEnable()
    {
        if (_loop == null) _loop = StartCoroutine(FlickerLoop());
    }

    void OnDisable()
    {
        if (_loop != null)
        {
            StopCoroutine(_loop);
            _loop = null;
        }
        SetEnabled(false);
    }

    private void CollectTargets()
    {
        _targets.Clear();

        if (renderers != null && renderers.Length > 0)
        {
            for (int i = 0; i < renderers.Length; i++)
                if (renderers[i]) _targets.Add(renderers[i]);
        }
        else
        {
            if (includeChildren)
                GetComponentsInChildren(true, _targets);
            else
            {
                var r = GetComponent<Renderer>();
                if (r) _targets.Add(r);
            }
        }
    }

    private IEnumerator FlickerLoop()
    {
        if (randomizeStartPhase)
            yield return TimeDelay(RandomRange(0.0f, 0.12f));

        while (true)
        {
            SetEnabled(true);
            yield return TimeDelay(RandomRange(onTimeRange.x, onTimeRange.y));

            SetEnabled(false);
            yield return TimeDelay(RandomRange(offTimeRange.x, offTimeRange.y));
        }
    }

    private void SetEnabled(bool value)
    {
        for (int i = 0; i < _targets.Count; i++)
        {
            var r = _targets[i];
            if (!r) continue;
            if (r.enabled != value) r.enabled = value;
        }
    }

    private float RandomRange(float min, float max)
    {
        if (max <= min) return min;
        return (float)(_rng.NextDouble() * (max - min) + min);
    }

    // NOTE: Return type is object so we can yield either WaitForSeconds or WaitForSecondsRealtime.
    private object TimeDelay(float seconds)
    {
        if (seconds <= 0f) return null;
        if (useUnscaledTime)
            return WaitForSecondsRealtimePool.Get(seconds);
        else
            return WaitForSecondsPool.Get(seconds);
    }
}

static class WaitForSecondsPool
{
    private static readonly Dictionary<int, WaitForSeconds> _pool = new();

    public static WaitForSeconds Get(float seconds)
    {
        int key = Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f)); // quantize to ms
        if (!_pool.TryGetValue(key, out var wfs))
        {
            wfs = new WaitForSeconds(key / 1000f);
            _pool[key] = wfs;
        }
        return wfs;
    }
}

static class WaitForSecondsRealtimePool
{
    private static readonly Dictionary<int, WaitForSecondsRealtime> _pool = new();

    public static WaitForSecondsRealtime Get(float seconds)
    {
        int key = Mathf.Max(0, Mathf.RoundToInt(seconds * 1000f)); // quantize to ms
        if (!_pool.TryGetValue(key, out var wfs))
        {
            wfs = new WaitForSecondsRealtime(key / 1000f);
            _pool[key] = wfs;
        }
        else
        {
            wfs.waitTime = key / 1000f;
        }
        return wfs;
    }
}
