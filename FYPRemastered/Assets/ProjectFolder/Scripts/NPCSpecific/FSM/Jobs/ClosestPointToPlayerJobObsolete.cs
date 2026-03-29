using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[Obsolete("",true)]
public class ClosestPointToPlayerJobObsolete : SceneResourcesObsolete
{
    //public static ClosestPointToPlayerJob Instance { get; private set; }

    public UniformZoneGridManagerObsolete zoneGridManager;

    
    private NativeList<Vector3> _samplePositions;
    private NativeArray<float> _threadDistances;
    private NativeArray<int> _threadIndices;
    private SamplePointDataSO _samplePointData;

    public ClosestPointToPlayerJobObsolete(SamplePointDataSO samplePointData)
    {
        _samplePointData = samplePointData;
        AddSamplePointData(_samplePointData);
        SceneEventAggregatorObsolete.Instance.OnRunClosestPointToPlayerJob += RunClosestPointJob;
    }


    public void AddSamplePointData(SamplePointDataSO sampleData)
    {
       
        _samplePositions = new NativeList<Vector3>(5000, Allocator.Persistent);
        
        foreach (var pos in sampleData.savedPoints)
        {
            _samplePositions.Add(pos.position);
        }

        int length = _samplePositions.Length;
       
        _threadDistances = new NativeArray<float>(length, Allocator.Persistent);
        _threadIndices = new NativeArray<int>(length, Allocator.Persistent);
        
    }

    /*public bool _testRun = false;
    private void Update()
    {
        if(_testRun)
        {
            RunClosestPointJob();
            _testRun = false;
        }
    }*/

    public void RunClosestPointJob()
    {
        using (var sampleArray = _samplePositions.ToArray(Allocator.TempJob))
        {
            var job = new ClosestPointJob
            {
                samplePositions = sampleArray,
                playerPosition = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position,
                threadDistances = _threadDistances,
                threadIndices = _threadIndices
            };

            var jobHandle = job.Schedule(sampleArray.Length, 64);
            jobHandle.Complete();

            float min = float.MaxValue;
            int minIndex = -1;

            for (int i = 0; i < _threadDistances.Length; i++)
            {
                if (_threadDistances[i] < min)
                {
                    min = _threadDistances[i];
                    minIndex = i;
                }
            }

           // BaseSceneManager._instance.ClosestPointToPlayerJobComplete(minIndex);
            SceneEventAggregatorObsolete.Instance.ClosestFlankPointToPlayerJobComplete(minIndex);
            //return minIndex;
            //zoneGridManager.SetNearestIndexToPlayer(minIndex);
        }
    }

    public void Dispose()
    {
        if (_samplePositions.IsCreated)
            _samplePositions.Dispose();
        if (_threadDistances.IsCreated)
            _threadDistances.Dispose();
        if (_threadIndices.IsCreated)
            _threadIndices.Dispose();
    }



    [BurstCompile]
    public struct ClosestPointJob : IJobParallelFor
    {
        public NativeArray<Vector3> samplePositions;
        [ReadOnly] public Vector3 playerPosition;
        
        public NativeArray<float> threadDistances;
        public NativeArray<int> threadIndices;

        public void Execute(int index)
        {
            float distance = Vector3.Distance(samplePositions[index], playerPosition);
            threadDistances[index] = distance;
            threadIndices[index] = index;
            
        }

       
    }
}












public sealed class ClosestPointToPlayerJobNew : /*SceneResources,*/ IClosestIndexService, IListInitializable<FlankPointData>, ITickable
{

    private SamplePointDataSO _samplePointData;
    private NativeList<Vector3> _samplePositions;
    private NativeArray<int> _closestIndices;
    private NativeArray<Vector3> _targetPositions;

    private readonly List<PendingRequest> _pendingRequests = new(20);
    private readonly List<PendingRequest> _activeRequests = new(20);
    private bool _jobScheduled;
    private JobHandle _jobHandle;

    public ClosestPointToPlayerJobNew() { }

    public ClosestPointToPlayerJobNew(SamplePointDataSO samplePointData)
    {
        _samplePointData = samplePointData;
        int count = samplePointData.savedPoints.Count;
        _samplePositions = new NativeList<Vector3>(count, Allocator.Persistent);

        foreach (var pos in _samplePointData.savedPoints)
            _samplePositions.Add(pos.position);
    }



    public bool TryInit(IReadOnlyList<FlankPointData> data)
    {
        if (data == null || data.Count == 0) { DebugLogs.ArgNotNull(data, "flank point data", this); return false; }

        int count = data.Count;
        _samplePositions = new NativeList<Vector3>(count, Allocator.Persistent);

        foreach(var point in data)
            _samplePositions.Add(point.position);

        DebugLogs.Log($"Successfully constructed player point job: [{count}]", this);
        return true;
    }


    public void RequestClosestIndex(int id, Vector3 targetPosition, Action<int, int, bool> OnRequestComplete)
    {
        if(!_samplePositions.IsCreated || _samplePositions.Length == 0)
        {
            OnRequestComplete?.Invoke(id, -1, false);
            return;
        }

        _pendingRequests.Add(new PendingRequest
        {
            RequestId = id,
            TargetPosition = targetPosition,
            OnRequestComplete = OnRequestComplete
        });
    }

    public void Tick(float dt)
    {
        if(!_jobScheduled && _pendingRequests.Count > 0)
            ScheduleJob();

        if(_jobScheduled && _jobHandle.IsCompleted)
        {
            _jobHandle.Complete();
            DispatchResults();
            _jobScheduled = false;
        }
    }

    private void ScheduleJob()
    {
        _activeRequests.Clear();
        _activeRequests.AddRange(_pendingRequests);
        _pendingRequests.Clear();

        int count = _activeRequests.Count;
        if (count == 0) return;


        EnsureCapacity(ref _targetPositions, count);
        EnsureCapacity(ref _closestIndices, count);

        for(int i = 0; i < count; i++)
            _targetPositions[i] = _pendingRequests[i].TargetPosition;

        var job = new ClosestIndexJob
        {
            SamplePositions = _samplePositions.AsArray(),
            TargetPositions = _targetPositions,
            ClosestIndices = _closestIndices
        };

        _jobHandle = job.Schedule(count, 64);
        _jobScheduled = true;
    }

    private void DispatchResults()
    {
        int count = _activeRequests.Count;
        for(int i = 0; i < count; i++)
        {
            var request = _pendingRequests[i];
            int closestIndex = _closestIndices[i];
            request.OnRequestComplete?.Invoke(request.RequestId, closestIndex, closestIndex >= 0);
        }
        _activeRequests.Clear();
    }   

    private static void EnsureCapacity<T>(ref NativeArray<T> array, int needed) where T : struct
    {
        if(!array.IsCreated || array.Length < needed)
        {
            if(array.IsCreated) array.Dispose();
            array = new NativeArray<T>(needed, Allocator.Persistent);
        }
    }

    public void Dispose()
    {
        if (_samplePositions.IsCreated)
            _samplePositions.Dispose();
        if (_closestIndices.IsCreated)
            _closestIndices.Dispose();
        if (_targetPositions.IsCreated)
            _targetPositions.Dispose();
    }


    private struct PendingRequest
    {
        public int RequestId;
        public Vector3 TargetPosition;
        public Action<int, int, bool> OnRequestComplete;
    }

    
    [BurstCompile]
    public struct ClosestIndexJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> SamplePositions;
        [ReadOnly] public NativeArray<Vector3> TargetPositions;

        public NativeArray<int> ClosestIndices;

        public void Execute(int index)
        {
            Vector3 target = TargetPositions[index];

            float bestDistance = float.MaxValue;
            int bestIndex = -1;

            for (int i = 0; i < SamplePositions.Length; i++)
            {
                Vector3 sample = SamplePositions[i];
                Vector3 delta = sample - target;
                float distanceSq = delta.sqrMagnitude;
                if (distanceSq < bestDistance)
                {
                    bestDistance = distanceSq;
                    bestIndex = i;
                }
            }
            ClosestIndices[index] = bestIndex;
        }
    }


    public void LateTick(float dt) { }

   
}