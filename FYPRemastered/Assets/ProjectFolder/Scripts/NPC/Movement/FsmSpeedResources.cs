using Services.Internal;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public sealed class FsmSpeedResources : FsmResources, IFsmSpeedService
{
   
    private float _walkSpeed;
    private float _sprintSpeed;
    private float _sprintEnterDistance;
    private float _sprintExitDistance;



    public float GetWalkSpeed() => _walkSpeed;


    public float GetSprintSpeed() => _sprintSpeed;



    public float GetSprintEnterDistance() => _sprintEnterDistance;


    public float GetSprintExitDistance() => _sprintExitDistance;

    protected override void ExtractData(IReadOnlyList<ScriptableObject> subData)
    {
        DebugLogs.Log($"Extracting data from {subData.Count} SO's", this);
        foreach (var d in subData)
        {
            if (d is AgentSpeedData s)
            {
                _walkSpeed = s.WalkSpeed;
                _sprintSpeed = s.SprintSpeed;
                _sprintEnterDistance = s.SprintEnterDistance;
                _sprintExitDistance = s.SprintExitDistance;
            }
        }
    }
}

