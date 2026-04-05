using Services.Internal;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class FsmSpeedResources : FsmResources, IFsmSpeedService
{
    private AgentSpeedData _speedData;
    private AsyncOperationHandle<AgentSpeedData>? _dataHandle;


    public override async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        _dataHandle = await AddressableLoader.TryLoadAssetAsync<AgentSpeedData>(addressKey);
        if (!_dataHandle.HasValue || !_dataHandle.Value.IsValid())
        {
            DebugLogs.Nre(_dataHandle, "Agent Speed Handle", this);
            return false;
        }

        _speedData = _dataHandle.Value.Result;
        if (_speedData == null)
        {
            DebugLogs.Nre(_speedData, "Agent Speed Data asset", this);
            Addressables.Release(_dataHandle.Value);
            return false;
        }

        DebugLogs.Err($"WalkSpeed: {_speedData.SprintSpeed}");
        return true;
        // await Task.CompletedTask;
    }



    public float GetWalkSpeed() => _speedData.WalkSpeed;


    public float GetSprintSpeed() => _speedData.SprintSpeed;


    public override void Dispose()
    {
        if (_dataHandle.HasValue && _dataHandle.Value.IsValid())
        {
            Addressables.Release(_dataHandle.Value);
            _dataHandle = null;
        }
    }

    public float GetSprintEnterDistance() => _speedData.SprintEnterdistance;


    public float GetSprintExitDistance() => _speedData.SprintExitdistance;

  
}

