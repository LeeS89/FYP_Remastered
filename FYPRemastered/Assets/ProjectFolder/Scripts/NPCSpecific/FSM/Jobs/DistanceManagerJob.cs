using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class DistanceManagerJob : IDistanceService, ITickable
{
    private NativeList<int> _subscriberIds;
    private List<ITargetable> _targets = new(25);
    //private NativeList<Transform> _subscriberIdszz;
    private NativeList<Vector3> _subscriberPositions;
    private NativeList<Vector3> _subscriberTargetPositions;
    //private NativeList<float> _bufferMultipliers;
    private NativeList<float> _initialDistances;
    private NativeList<float> _currentDistances;
    private NativeList<bool> _hasInitialized;

    private Dictionary<int, int> _subscriberIndexMap = new();
    private Dictionary<int, Action<float, float>> _subscriberCallbacks = new();
    private int _nextSubscriberId = 0;

    private float _jobInterval = 0.2f;
    private float _nextJobTime = 0f;
    private Queue<int> _removeQueue = new Queue<int>();
    
    public DistanceManagerJob()
    {
        const int preallocate = 128;
        _subscriberIds = new NativeList<int>(preallocate, Allocator.Persistent);
        _subscriberPositions = new NativeList<Vector3>(preallocate, Allocator.Persistent);
        _subscriberTargetPositions = new NativeList<Vector3>(preallocate, Allocator.Persistent);
      
        _initialDistances = new NativeList<float>(preallocate, Allocator.Persistent);
        _currentDistances = new NativeList<float>(preallocate, Allocator.Persistent);
        _hasInitialized = new NativeList<bool>(preallocate, Allocator.Persistent);

    }

    public void Dispose()
    {
        if(_subscriberPositions.IsCreated) _subscriberPositions.Dispose();
        if(_subscriberIds.IsCreated) _subscriberIds.Dispose();
        if (_subscriberTargetPositions.IsCreated) _subscriberTargetPositions.Dispose();
    
        if(_initialDistances.IsCreated) _initialDistances.Dispose();
        if(_currentDistances.IsCreated) _currentDistances.Dispose();
        if (_hasInitialized.IsCreated) _hasInitialized.Dispose();
    }

    public void Tick(float dt)
    {
        if (!_subscriberPositions.IsCreated || _subscriberPositions.Length == 0) return;
        if (Time.time >= _nextJobTime)
        {
            RunDistanceCheckJob();
            _nextJobTime = Time.time + _jobInterval;
        }
    }

    // Unused interface function
    public void LateTick(float dt) { }

    public int RegisterSubscriber(Vector3 position, ITargetable target/*Vector3 targetPosiiton*/, float bufferMultiplier, Action<float, float> callback)
    {
        int subscriberId = _nextSubscriberId++;
        int index = _subscriberPositions.Length;
        _subscriberPositions.Add(position);
        _targets.Add(target);
       // _subscriberTargetPositions.Add(targetPosiiton);
   
        _initialDistances.Add(0f);
        _currentDistances.Add(0f);
        _hasInitialized.Add(false);
        _subscriberIds.Add(subscriberId);

        _subscriberIndexMap[subscriberId] = index;
        _subscriberCallbacks[subscriberId] = callback;
        return subscriberId;
    }

    public void UnregisterSubscriber(int subscriberId) => _removeQueue.Enqueue(subscriberId);


    private void RemoveAtIndex(int index)
    {
        int lastIndex = _subscriberPositions.Length - 1;

        if (index != lastIndex)
        {
            int movedSubscriberId = _subscriberIds[lastIndex];

            _subscriberPositions[index] = _subscriberPositions[lastIndex];
            _subscriberTargetPositions[index] = _subscriberTargetPositions[lastIndex];
      
            _initialDistances[index] = _initialDistances[lastIndex];
            _currentDistances[index] = _currentDistances[lastIndex];
            _hasInitialized[index] = _hasInitialized[lastIndex];
            _subscriberIds[index] = movedSubscriberId;

            _subscriberIndexMap[movedSubscriberId] = index;
        }

        _subscriberPositions.RemoveAt(lastIndex);
        _subscriberTargetPositions.RemoveAt(lastIndex);
      
        _initialDistances.RemoveAt(lastIndex);
        _currentDistances.RemoveAt(lastIndex);
        _hasInitialized.RemoveAt(lastIndex);
        _subscriberIds.RemoveAt(lastIndex);

    }

    private void SafeRemove()
    {
        if (_removeQueue == null || _removeQueue.Count == 0) return;

        while (_removeQueue.Count > 0)
        {
            int subscriberId = _removeQueue.Dequeue();
            if (_subscriberIndexMap.TryGetValue(subscriberId, out int index))
            {
                RemoveAtIndex(index);
                _subscriberIndexMap.Remove(subscriberId);
                _subscriberCallbacks.Remove(subscriberId);
               
            }
        }
    }

    private void RunDistanceCheckJob()
    {
        //Debug.LogError("Running Distance Check Job");
        SafeRemove();
        if (_subscriberPositions.Length == 0 || _targets.Count == 0) return;

        for (int i = 0; i < _subscriberPositions.Length; i++)
        {
            _subscriberTargetPositions[i] = _targets[i].Position();
        }

        var distanceJob = new DistanceCheckJob
        {
            SubscriberPositions = _subscriberPositions.AsDeferredJobArray(),
            SubscriberTargetPositions = _subscriberTargetPositions.AsDeferredJobArray(),
            InitialDistances = _initialDistances.AsDeferredJobArray(),
            CurrentDistances = _currentDistances.AsDeferredJobArray(),
            HasInitialized = _hasInitialized.AsDeferredJobArray()
        };
        JobHandle jobHandle = distanceJob.Schedule(_subscriberPositions.Length, 64);
        jobHandle.Complete();
        // Invoke callbacks

        foreach (var kvp in _subscriberIndexMap)
        {
            int subscriberId = kvp.Key;
            int index = kvp.Value;

            if (_subscriberCallbacks.TryGetValue(subscriberId, out var callback))
            {
                float initialDistance = _initialDistances[index];
                float currentDistance = _currentDistances[index];
                callback?.Invoke(initialDistance, currentDistance);
            }
        }

        SafeRemove();
    }

    [BurstCompile]
    private struct DistanceCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> SubscriberPositions;
        [ReadOnly] public NativeArray<Vector3> SubscriberTargetPositions;
        [WriteOnly] public NativeArray<float> InitialDistances;
        [WriteOnly] public NativeArray<float> CurrentDistances;
        public NativeArray<bool> HasInitialized;
        public void Execute(int index)
        {
            Vector3 subscriberPos = SubscriberPositions[index];
            Vector3 targetPos = SubscriberTargetPositions[index];
            float distance = Vector3.Distance(subscriberPos, targetPos);
            if (!HasInitialized[index])
            {
                InitialDistances[index] = distance;
                HasInitialized[index] = true;
            }
            CurrentDistances[index] = distance;
        }
    }


}



