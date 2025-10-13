using UnityEngine;
using UnityEngine.Pool;
using System;

namespace Greyrat.StarBlaster
{
    public class PooledRigidbodyObject : MonoBehaviour
    {
        [HideInInspector] public IObjectPool<PooledRigidbodyObject> pool;
        public Rigidbody rb;

        public event Action<Collision, PooledRigidbodyObject> OnCollisionEnterEvent;

        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEnterEvent?.Invoke(collision, this);
        }

        public void ReturnToPool()
        {
            if (pool != null)
            {
                pool.Release(this);
            }
        }
    }
}