using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManagers : MonoBehaviour
{
    public static BulletManagers instance;
    private ParticleSystem.Particle[] particles;

    private void Awake()
    {
        instance = this;
    }

    public ParticleSystem _particleSystem; // Reference to the global particle system
    public List<Bullet> activeBullets; // List of active bullets

    private ParticleSystem.EmitParams emitParams; // Reusable struct


   /* void Start()
    {
        if (_particleSystem == null)
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        // Ensure particles have a long lifetime so they don't disappear
        var mainModule = _particleSystem.main;
        mainModule.startLifetime = Mathf.Infinity; // Or a very high value, like 99999f

        // Pre-warm particles
        _particleSystem.Emit(activeBullets.Count);

        // Ensure buffer is large enough
        int maxParticles = _particleSystem.main.maxParticles;
        particles = new ParticleSystem.Particle[maxParticles];

        // Get particles immediately
        _particleSystem.GetParticles(particles);
    }*/


    public void Removebullet(Bullet bullet)
    {
        activeBullets.Remove(bullet);
    }

    void Update()
    {

        int bulletCount = activeBullets.Count;

        if (bulletCount == 0) return;

        // Ensure we have enough particle slots
        if (particles == null || particles.Length < bulletCount)
        {
            particles = new ParticleSystem.Particle[bulletCount];
        }

        // Get existing particles (optional but helps maintain settings)
        int particleCount = _particleSystem.GetParticles(particles);

        // Update each particle position based on bullet positions
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
        /*  int particleCount = _particleSystem.particleCount;
          if (particleCount == 0) return;

          _particleSystem.GetParticles(particles); // Get active particles

          for (int i = 0; i < activeBullets.Count && i < particleCount; i++)
          {
              // ✅ Only update position — DO NOT modify other particle properties
              particles[i].position = activeBullets[i].transform.localPosition;
          }

          _particleSystem.SetParticles(particles, particleCount); // Apply updated positions*/
        /* foreach (var bullet in activeBullets)
         {
             ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams(); // Ensure it's reset
             emitParams.position = bullet.transform.position;

             _particleSystem.Emit(emitParams, 1);
         }*/

        //OLDDDD NEWWWWWW
        /*foreach (var bullet in activeBullets)
        {
            // Set the position dynamically
            emitParams.position = bullet.transform.localPosition;

            // Emit the particle using the pre-allocated struct
            _particleSystem.Emit(emitParams, 1);
        }*/
    }

    /*public void UpdateParticleOnBullet(Vector3 position)
    {
        emitParams.position = position;

        // Emit the particle using the pre-allocated struct
        _particleSystem.Emit(emitParams, 1);
    }*/

    /*private static ParticleSystem bulletParticleSystem;
    private static List<Bullet> activeBullets = new List<Bullet>();
    private ParticleSystem.Particle[] particles;
    private int particleIndex;
    private bool isActive = false;

    void Start()
    {
        if (bulletParticleSystem == null)
        {
            bulletParticleSystem = GameObject.Find("BulletParticleSystem").GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire(transform.position, transform.forward);
        }
    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.forward = direction;
        isActive = true;
        activeBullets.Add(this);

        if (!bulletParticleSystem.isPlaying)
        {
            bulletParticleSystem.Play();
        }

        // Emit a new particle and track it
        EmitNewParticle();

        StartCoroutine(TrackBullet());
    }

    private void EmitNewParticle()
    {
        if (bulletParticleSystem != null)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = transform.position,
                velocity = Vector3.zero // No initial movement
            };

            bulletParticleSystem.Emit(emitParams, 1);

            int particleCount = bulletParticleSystem.particleCount;
            if (particles == null || particles.Length < particleCount)
            {
                particles = new ParticleSystem.Particle[particleCount];
            }

            bulletParticleSystem.GetParticles(particles);
            particleIndex = particleCount - 1; // Store index of this bullet's particle
        }
    }

    private IEnumerator TrackBullet()
    {
        while (isActive)
        {
            int particleCount = bulletParticleSystem.particleCount;
            if (particleCount > 0 && particleIndex < particleCount)
            {
                particles[particleIndex].position = transform.position;
                bulletParticleSystem.SetParticles(particles, particleCount);
            }

            yield return null;
        }
    }

    public void Deactivate()
    {
        isActive = false;
        activeBullets.Remove(this);
    }*/

    /*private static ParticleSystem globalParticleSystem;
    private ParticleSystem.Particle[] particles;
    private bool isActive = false;
    private int particleIndex;  // Track which particle belongs to this bullet

    void Start()
    {
        if (globalParticleSystem == null)
        {
            globalParticleSystem = GameObject.Find("BulletParticleSystem").GetComponent<ParticleSystem>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire(transform.position, transform.forward);
        }
    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.forward = direction;
        isActive = true;

        if (!globalParticleSystem.isPlaying)
        {
            globalParticleSystem.Play();
        }

        // Emit a new particle and store its index
        EmitNewParticle();

        // Start tracking the bullet
        StartCoroutine(TrackBullet());
    }

    private void EmitNewParticle()
    {
        if (globalParticleSystem != null)
        {
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = transform.position;
            emitParams.velocity = Vector3.zero; // No initial movement
            globalParticleSystem.Emit(emitParams, 1); // Emit one particle per bullet

            // Get latest particle count and assume last particle is the new one
            int particleCount = globalParticleSystem.particleCount;
            if (particles == null || particles.Length < particleCount)
            {
                particles = new ParticleSystem.Particle[particleCount];
            }
            globalParticleSystem.GetParticles(particles);
            particleIndex = particleCount - 1; // Store index of the newest particle
        }
    }

    private IEnumerator TrackBullet()
    {
        while (isActive)
        {
            int particleCount = globalParticleSystem.particleCount;
            if (particleCount > 0 && particleIndex < particleCount)
            {
                // Move this bullet's assigned particle to its position
                particles[particleIndex].position = transform.position;
                globalParticleSystem.SetParticles(particles, particleCount);
            }

            yield return null;
        }
    }

    public void Deactivate()
    {
        isActive = false;
    }
*/

    /*private static ParticleSystem bulletParticleSystem;
    private ParticleSystem.Particle[] particles;
    private bool isActive = false;

    void Start()
    {
        if (bulletParticleSystem == null)
        {
            bulletParticleSystem = GameObject.Find("BulletParticleSystem").GetComponent<ParticleSystem>();
        }

    }

    public void Fire(Vector3 position, Vector3 direction)
    {
        transform.position = position;
        transform.forward = direction;
        isActive = true;

        if (!bulletParticleSystem.isPlaying)
        {
            bulletParticleSystem.Play();
        }
        // Attach particle system to this bullet
        StartCoroutine(TrackBullet());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Fire(transform.position, transform.forward);
        }
    }

    private IEnumerator TrackBullet()
    {
        while (isActive)
        {
            // Get the current particles
            int particleCount = bulletParticleSystem.particleCount;
            if (particles == null || particles.Length < particleCount)
            {
                particles = new ParticleSystem.Particle[particleCount];
            }

            bulletParticleSystem.GetParticles(particles);

            // Move particles to the bullet’s position
            for (int i = 0; i < particleCount; i++)
            {
                particles[i].position = transform.position;
            }

            bulletParticleSystem.SetParticles(particles, particleCount);

            yield return null; 
        }
    }

    public void Deactivate()
    {
        isActive = false;
    }*/
}
