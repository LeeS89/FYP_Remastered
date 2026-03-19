using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ChaseResources : IChaseService, IAddressableService
{
    private AsyncOperationHandle<AgentChaseData>? _chaseDataHandle;
    private AgentChaseData _data;


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

    public bool TryGetCandidates(List<Vector3> buffer)
    {
        throw new System.NotImplementedException();
    }

   

    public void Dispose()
    {
        if(_chaseDataHandle.HasValue && _chaseDataHandle.Value.IsValid())
            Addressables.Release(_chaseDataHandle.Value);
    }
}
