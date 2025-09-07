using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;

    [Header("Main Particle System")]
    public ParticleSystem _particleSystem;

    [Header("Tracked Bullets")]
    public List<Projectile> activeBullets;

    private ParticleSystem.Particle[] particles;
    private bool _particlesExist = false;

    private void Awake()
    {
        instance = this;
    }

    public void AddBullet(Projectile bullet)
    {
        if (!activeBullets.Contains(bullet))
        {
            activeBullets.Add(bullet);
            _particlesExist = true;

            // Emit a particle immediately for this bullet
            _particleSystem.Emit(1);
        }
    }

    public void RemoveBullet(Projectile bullet)
    {
        activeBullets.Remove(bullet);
        if (activeBullets.Count <= 0)
        {
            _particlesExist = false;
        }
    }

    private void LateUpdate() // Changed to LateUpdate for better sync with rendered positions
    {
        if (_particlesExist)
        {
            UpdateMovingParticles();
        }
    }

    private void UpdateMovingParticles()
    {
        int bulletCount = activeBullets.Count;
        if (bulletCount == 0) return;

        // Ensure particles array is large enough
        if (particles == null || particles.Length < bulletCount)
        {
            particles = new ParticleSystem.Particle[bulletCount];
        }

        int particleCount = _particleSystem.GetParticles(particles);

        // Ensure enough particles exist
        if (particleCount < bulletCount)
        {
            int toEmit = bulletCount - particleCount;
            _particleSystem.Emit(toEmit);
            particleCount += toEmit;

            // Refresh buffer
            _particleSystem.GetParticles(particles);
        }

        for (int i = 0; i < bulletCount; i++)
        {
            particles[i].position = activeBullets[i].transform.position;
        }

        _particleSystem.SetParticles(particles, particleCount);
    }
}



/*using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instance;
    private ParticleSystem.Particle[] particles;
    private bool _particlesExist = false;

    private void Awake()
    {
        instance = this;
    }

    public ParticleSystem _particleSystem;
    public List<BulletBase> activeBullets;

    public void AddBullet(BulletBase bullet*//*, BulletType bulletType*//*)
    {
        if(!activeBullets.Contains(bullet))
        {
            activeBullets.Add(bullet);
            if(!_particlesExist)
            {
                _particlesExist = true;
            }
        }
    }

    public void Removebullet(BulletBase bullet)
    {
        activeBullets.Remove(bullet);
        if(activeBullets.Count <= 0 )
        {
            _particlesExist = false;

        }
    }

    void FixedUpdate()
    {
        if (!_particlesExist) { return; }

        UpdateMovingParticles();

    }

    private void UpdateMovingParticles()
    {
        int bulletCount = activeBullets.Count;

        if (bulletCount == 0) return;


        if (particles == null || particles.Length < bulletCount)
        {
            particles = new ParticleSystem.Particle[bulletCount];
        }


        int particleCount = _particleSystem.GetParticles(particles);


        for (int i = 0; i < bulletCount; i++)
        {
            if (i >= particleCount) // If there aren’t enough active particles, emit new ones
            {
                _particleSystem.Emit(1);
                _particleSystem.GetParticles(particles); // Refresh the particles list
                particleCount = _particleSystem.particleCount; // Update count
            }

            particles[i].position = activeBullets[i].transform.position;
        }

        // Apply the updated positions
        _particleSystem.SetParticles(particles, particleCount);
    }


}
*/