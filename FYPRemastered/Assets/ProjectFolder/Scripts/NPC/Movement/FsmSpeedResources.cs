using Services.Internal;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class FsmSpeedResources : IAddressableService, IFsmSpeedService
{
    private AgentSpeedData _speedData;



    public async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        var spHandle = await AddressableLoader.TryLoadAssetAsync<AgentSpeedData>(addressKey);
        if (!spHandle.HasValue || !spHandle.Value.IsValid())
        {
            DebugLogs.Nre(spHandle, "Agent Speed Handle", this);
            return false;
        }

        _speedData = spHandle.Value.Result;
        if (_speedData == null)
        {
            DebugLogs.Nre(_speedData, "Agent Speed Data asset", this);
            Addressables.Release(spHandle.Value);
            return false;
        }

        DebugLogs.Err($"WalkSpeed: {_speedData.SprintSpeed}");
        return true;
        // await Task.CompletedTask;
    }



    public float GetWalkSpeed() => _speedData.WalkSpeed;


    public float GetSprintSpeed() => _speedData.SprintSpeed;


    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    public float GetSprintEnterDistance() => _speedData.SprintEnterdistance;


    public float GetSprintExitDistance() => _speedData.SprintExitdistance;

}

