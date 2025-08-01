using UnityEngine;

public interface IParticleHitReceiver
{
    void OnParticleHit(ParticleSystem.Particle particle);
}
