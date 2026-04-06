using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public abstract class FsmResources : IAddressableService, IFsmService
{
    public virtual void Dispose() { }

    public virtual float GetStoppingDistance() => 0f;
   

    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        if (!await TryLoadPathingData(data)) return false;

        var subDataList = new List<ScriptableObject>();
        var handles = new List<AsyncOperationHandle<ScriptableObject>>();

        foreach (var key in data.subDataKeys)
        {
            if (string.IsNullOrWhiteSpace(key)) { DebugLogs.RequireNotNull(key, "addressKey", this); return false; }

            var handleNullasble = await AddressableLoader.TryLoadAssetAsync<ScriptableObject>(key);
            if (handleNullasble is null || !handleNullasble.Value.IsValid() || handleNullasble.Value.Result is null)
            {
                DebugLogs.Err($"handle was null for address key: {key}", this);
                foreach (var h in handles) Addressables.Release(h);
                return false;
            }
            
            DebugLogs.Log($"Successfully loaded sub data for key: {key}", this);

            var handle = handleNullasble.Value;
            subDataList.Add(handle.Result);
            handles.Add(handle);

        }

        ExtractData(subDataList);

        foreach (var h in handles) Addressables.Release(h);
        return true;

    }

    protected virtual Task<bool> TryLoadPathingData(FeatureMeta meta) => Task.FromResult(true);

 

    protected abstract void ExtractData(IReadOnlyList<ScriptableObject> subData);
   
}
