using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class BulletDistanceJob : MonoBehaviour
{
    public static BulletDistanceJob Instance { get; private set; }

    [SerializeField] private Transform _player;

    private NativeArray<Vector3> _bulletPositions;
    private NativeArray<float> _distances;
    private List<Projectile> _frozenBullets = new List<Projectile>();
    private CalculateDistanceJob _job;
    [SerializeField] private int _maxbullets = 100;
    private float _jobCooldown = 0.1f;
    private float _nextJobTime = 0f;

    // NEW
    private List<DeflectableProjectile> _frozenProjectiles = new List<DeflectableProjectile>();
    // END NEW

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        _bulletPositions = new NativeArray<Vector3>(_maxbullets, Allocator.Persistent);
        _distances = new NativeArray<float>(_maxbullets, Allocator.Persistent);

        _job = new CalculateDistanceJob()
        {
            _bulletPositions = _bulletPositions,
            _distances = _distances
        };
    }

   
    void Update()
    {
        if (Time.time >= _nextJobTime && _frozenBullets.Count > 0)
        {
            _nextJobTime = Time.time + _jobCooldown;
            for (int i = 0; i < _frozenBullets.Count; i++)
            {
                _bulletPositions[i] = _frozenBullets[i].transform.position;
            }

            _job._playerPosition = _player.position;
            var jobHandle = _job.Schedule();
            jobHandle.Complete();

            for (int i = 0; i < _frozenBullets.Count; i++)
            {
                _frozenBullets[i].SetDistanceToPlayer(_distances[i]);
            }
        }
    }

    // NEW
    public bool AddFrozenProjectile(DeflectableProjectile projectile)
    {
        if (!_frozenProjectiles.Contains(projectile))
        {
            _frozenProjectiles.Add(projectile);
            return true;
        }
        return false;
    }
    public bool RemoveFrozenProjectile(DeflectableProjectile projectile)
    {
        if (_frozenProjectiles.Contains(projectile))
        {
            _frozenProjectiles.Remove(projectile);
            return true;
        }
        return false;
    }

    // END NEW

    public bool AddFrozenBullet(Projectile bullet)
    {
        if (!_frozenBullets.Contains(bullet))
        {
            _frozenBullets.Add(bullet);
            return true;
        }
        return false;
    }

    public bool RemoveFrozenBullet(Projectile bullet)
    {
        if (_frozenBullets.Contains(bullet))
        {
            _frozenBullets.Remove(bullet);
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if(_bulletPositions.IsCreated) _bulletPositions.Dispose();

        if(_distances.IsCreated) _distances.Dispose();

        if(Instance == this)
        {
            Instance = null;
        }
    }

    [BurstCompile]
    public struct CalculateDistanceJob : IJob
    {
        public Vector3 _playerPosition;
        [ReadOnly] public NativeArray<Vector3> _bulletPositions;
        public NativeArray<float> _distances;

        public void Execute()
        {
            //Debug.LogError("Execute Called");
            for(int i = 0; i < _bulletPositions.Length; i++)
            {
                _distances[i] = Mathf.Sqrt(
                    Mathf.Pow(_playerPosition.x - _bulletPositions[i].x, 2) +
                    Mathf.Pow(_playerPosition.y - _bulletPositions[i].y, 2) +
                    Mathf.Pow(_playerPosition.z - _bulletPositions[i].z, 2)
                );
            }
        }
    }
}
