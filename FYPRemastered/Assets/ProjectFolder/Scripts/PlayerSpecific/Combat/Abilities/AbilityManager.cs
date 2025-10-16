using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.GridLayoutGroup;

public class AbilityManager
{
    readonly AbilityDefinition _def;
    readonly IAbilityOwner _owner;
    readonly Collider[] _hits;
    readonly IEffectExecutor _executor;
   // readonly List<IEffectExecutor> _execs = new();
    private float _cooldownEnd;
    private bool _channeling;
    private float _nextTick;

    public bool IsReady { get; set; } = true;
    //  private PlayerTraceManager _traceComp; // Optional
    private bool _inProgress = false;
   
    private IPoolManager _startPhasePool, _impactPhasePool, _endPhasePool;


    private Action _spendResourcesCallback;
    private Action<string, IPoolManager> OnPoolReceived;
    // private string StartPoolId, ImpactPoolId, EndPoolId;
    private PoolIdSO StartPoolId, ImpactPoolId, EndPoolId;

    public Transform ExecuteOrigin { get; private set; }
    public Transform DirectionOrigin { get; private set; }
    public float DirectionOffset { get; private set; } = 0f;


    public AbilityManager(AbilityDefinition def, IAbilityOwner owner)
    {
        _def = def;

      //  ExecuteOrigin = owner.ExecuteOrigin;
      //  DirectionOrigin = owner.DirectionOrigin != null ? owner.DirectionOrigin : owner.ExecuteOrigin;
      //  DirectionOffset = owner.DirectionOffset;

        if (_def.UsesPools())
        {
            OnPoolReceived = OnPoolRequestComplete;
            RequestPools(_def);
        }


        _owner = owner;


        _spendResourcesCallback = SpendResources;

        if (_def.Targeting?.Maxhits > 0)
        {
            int cap = Mathf.Max(64, _def.Targeting?.Maxhits ?? 64);
            _hits = new Collider[cap];
        }

        if (_def.EffectKind != EffectType.None) _executor = CreateExecutor(_def.EffectKind);

    }

    private void SetOrigins(AbilityOrigins origins)
    {
        if (origins == null)
        {
            Debug.LogError("[AbilityRuntime] Origins was NULL. Did you forget to instantiate/assign it?");
            return;
        }
        ExecuteOrigin = origins.Position;
        DirectionOrigin = origins.DirectionOrigin;
        DirectionOffset = origins.DirectionOffset;
    }

    private IEffectExecutor CreateExecutor(EffectType kind)
    {
        IEffectExecutor exec = kind switch
        {
            EffectType.FreezeAndTrack => new FreezeAndTrackExecutor(_spendResourcesCallback),
            EffectType.SpawnAndFire => new ProjectileFireExecutor(_spendResourcesCallback),
            _ => null
        };
        return exec;

    }

    private void RequestPools(AbilityDefinition def)
    {
        if (def.StartPhasePool != null)
        {
            StartPoolId = def.StartPhasePool;
            this.RequestPool(StartPoolId, OnPoolReceived);
        }
        if (def.ImpactPhasePool != null)
        {
            ImpactPoolId = def.ImpactPhasePool;
            this.RequestPool(ImpactPoolId, OnPoolReceived);
        }
        if (def.EndPhasePool != null)
        {
            EndPoolId = def.EndPhasePool;
            this.RequestPool(EndPoolId, OnPoolReceived);
        }
    }

    private void OnPoolRequestComplete(string id, IPoolManager pool)
    {
        if (string.IsNullOrEmpty(id) || pool == null) return;

        if (StartPoolId != null && id == StartPoolId.Id) _startPhasePool = pool;

        if (ImpactPoolId != null && id == ImpactPoolId.Id) _impactPhasePool = pool;

        if (EndPoolId != null && id == EndPoolId.Id) _endPhasePool = pool;

    }


    public void ChangePools(CuePhase phase, IPoolManager pool)
    {
        switch (phase)
        {
            case CuePhase.Start:
                _startPhasePool = pool;
                break;
            case CuePhase.Impact:
                _impactPhasePool = pool;
                break;
            case CuePhase.End:
                _endPhasePool = pool;
                break;
            default:
#if UNITY_EDITOR
                Debug.LogError("A valid pool manager must be provided");
#endif
                return;
        }
    }
    /* public void UpdateExecutorPool(PoolIdSO poolId)
     {
         foreach (var exec in _execs)
         {
             exec.UpdatePool(poolId);
         }
     }*/

    public bool TryActivate(float now, AbilityOrigins origins = null)
    {

        // float now = resources.Now;
        if (!CanActivate(now)) return false;

        SetOrigins(origins);
        /// Important: Set the pools before Commit, as Commit may fail if resources are insufficient
        /// If ability doesnt use pools, its ok to set null
       // SetExecutorPools(in resources);

        Commit(now);
        GatherAndDispatch(now, CuePhase.Start);

        if (_def.IsChanneled && (_def.UsesFixedUpdate || _def.ChannelTick > 0))
        {
            _channeling = true;
            if (!_def.UsesFixedUpdate) _nextTick = now + _def.ChannelTick;
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
        if (_channeling) _channeling = false;
        if (!_inProgress) return;

        Dispatch(new AbilityContext(_owner, CuePhase.End, ExecuteOrigin, _hits, 0, now, directionOffset: DirectionOffset, directionOrigin: DirectionOrigin));

        if (_def.GrantTags != null)
        {
            for (int i = 0; i < _def.GrantTags.Length; i++) _owner.RemoveTag(_def.GrantTags[i]);
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
        Dispatch(new AbilityContext(_owner, phase, ExecuteOrigin, _hits, count, now, directionOffset: DirectionOffset, directionOrigin: DirectionOrigin));
    }

    private void Dispatch(in AbilityContext context, IPoolManager pool = null)
    {
        
        CueDef cueDef;
        IPoolManager pm;
        switch (context.Phase)
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


        if (cueDef) _owner.PlayCue(cueDef);

        _executor?.Execute(context, _def.EffectKind, pm);
       // int n = Mathf.Min(_execs.Count, _def.Effects?.Length ?? 0);
       // for (int i = 0; i < n; i++) _execs[i].Execute(context, _def.Effects[i], phase, pm);

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
                if (!_owner.HasTag(_def.RequiredTags[i])) return false;
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
#if UNITY_EDITOR
        if(ExecuteOrigin != null)
        DebugExtension.DebugWireSphere(ExecuteOrigin.position, Color.blue, tar.Radius);
#endif
        return Physics.OverlapSphereNonAlloc(ExecuteOrigin.position, tar.Radius, _hits, tar.hitMask);
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
        bool hasEnoughResources = _owner.HasSufficientResources(_def.Cost);
        if (!spend) return hasEnoughResources;

        if (!hasEnoughResources) return false;
        _owner.Spend(_def.Cost);
        return true;
    }

    private bool HasSufficientResources() => _owner.HasSufficientResources(_def.Cost);


    private void SpendResources()
    {
       // if (_def.Cost /*&& _def.Cost.Length > 0*/)
            _owner.Spend(_def.Cost);
    }
}
