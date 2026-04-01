using Services.Internal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class ChaseResources : IChaseService, IAddressableService
{
    private AsyncOperationHandle<AgentChaseData>? _chaseDataHandle;
    private AgentChaseData _data;

/*    [Obsolete]
    private readonly IFsmTargetQuery _targetQuery;
    [Obsolete]
    private readonly IFsmNavigationQuery _navQuery;
    private readonly IDistanceMonitoringService _distService;*/

  /*  private ChaseResources() { }

    [Obsolete]
    public ChaseResources(IFsmNavigationQuery navQuery, IFsmTargetQuery targetRegistry, IDistanceMonitoringService distanceService)
    {
        _navQuery = navQuery;
        _targetQuery = targetRegistry;
        _distService = distanceService;
    }*/

    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        _chaseDataHandle = await AddressableLoader.TryLoadAssetAsync<AgentChaseData>(addressKey);
        
        if(!_chaseDataHandle.HasValue || !_chaseDataHandle.Value.IsValid())
        {
            DebugLogs.Nre(_chaseDataHandle, "Chase data handle", this);
            return false;
        }

        _data = _chaseDataHandle.Value.Result;

        if(_data == null)
        {
            DebugLogs.Nre(_data, "Agent chase data", this);
            Addressables.Release(_chaseDataHandle.Value);
            return false;
        }

        return true;
    }

  

   
  

    public void Dispose()
    {
        if(_chaseDataHandle.HasValue && _chaseDataHandle.Value.IsValid())
            Addressables.Release(_chaseDataHandle.Value);
    }


 
}
