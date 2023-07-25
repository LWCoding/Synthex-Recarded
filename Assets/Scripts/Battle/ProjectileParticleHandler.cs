using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ParticleType
{
    NONE = 0, SPIT_PARTICLES = 1, SMOKE_PARTICLES = 2, HEAL_PARTICLES = 3
}

public enum ParticleBurstType
{
    NONE = 0, ANGLED_BURST_TO_TARGET = 1, ANGLED_BURST_UP = 2, OUTWARDS_FROM_CENTER = 3
}

[System.Serializable]
public class ParticleInfo
{
    public ParticleType particleType = ParticleType.NONE;
    public ParticleBurstType particleBurstType = ParticleBurstType.NONE;
    public Vector2 particleScale = new Vector2(1, 1);
    public float particleLifetime = 0.8f;
    public float particleSpeed = 6f;
    public int particleCount = 10;
    public Color particleColor = Color.white;
    public bool affectedByGravity = false;
}

[RequireComponent(typeof(ParticleSystem))]
public class ProjectileParticleHandler : MonoBehaviour
{

    public Material spitParticleMaterial;
    public Material smokeParticleMaterial;
    public Material healParticleMaterial;
    private ParticleSystem _pSystem;
    private ParticleSystemRenderer _pRenderer;

    private void Awake()
    {
        _pSystem = GetComponent<ParticleSystem>();
        _pRenderer = GetComponent<ParticleSystemRenderer>();
    }

    private void InitializeParticle(ParticleInfo particleInfo, int burstDirectionX)
    {
        ParticleSystem.MainModule pSystemMain = _pSystem.main;
        // Set the color of the particle.
        pSystemMain.startColor = particleInfo.particleColor;
        // Set the lifetime of the particle.
        pSystemMain.startLifetime = particleInfo.particleLifetime;
        // Set the velocity of the particle.
        pSystemMain.startSpeed = particleInfo.particleSpeed;
        // Set the gravity scale of the particle.
        pSystemMain.gravityModifier = (particleInfo.affectedByGravity) ? 0.5f : 0;
        // Switch up the shape of the particles based on the ParticleType.
        switch (particleInfo.particleType)
        {
            case ParticleType.SPIT_PARTICLES:
                _pRenderer.GetComponent<ParticleSystemRenderer>().material = spitParticleMaterial;
                break;
            case ParticleType.SMOKE_PARTICLES:
                _pRenderer.GetComponent<ParticleSystemRenderer>().material = smokeParticleMaterial;
                break;
            case ParticleType.HEAL_PARTICLES:
                _pRenderer.GetComponent<ParticleSystemRenderer>().material = healParticleMaterial;
                break;
        }
        // Switch up the burst type of the particles based on the ParticleBurstType.
        ParticleSystem.ShapeModule shapeModule = _pSystem.shape;
        transform.localScale = new Vector2(burstDirectionX, 1);
        switch (particleInfo.particleBurstType)
        {
            case ParticleBurstType.ANGLED_BURST_TO_TARGET:
                shapeModule.shapeType = ParticleSystemShapeType.Cone;
                shapeModule.angle = 38.0f;
                shapeModule.radius = 0.25f;
                shapeModule.rotation = new Vector3(0, burstDirectionX * 90, 0);
                break;
            case ParticleBurstType.ANGLED_BURST_UP:
                shapeModule.shapeType = ParticleSystemShapeType.Cone;
                shapeModule.angle = 38.0f;
                shapeModule.radius = 0.25f;
                shapeModule.rotation = new Vector3(-90, 0, 0);
                break;
            case ParticleBurstType.OUTWARDS_FROM_CENTER:
                shapeModule.shapeType = ParticleSystemShapeType.Sphere;
                shapeModule.radius = 0f;
                shapeModule.rotation = new Vector3(0, 0, 0);
                break;
        }
    }

    // Summons a particle. Can optionally run code afterwards.
    public void SummonParticle(ParticleInfo particleInfo, Vector3 spawnPosition, int burstDirectionX, Action<GameObject> codeToRunAfter = null)
    {
        InitializeParticle(particleInfo, burstDirectionX);
        _pSystem.transform.position = spawnPosition;
        _pSystem.transform.localScale = particleInfo.particleScale;
        _pSystem.Emit(particleInfo.particleCount);
        if (codeToRunAfter != null)
        {
            StartCoroutine(StartParticleCoroutine(codeToRunAfter));
        }
    }

    private IEnumerator StartParticleCoroutine(Action<GameObject> codeToRunAfter)
    {
        yield return new WaitForSeconds(_pSystem.main.startLifetime.constant);
        codeToRunAfter.Invoke(gameObject);
    }

}
