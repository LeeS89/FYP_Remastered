using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

[Obsolete("", true)]
public class AbilitiesComponent : BaseAbilities
{
    [Header("Trace Center")]
    [SerializeField] private Transform _traceLocation;

    [Header("Trace Object Layer")]
    [SerializeField] private LayerMask _traceLayer;

    [Header("Trace shape size")]
    [SerializeField] private float _sphereRadius = 0.2f;

    [Header("Trace results max size")]
    [SerializeField] private int _maxTraceResults = 30;
    [SerializeField] private IFreezeAndDeflectable[] _bulletTraceresults;
   // [SerializeField] private List<IDeflectable> _deflectables;
    private PlayerTraceManager _traceComp;
    [SerializeField] private bool _traceEnabled = false;
    private List<Projectile> _bullets;

    private PlayerEventManager _playerEventManager;
    private int _deflectableCount = 0;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _playerEventManager = eventManager as PlayerEventManager;
        base.RegisterLocalEvents(_playerEventManager);
        _traceComp = new PlayerTraceManager(_maxTraceResults);
        _bulletTraceresults = new IFreezeAndDeflectable[_maxTraceResults];
       // _deflectables = new List<IDeflectable>();
       // _deflectables.EnsureCapacity(_maxTraceResults);
        _bullets = new List<Projectile>();

        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        base.UnRegisterLocalEvents(_playerEventManager);
        Array.Clear(_bulletTraceresults, 0, _bulletTraceresults.Length);
        
        UnRegisterGlobalEvents();
    }

    protected override void OnSceneComplete()
    {
        base.OnSceneComplete();
        _bulletTraceresults = null;
        _traceComp = null;
        _bullets.Clear();
        _bullets = null;
        _playerEventManager = null;
        _traceLocation = null;
    }

    public bool TraceEnabled
    {
        get => _traceEnabled;
        set => _traceEnabled = value;
    }

    public bool _testStartTrace = false;
    public bool _testStopTrace = false;

    private void Update()
    {
        if (_testStartTrace)
        {
            ToggleTrace(true);
            _testStartTrace = false;
        }
        if (_testStopTrace)
        {
            ToggleTrace(false);
            _testStopTrace = false;
        }
    }

    public void ToggleTrace(bool traceEnabled)
    {
        if(_traceEnabled == traceEnabled) { return; }

        _traceEnabled = traceEnabled;
        if(!_traceEnabled)
        {
            if(_bulletTraceresults != null && _bulletTraceresults.Length > 0)
            {
                FireBulletsBack();
            }
        }

    }

    private void FixedUpdate()
    {
        if (!_traceEnabled) { return; }

        SweepForBullets();
    }

    private void SweepForBullets()
    {
        _traceComp.CheckTargetProximity<IFreezeAndDeflectable>(_traceLocation, _bulletTraceresults, ref _deflectableCount, _sphereRadius, _traceLayer);
        
        if(_deflectableCount <= 0) { return; }
        
        for (int i = 0; i < _deflectableCount; i++)
        {
            //GameObject obj = _bulletTraceresults[i].gameObject;
           // if (!ComponentRegistry.TryGet<IDeflectable>(obj, out IDeflectable deflectable)) continue;
           var deflectable = _bulletTraceresults[i];
            deflectable?.Freeze();
            /*_deflectables.Add(deflectable);
            deflectable.Freeze();*/
            //HandleTraceResults(deflectable);
        }
        
    }

   /* private void HandleTraceResults(IDeflectable deflectable*//*int j*//*) // => Search instead for IDeflectable interface and call FireBack()
    {
        GameObject obj = _bulletTraceresults[j].transform.parent.gameObject;

        Projectile bullet = obj.GetComponentInChildren<Projectile>();

        if (bullet == null || bullet.HasState(Projectile.IsFrozen)) { return; }
        //bullet.Freeze();
        _bullets.Add(bullet);
    }*/

    private void FireBulletsBack()
    {
        if (_bulletTraceresults == null || _bulletTraceresults.Length == 0) return;
        //if (_bullets.Count == 0) { return; }

        StartCoroutine(FireBackDelay());
    }

    private IEnumerator FireBackDelay()
    {
        Debug.LogError("Deflectable count: "+ _deflectableCount);
        for (int i = _deflectableCount - 1; i >= 0; i--)
        {
            var d = _bulletTraceresults[i];
            if (d != null)
            {
                d.Deflect(ProjectileKickType.ReFire);
                _bulletTraceresults[i] = null;
            }
            yield return new WaitForSeconds(0.1f);

        }
        _deflectableCount = 0;

       /* for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            if (!_bullets[i].HasState(Projectile.IsFrozen)) { continue; }

            //_bullets[i].UnFreeze();
            _bullets.RemoveAt(i);
            yield return new WaitForSeconds(0.1f);
        }*/
       
    }

}
