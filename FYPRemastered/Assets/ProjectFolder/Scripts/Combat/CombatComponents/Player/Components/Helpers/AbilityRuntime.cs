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

    private TrackedFreezeables _freezeTracker;
    private PlayerTraceManager _traceComp; // Optional

    public AbilityTags Id => _def.Id;

    public AbilityRuntime(AbilityDef def, IAbilityOwner owner, PlayerTraceManager trace = null)
    {
        _def = def;
        _owner = owner;
        _traceComp = trace;
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
            EffectKind.FreezeAndTrack => new FreezeAndTrackExecutor(_freezeTracker ??= new TrackedFreezeables()),
            EffectKind.ReturnTrackedOnEnd => new ReturnTrackedOnEndExecutor(_freezeTracker ??= new TrackedFreezeables()),
            _ => null
        };
        return exec;
       
    }

    public bool TryActivate(float now)
    {
        if (!CanActivate(now)) return false;

        Commit(now);
        int count = GatherTargets();
        Dispatch(new AbilityContext(_owner, _owner.Origin, _hits, count, now), CuePhase.Start);

        if (_def.IsChanneled && _def.ChannelTick > 0)
        {
            _channeling = true;
            _nextTick = now + _def.ChannelTick;
        }
        else
        {
            End(now);
        }

        return true;
    }

    private void End(float now)
    {
        if(_channeling) _channeling = false;
        Dispatch(new AbilityContext(_owner, _owner.Origin, _hits, 0, now), CuePhase.End);

        if(_def.GrantTags != null)
        {
            for(int i = 0; i < _def.GrantTags.Length; i++) _owner.RemoveTag(_def.GrantTags[i]);
        }
    }

    public void Tick(float now)
    {
        if (!_channeling || now < _nextTick) return;
        int count = GatherTargets();
        Dispatch(new AbilityContext(_owner, _owner.Origin, _hits, count, now), CuePhase.Impact);
        _nextTick += _def.ChannelTick;
    }

    private void Dispatch(in AbilityContext context, CuePhase phase)
    {
        CueDef cueDef;
        switch (phase)
        {
            case CuePhase.Start:
                cueDef = _def.Start;
                break;
            case CuePhase.Impact:
                cueDef = _def.Impact;
                break;
            case CuePhase.End:
                cueDef = _def.End;
                break;
            default:
                cueDef = _def.Impact;
                break;
        }
        _owner.PlayCue(cueDef);

        for(int i = 0; i < _execs.Count; i++) _execs[i].Execute(context, _def.Effects[i], phase);

    }

    private bool CanActivate(float now)
    {
        if (now < _cooldownEnd) return false;
        if(_def.RequiredTags != null)
        {
            for (int i = 0; i < _def.RequiredTags.Length; i++)
            {
                if(!_owner.HasTag(_def.RequiredTags[i])) return false;
            }
        }
        if(_def.BlockedByTags != null)
        {
            for(int i = 0; i < _def.BlockedByTags.Length; i++)
            {
                if (_owner.HasTag(_def.BlockedByTags[i])) return false;
            }
        }
        if (!_owner.HasSufficientResources(_def.Costs)) return false;

        return true;
    }

    private int GatherTargets()
    {
        var tar = _def.Targeting;
        if (tar != null || tar.Mode != TargetingMode.Sphere) return 0;
        return Physics.OverlapSphereNonAlloc(_owner.Origin.position, tar.Radius, _hits, tar.hitMask);
    }

    private void Commit(float now)
    {
        if(_def.Costs != null && _def.Costs.Length > 0) _owner.SpendAll(_def.Costs);
        if (_def.GrantTags != null) for (int i = 0; i < _def.GrantTags.Length; i++) _owner.AddTag(_def.GrantTags[i]);
        _cooldownEnd = now + _def.CooldownSeconds;
        _owner.PlayCue(_def.Start);
    }

    
}
