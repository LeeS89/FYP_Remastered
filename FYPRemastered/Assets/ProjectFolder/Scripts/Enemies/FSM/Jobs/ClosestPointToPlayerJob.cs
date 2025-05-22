using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class ClosestPointToPlayerJob : MonoBehaviour
{
    public static ClosestPointToPlayerJob Instance { get; private set; }

    public UniformZoneGridManager zoneGridManager;

    
    private NativeList<Vector3> _samplePositions;
    private NativeArray<int> _closestIndexToPlayer;
    private NativeArray<float> _closestDistance;

    private void Start()
    {
        Instance = this;

        _samplePositions = new NativeList<Vector3>(1024, Allocator.Persistent);
        _closestIndexToPlayer = new NativeArray<int>(1, Allocator.Persistent);
        _closestDistance = new NativeArray<float>(1, Allocator.Persistent);

    }

   

    public void AddSamplePointData(SamplePointDataSO sampleData)
    {
        _samplePositions.Clear();
        foreach (var pos in sampleData.savedPoints)
        {
            _samplePositions.Add(pos.position);
        }
    }

    private void RunClosestPointJob()
    {
        _closestDistance[0] = float.MaxValue;
        var sampleArray = _samplePositions.ToArray(Allocator.TempJob);

        var job = new ClosestPointJob()
        {
            samplePositions = sampleArray,
            closestIndexToPlayer = _closestIndexToPlayer,
            playerPosition = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position,
            closestDistance = _closestDistance
        };

        var jobHandle = job.Schedule(_samplePositions.Length, 64);
        jobHandle.Complete();

        //Debug.Log("Closest Point Index: " + _closestIndexToPlayer[0]);
        zoneGridManager.SetNearestIndexToPlayer(_closestIndexToPlayer[0]);
        sampleArray.Dispose();
    }

    private void OnDestroy()
    {
        if (_samplePositions.IsCreated)
            _samplePositions.Dispose();
        if (_closestIndexToPlayer.IsCreated)
            _closestIndexToPlayer.Dispose();
        if (_closestDistance.IsCreated)
            _closestDistance.Dispose();
    }

    [BurstCompile]
    public struct ClosestPointJob : IJobParallelFor
    {
        public NativeArray<Vector3> samplePositions;
        public NativeArray<int> closestIndexToPlayer;
        [ReadOnly] public Vector3 playerPosition;
        public NativeArray<float> closestDistance;

        public void Execute(int index)
        {
            float distance = Vector3.Distance(samplePositions[index], playerPosition);
            if (distance < closestDistance[0])
            {
                closestDistance[0] = distance;
                closestIndexToPlayer[0] = index;
            }
        }

       
    }
}