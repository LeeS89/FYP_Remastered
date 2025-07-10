using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class StationaryChaseManagerJob : MonoBehaviour
{
    public static StationaryChaseManagerJob Instance { get; private set; }

    private NativeList<Vector3> _agentPositions;
    private NativeList<float> _bufferMultipliers;
    private NativeList<float> _baseDistances;
    private NativeList<bool> _hasInitialized;
    private NativeList<bool> _shouldReChase;

    private Dictionary<int, int> _agentIndexMap = new();
    private Dictionary<int, EnemyState> _agentStates = new();
    private int _nextAgentId = 0;

    private float _jobInterval = 0.2f;
    private float _nextJobTime = 0f;
    private Queue<int> _removeQueue = new Queue<int>();

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        const int preallocate = 128;
        _agentPositions = new NativeList<Vector3>(preallocate, Allocator.Persistent);
        _bufferMultipliers = new NativeList<float>(preallocate, Allocator.Persistent);
        _baseDistances = new NativeList<float>(preallocate, Allocator.Persistent);
        _hasInitialized = new NativeList<bool>(preallocate, Allocator.Persistent);
        _shouldReChase = new NativeList<bool>(preallocate, Allocator.Persistent);
    }

    private void OnDestroy()
    {
        _agentPositions.Dispose();
        _bufferMultipliers.Dispose();
        _baseDistances.Dispose();
        _hasInitialized.Dispose();
        _shouldReChase.Dispose();
    }

    private void Update()
    {
        if (Time.time >= _nextJobTime && _agentPositions.Length > 0)
        {
            RunDistanceCheckJob();
            _nextJobTime = Time.time + _jobInterval;
        }
    }

    public int RegisterAgent(Vector3 position, float bufferMultiplier, EnemyState state)
    {
        int agentId = _nextAgentId++;
        int index = _agentPositions.Length;

        _agentPositions.Add(position);
        _bufferMultipliers.Add(bufferMultiplier);
        _baseDistances.Add(0f); // Set in job
        _hasInitialized.Add(false);
        _shouldReChase.Add(false);

        _agentIndexMap[agentId] = index;
        _agentStates[agentId] = state;
        

        return agentId;
    }

    private void RemoveAgentAtIndex(int index)
    {
        int lastIndex = _agentPositions.Length - 1;

        // Swap last item into the removed index
        _agentPositions[index] = _agentPositions[lastIndex];
        _bufferMultipliers[index] = _bufferMultipliers[lastIndex];
        _baseDistances[index] = _baseDistances[lastIndex];
        _hasInitialized[index] = _hasInitialized[lastIndex];
        _shouldReChase[index] = _shouldReChase[lastIndex];

        _agentPositions.RemoveAt(lastIndex);
        _bufferMultipliers.RemoveAt(lastIndex);
        _baseDistances.RemoveAt(lastIndex);
        _hasInitialized.RemoveAt(lastIndex);
        _shouldReChase.RemoveAt(lastIndex);

        // Fix index map
        foreach (var kvp in _agentIndexMap)
        {
            if (kvp.Value == lastIndex)
            {
                _agentIndexMap[kvp.Key] = index;
                break;
            }
        }
    }

    public void UpdateAgentPosition(int agentId, Vector3 position)
    {
        if (_agentIndexMap.TryGetValue(agentId, out int index))
            _agentPositions[index] = position;
    }

    public void UnregisterAgent(int agentId)
    {
        _removeQueue.Enqueue(agentId);

        
    }

    private void SafeRemove()
    {
        if(_removeQueue.Count == 0) { return; }

        while (_removeQueue.Count > 0)
        {
            int agentId = _removeQueue.Dequeue();
            

            if (_agentIndexMap.TryGetValue(agentId, out int index))
            {
                RemoveAgentAtIndex(index);
                _agentIndexMap.Remove(agentId);
                _agentStates.Remove(agentId);
            }
        }
    }

    private void RunDistanceCheckJob()
    {
        var job = new StationaryChaseCheckJob
        {
            agentPositions = _agentPositions.AsDeferredJobArray(),
            bufferMultipliers = _bufferMultipliers.AsDeferredJobArray(),
            baseDistances = _baseDistances.AsDeferredJobArray(),
            hasInitialized = _hasInitialized.AsDeferredJobArray(),
            shouldReChase = _shouldReChase.AsDeferredJobArray(),
            playerPosition = GameManager.Instance.GetPlayerPosition(PlayerPart.Position).position
        };

        JobHandle handle = job.Schedule(_agentPositions.Length, 32);
        handle.Complete();

        // Push result to agent states
        foreach (var kvp in _agentIndexMap)
        {
            int agentId = kvp.Key;
            int index = kvp.Value;

            if (_agentStates.TryGetValue(agentId, out var state))
            {
                state.SetChaseEligibility(_shouldReChase[index], job.playerPosition);
            }
        }
        SafeRemove();
    }


    [BurstCompile]
    public struct StationaryChaseCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> agentPositions;
        [ReadOnly] public NativeArray<float> bufferMultipliers;

        public NativeArray<float> baseDistances;
        public NativeArray<bool> hasInitialized;
        public NativeArray<bool> shouldReChase;

        [ReadOnly] public Vector3 playerPosition;

        public void Execute(int index)
        {
            //Debug.LogError("Executing job for index: " + index);
            float currentDistance = Vector3.Distance(agentPositions[index], playerPosition);
            
            if (!hasInitialized[index])
            {
                baseDistances[index] = currentDistance;
                hasInitialized[index] = true;
                shouldReChase[index] = false;
                return;
            }

            float threshold = baseDistances[index] * bufferMultipliers[index];
            //Debug.LogError("Current distance: " + currentDistance + " and threshold: " + threshold);
            shouldReChase[index] = currentDistance > threshold;
        }
    }
}
