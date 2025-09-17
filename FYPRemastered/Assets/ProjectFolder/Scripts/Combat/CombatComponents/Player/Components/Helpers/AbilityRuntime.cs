using System;
using System.Collections.Generic;
using UnityEngine;


public sealed class AbilityRuntime : CSBase
{
    readonly AbilityDef _def;
    readonly IAbilityOwner _owner;
    readonly Collider[] _hits;
    readonly List<IEffectExecutor> _execs = new();
    private float _cooldownEnd;
    private bool _channeling;
    private float _nextTick;

    public bool IsReady { get; set; } = true;
  //  private PlayerTraceManager _traceComp; // Optional
    private bool _inProgress = false;
  //  public AbilityTags Id => _def.Tag;
    public string Ids { get; private set; }

    private IPoolManager _startPhasePool;
    private IPoolManager _impactPhasePool;
    private IPoolManager _endPhasePool;
    private Action _spendResourcesCallback;


    public AbilityRuntime(AbilityDef def, IAbilityOwner owner)
    {
        Ids = def.Tag.Id;
        _def = def;
        _owner = owner;
 
        _spendResourcesCallback = SpendResources;

        int cap = Mathf.Max(64, def.Targeting?.Maxhits ?? 64);
        _hits = new Collider[cap];

        if(_def.Effects != null)
        {
            for(int i = 0; i < _def.Effects.Length; i++)
            {
                _execs.Add(CreateExecutor(_def.Effects[i]));
            }
        }
    }

    private IEffectExecutor CreateExecutor(EffectDef eff)
    {
        IEffectExecutor exec = eff.Kind switch
        {
            EffectKind.FreezeAndTrack => new FreezeAndTrackExecutor(_spendResourcesCallback),
            EffectKind.SpawnAndFire => new ProjectileFireExecutor(_spendResourcesCallback),
            _ => null
        };
        return exec;
       
    }



   /* public void UpdateExecutorPool(PoolIdSO poolId)
    {
        foreach (var exec in _execs)
        {
            exec.UpdatePool(poolId);
        }
    }*/

    public bool TryActivate(/*float now, */in AbilityResources resources)
    {
        
        float now = resources.Now;
        if (!CanActivate(now)) return false;

        /// Important: Set the pools before Commit, as Commit may fail if resources are insufficient
        /// If ability doesnt use pools, its ok to set null
        SetExecutorPools(in resources);

        Commit(now);
        GatherAndDispatch(now, CuePhase.Start);
       
        if (_def.IsChanneled && (_def.UsesFixedUpdate || _def.ChannelTick > 0))
        {
            _channeling = true;
            if(!_def.UsesFixedUpdate) _nextTick = now + _def.ChannelTick;
        }
        else
        {
            End(now);
        }

        return true;
    }

    private void SetExecutorPools(in AbilityResources resources)
    {
        /* _startPhasePool = resources._startPhasePool;
         _impactPhasePool = resources._impactPhasePool;
         _endPhasePool = resources._endPhasePool;*/
        foreach (var kv in resources.AbilityPools)
        {
            switch (kv.Key)
            {
                case CuePhase.Start:
                    _startPhasePool = kv.Value;
                    break;
                case CuePhase.Impact:
                    _impactPhasePool = kv.Value;
                     break;
                case CuePhase.End:
                    _endPhasePool = kv.Value;
                    break;
                default:
                    return;
            }
        }

    }

    public void OnInterrupted()
    {
        if (!_inProgress) return;
        End(0f);
    }

    public void End(float now)
    {
        if(_channeling) _channeling = false;
        if (!_inProgress) return;

        Dispatch(new AbilityContext(_owner, _owner.FireOrigin, _hits, 0, now, directionOffset: _owner.DirectionOffset, directionOrigin: _owner.DirectionOrigin), CuePhase.End);

        if(_def.GrantTags != null)
        {
            for(int i = 0; i < _def.GrantTags.Length; i++) _owner.RemoveTag(_def.GrantTags[i]);
        }
        _inProgress = false;
    }

    public void Tick(float now)
    {
        if (_def.UsesFixedUpdate) return;
        if (!_channeling || now < _nextTick) return;
        if (!HasSufficientResources())
        {
            End(now);
            return;
        }
        GatherAndDispatch(now, CuePhase.Impact);
        _nextTick += _def.ChannelTick;
    }

    public void FixedTick(float now)
    {
        if (!_def.UsesFixedUpdate) return;
        if (!_channeling) return;
        GatherAndDispatch(now, CuePhase.Impact);
    }

    private void GatherAndDispatch(float now, CuePhase phase, IPoolManager pool = null)
    {
        int count = GatherTargets();
        Dispatch(new AbilityContext(_owner, _owner.FireOrigin, _hits, count, now, directionOffset: _owner.DirectionOffset, directionOrigin: _owner.DirectionOrigin), phase);
    }

    private void Dispatch(in AbilityContext context, CuePhase phase, IPoolManager pool = null)
    {
        /*CueDef cueDef = phase switch
        {
            CuePhase.Start => _def.Start,
            CuePhase.Impact => _def.Impact,
            CuePhase.End => _def.End,
            _ => null
        };*/
        CueDef cueDef;
        IPoolManager pm;
        switch (phase)
        {
            case CuePhase.Start:
                pm = _startPhasePool;
                cueDef = _def.Start;
                break;
            case CuePhase.Impact:
                pm = _impactPhasePool;
                cueDef = _def.Impact;
                break;
            case CuePhase.End:
                pm = _endPhasePool;
                cueDef = _def.End;
                break;
            default:
                End(0f);
                return;
        }

        
        if(cueDef)_owner.PlayCue(cueDef);

        int n = Mathf.Min(_execs.Count, _def.Effects?.Length ?? 0);
        for(int i = 0; i < n; i++) _execs[i].Execute(context, _def.Effects[i], phase, pm);

    }

    private bool CanActivate(float now)
    {
        if (now < _cooldownEnd || _inProgress) return false;

        if (_def.BlockedByTags != null)
        {
            for (int i = 0; i < _def.BlockedByTags.Length; i++)
            {
                if (_owner.HasTag(_def.BlockedByTags[i])) return false;
            }
        }
        if (_def.RequiredTags != null)
        {
            for (int i = 0; i < _def.RequiredTags.Length; i++)
            {
                if(!_owner.HasTag(_def.RequiredTags[i])) return false;
            }
        }

        if (!HasSufficientResources()) return false;
        //if (!_owner.HasSufficientResources(_def.Costs)) return false;

        _inProgress = true;
        return _inProgress;
    }

    private int GatherTargets()
    {
        var tar = _def.Targeting;
        if (tar == null || tar.Mode != TargetingMode.Sphere) return 0;
        return Physics.OverlapSphereNonAlloc(_owner.FireOrigin.position, tar.Radius, _hits, tar.hitMask);
    }

    private void Commit(float now)
    {
        //if (_def.Costs != null && _def.Costs.Length > 0) CheckOrSpendResources(spend: true);//_owner.SpendAll(_def.Costs);
        if (_def.GrantTags != null) for (int i = 0; i < _def.GrantTags.Length; i++) _owner.AddTag(_def.GrantTags[i]);
        _cooldownEnd = now + _def.CooldownSeconds;
        //_owner.PlayCue(_def.Start);
    }

    private bool CheckOrSpendResources(bool spend = false)
    {
        bool hasEnoughResources = _owner.HasSufficientResources(_def.Costs);
        if (!spend) return hasEnoughResources;

        if (!hasEnoughResources) return false;
        _owner.SpendAll(_def.Costs);
        return true;
    }

    private bool HasSufficientResources() => _owner.HasSufficientResources(_def.Costs);
    

    private void SpendResources()
    {
        if (_def.Costs != null && _def.Costs.Length > 0)
            _owner.SpendAll(_def.Costs);
    }


    public override void OnInstanceDestroyed()
    {
        _startPhasePool = null;
        _impactPhasePool = null;
        _endPhasePool = null;
        Array.Clear(_hits, 0, _hits.Length);
    }
}
