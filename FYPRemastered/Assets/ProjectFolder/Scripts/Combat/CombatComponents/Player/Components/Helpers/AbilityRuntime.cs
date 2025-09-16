using System;
using System.Collections.Generic;
using UnityEngine;


public sealed class AbilityRuntime
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
    public AbilityTags Id => _def.Tag;
    public string Ids { get; private set; }


    private Action<bool> _executorReadyCallback;

    public AbilityRuntime(AbilityDef def, IAbilityOwner owner)
    {
        Ids = def.Tag.Id;
        _def = def;
        _owner = owner;
 
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
            EffectKind.FreezeAndTrack => new FreezeAndTrackExecutor(),
            EffectKind.SpawnAndFire => new ProjectileFireExecutor(eff),
            _ => null
        };
        return exec;
       
    }

    public void UpdateExecutorPool(PoolIdSO poolId)
    {
        foreach (var exec in _execs)
        {
            exec.UpdatePool(poolId);
        }
    }

    public bool TryActivate(float now)
    {
        if (!CanActivate(now)) return false;
        
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
        GatherAndDispatch(now, CuePhase.Impact);
        _nextTick += _def.ChannelTick;
    }

    public void FixedTick(float now)
    {
        if (!_def.UsesFixedUpdate) return;
        if (!_channeling) return;
        GatherAndDispatch(now, CuePhase.Impact);
    }

    private void GatherAndDispatch(float now, CuePhase phase)
    {
        int count = GatherTargets();
        Dispatch(new AbilityContext(_owner, _owner.FireOrigin, _hits, count, now, directionOffset: _owner.DirectionOffset, directionOrigin: _owner.DirectionOrigin), phase);
    }

    private void Dispatch(in AbilityContext context, CuePhase phase)
    {
        CueDef cueDef = phase switch
        {
            CuePhase.Start => _def.Start,
            CuePhase.Impact => _def.Impact,
            CuePhase.End => _def.End,
            _ => null
        };

        
        if(cueDef)_owner.PlayCue(cueDef);

        int n = Mathf.Min(_execs.Count, _def.Effects?.Length ?? 0);
        for(int i = 0; i < n; i++) _execs[i].Execute(context, _def.Effects[i], phase);

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
        
        if (!_owner.HasSufficientResources(_def.Costs)) return false;

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
        if(_def.Costs != null && _def.Costs.Length > 0) _owner.SpendAll(_def.Costs);
        if (_def.GrantTags != null) for (int i = 0; i < _def.GrantTags.Length; i++) _owner.AddTag(_def.GrantTags[i]);
        _cooldownEnd = now + _def.CooldownSeconds;
        //_owner.PlayCue(_def.Start);
    }

    
}
