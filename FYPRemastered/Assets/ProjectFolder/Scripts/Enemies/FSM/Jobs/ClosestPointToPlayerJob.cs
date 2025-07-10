using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class ClosestPointToPlayerJob : SceneResources
{
    //public static ClosestPointToPlayerJob Instance { get; private set; }

    public UniformZoneGridManager zoneGridManager;

    
    private NativeList<Vector3> _samplePositions;
    private NativeArray<float> _threadDistances;
    private NativeArray<int> _threadIndices;
    private SamplePointDataSO _samplePointData;

    public ClosestPointToPlayerJob(SamplePointDataSO samplePointData)
    {
        _samplePointData = samplePointData;
        AddSamplePointData(_samplePointData);
        SceneEventAggregator.Instance.OnRunClosestPointToPlayerJob += RunClosestPointJob;
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
            SceneEventAggregator.Instance.ClosestFlankPointToPlayerJobComplete(minIndex);
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

   /* private void OnDestroy()
    {
        if (_samplePositions.IsCreated)
            _samplePositions.Dispose();
        if (_threadDistances.IsCreated)
            _threadDistances.Dispose();
        if (_threadIndices.IsCreated)
            _threadIndices.Dispose();
    }*/

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