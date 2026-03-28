using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChaseServiceBridge : StateServiceBridge<IChaseService>, IFsmChaseData
{
    private readonly ITargetProvider _targetProvider;
    private readonly IDistanceMonitoringService _distanceService;

    public ChaseServiceBridge(IChaseService service, IDistanceMonitoringService distService, ITargetProvider targetProvider) : base(service)
    { (_distanceService, _targetProvider) = (distService, targetProvider); }

    public override void ReleaseCandidates(List<Vector3> buffer)
    {
        throw new NotImplementedException();
    }



    public override bool TryGetDestinationCandidates(List<Vector3> buffer)
    {
        if (buffer is null || _targetProvider is null) return false;
        buffer.Clear();
        if (!_targetProvider.TryGetTargetPosition(out var pos) || pos is null) return false;
        buffer.Add(pos.Value);
        return true;
    }

    // Needs targets ITargetable
    private bool TryGetTarget(out ITargetable target) => _targetProvider.TryGetTarget(out target);

    public bool TargetIsMoving()
    {
        if (!TryGetTarget(out var target)) return false;
        return target.IsMoving();
    }

    public bool TryRegisterDistanceMonitoring(IInstanceIdentifiable id, Vector3 currentPosition, Action<float> callback, out float initDist)
    {

        initDist = 0f; // Remember to get
                       //     currentPosition.SqrDistanceTo

        if (id is null || callback is null) return false;
        if (!_targetProvider.TryGetTarget(out var target)) return false;
        if (target.Position() == null) return false;
        if (_distanceService.TryRegisterSubscriber(id, currentPosition, target, callback))
        {
            initDist = currentPosition.SqrDistanceTo(target.Position().Value);
            return true;
        }

        return false;//_distanceService.TryRegisterSubscriber(id, currentPosition, target, callback);
    }

    public bool TryUnregisterDistanceMonitoring(IInstanceIdentifiable id)
    {
        if (id is null) return false;
        return _distanceService.TryUnregisterSubscriber(id);
    }
}
