using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
   
    private static ParticleSystem bulletParticleSystem;

    void Start()
    {
        // Find the shared particle system only once
        if (bulletParticleSystem == null)
        {
            bulletParticleSystem = GameObject.Find("BulletParticleSystem").GetComponent<ParticleSystem>();
        }
    }

    public void Fire(Vector3 position)
    {
        transform.position = position;

        // Emit particles from the shared system at the bullet’s position
        var emitParams = new ParticleSystem.EmitParams();
        emitParams.position = position;
        bulletParticleSystem.Emit(emitParams, 10); // Emit 10 particles per bullet
    }

    /*public Mesh bulletMesh; // Assign your bullet capsule mesh
    public Material bulletMaterial; // Assign the material
    public ParticleSystem bulletTrailParticle; // Assign the particle system prefab
    public int maxBullets = 100;

    private List<BulletData> bullets = new List<BulletData>();

    struct BulletData
    {
        public Vector3 position;
        public Vector3 velocity;
        public ParticleSystem trailInstance; // Store the particle system instance
    }

    void Update()
    {
        // Update bullet positions
        for (int i = 0; i < bullets.Count; i++)
        {
            BulletData bullet = bullets[i];
            bullet.position += bullet.velocity * Time.deltaTime;

            // Update the particle system position
            if (bullet.trailInstance != null)
            {
                bullet.trailInstance.transform.position = bullet.position;
            }

            bullets[i] = bullet;
        }

        // Render bullets using GPU Instancing
        RenderBullets();
    }

    public void ShootBullet(Vector3 position, Vector3 direction, float speed)
    {
        if (bullets.Count >= maxBullets) return; // Don't exceed max bullets

        // Create a new bullet
        BulletData bullet = new BulletData
        {
            position = position,
            velocity = direction * speed,
            trailInstance = Instantiate(bulletTrailParticle, position, Quaternion.identity)
        };

        bullets.Add(bullet);
    }

    void RenderBullets()
    {
        if (bullets.Count == 0) return;

        Matrix4x4[] matrices = new Matrix4x4[bullets.Count];

        for (int i = 0; i < bullets.Count; i++)
        {
            matrices[i] = Matrix4x4.TRS(bullets[i].position, Quaternion.identity, Vector3.one * 0.1f);
        }

        Graphics.DrawMeshInstanced(bulletMesh, 0, bulletMaterial, matrices);
    }*/
}
