using UnityEngine;

namespace Greyrat.StarBlaster
{
    public class ParticlePooledObject : MonoBehaviour
    {
        public ParticleSystem pooledParticleSystem;

        private void Awake()
        {
            if (pooledParticleSystem == null)
            {
                pooledParticleSystem = GetComponent<ParticleSystem>();
            }
        }
    }
}