using System;
using UnityEngine;

public abstract class NPCControllerBase : ComponentEvents
{
    protected EnemyEventManager _eEventManager;
    protected FieldOfViewManager _fovhandler;
    [SerializeField] protected FieldOfViewParams _fovParams;
    protected Action<bool> OnVisibilityCallback;
    protected Action<bool> OnAimCheckCallback;
    protected Action<bool> OnMeleeRangeCheckCallback;
    protected FSMPolicy? _currentPolicy;
    protected uint _currentPolicyVersion;


    public State CurrentState { get; protected set; }

    public override void RegisterLocalEvents(EventManager eventManager)
    {
        _eEventManager = eventManager as EnemyEventManager;
        OnVisibilityCallback = OnVisibilityGained;
        OnAimCheckCallback = OnAimEnter;
        OnMeleeRangeCheckCallback = OnMeleeRangeEnter;
        _fovhandler = new FieldOfViewManager(_fovParams, OnVisibilityCallback, OnAimCheckCallback, OnMeleeRangeCheckCallback, new AITraceComponent());
        base.RegisterLocalEvents(_eEventManager);
        RegisterGlobalEvents();
    }

    public override void UnRegisterLocalEvents(EventManager eventManager)
    {
        OnVisibilityCallback = null;
        OnAimCheckCallback = null;
        OnMeleeRangeCheckCallback = null;
        base.UnRegisterLocalEvents(_eEventManager);
        UnRegisterGlobalEvents();
    }

    protected abstract void OnVisibilityGained(bool seen);

    protected abstract void OnAimEnter(bool aiming);

    protected abstract void OnMeleeRangeEnter(bool targetInRange);

    protected abstract void ChangeState(State state, Transform targetPos = null);

    protected abstract void SetAndChaseTarget(Transform targetPosition);

    protected abstract void OnPathValidationResult(bool pathBlocked, FSMPolicy policy);

    protected abstract void Engage();

    protected abstract void OnDamageTaken(float remainingHealth);

    protected virtual void Update() => _fovhandler?.Tick();

    protected abstract void PolicyResult(in FSMPolicyResult result);

    public abstract void LogUnhandled(IntentStateBase state, PolicyNotification notification);
}
