using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;



public sealed class ChaseResources : FsmResources, IChaseService
{

    private float _minStoppingDistance;
    private float _maxStoppingDistance;


   /* public override async Task<bool> TryInitialiseAsync(FeatureMeta data)
    {
        string addressKey = data.addressKey;
        if (string.IsNullOrWhiteSpace(addressKey)) { DebugLogs.RequireNotNull(addressKey, "addressKey", this); return false; }

        var chaseDataHandle = await AddressableLoader.TryLoadAssetAsync<AgentChaseData>(addressKey);
        
        if(!chaseDataHandle.HasValue || !chaseDataHandle.Value.IsValid())
        {
            DebugLogs.Nre(chaseDataHandle, "Chase data handle", this);
            return false;
        }

        var chaseData = chaseDataHandle.Value.Result;

        if(chaseData == null)
        {
            DebugLogs.Nre(chaseData, "Agent chase data", this);
            Addressables.Release(chaseDataHandle.Value);
            return false;
        }

        ExtractData(chaseData);

        Addressables.Release(chaseDataHandle.Value);

        return true;
    }*/



    protected override void ExtractData(IReadOnlyList<ScriptableObject> subData)
    {
        foreach (var data in subData)
        {
            if (data is AgentChaseData chaseData)
            {
                _minStoppingDistance = chaseData.MinStoppingDistance;
                _maxStoppingDistance = chaseData.MaxStoppingDistance;
            }
        }
    }


    public override float GetStoppingDistance() => Random.Range(_minStoppingDistance, _maxStoppingDistance);

}
