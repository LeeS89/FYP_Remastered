using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private Collider[] _bulletTraceresults;
    private TraceComponent traceComp;
    private bool _traceEnabled = false;
    private List<BulletBase> _bullets;

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        traceComp = new TraceComponent();
        _bulletTraceresults = new Collider[_maxTraceResults];
        _bullets = new List<BulletBase>();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        Array.Clear(_bulletTraceresults, 0, _bulletTraceresults.Length);
        _bulletTraceresults = null;
        traceComp = null;
        _bullets.Clear();
        _bullets = null;

    }

    public bool TraceEnabled
    {
        get => _traceEnabled;
        set => _traceEnabled = value;
    }

    public void ToggleTrace(bool traceEnabled)
    {
        if(_traceEnabled == traceEnabled) { return; }

        _traceEnabled = traceEnabled;
        if(!_traceEnabled)
        {
            if(_bullets.Count > 0)
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
        int numBullets = traceComp.CheckTargetProximity(_traceLocation, _bulletTraceresults, _sphereRadius, _traceLayer);
        
        if(numBullets <= 0) { return; }
        
        for (int i = 0; i < numBullets; i++)
        {
            HandleTraceResults(i);
        }
        
    }

    private void HandleTraceResults(int j) // => Search instead for IDeflectable interface and call FireeBack()
    {
        GameObject obj = _bulletTraceresults[j].transform.parent.gameObject;

        BulletBase bullet = obj.GetComponentInChildren<BulletBase>();

        if (bullet == null || bullet.HasState(BulletBase.IsFrozen)) { return; }
        bullet.Freeze();
        _bullets.Add(bullet);
    }

    private void FireBulletsBack()
    {
        if (_bullets.Count == 0) { return; }

        StartCoroutine(FireBackDelay());
    }

    private IEnumerator FireBackDelay()
    {

        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            if (!_bullets[i].HasState(BulletBase.IsFrozen)) { continue; }

            _bullets[i].UnFreeze();
            _bullets.RemoveAt(i);
            yield return new WaitForSeconds(0.1f);
        }
       
    }

}
